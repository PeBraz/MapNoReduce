using System;
using System.IO;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Net.Sockets;
using PADIMapNoReduce;

namespace PADIMapNoReduce
{

    public class Worker
    {
        public static int id;
        public static bool amMaster = false;
        private static string url;

        public Worker(int id, string url, string trackerUrl) 
        {
            Worker.id = id;
            Worker.url = url;
            this.start(trackerUrl);
        }

        private int getPort()
        {
            return int.Parse(Worker.url.Split(':')[2].Split('/')[0]);
        }
        public static int getId(){
            return Worker.id;
        }
        public static string getUrl() 
        {
            return Worker.url;
        }
        private void start(string entryUrl)
        {
            TcpChannel channel = new TcpChannel(getPort());
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkRemote), "W", WellKnownObjectMode.Singleton);
           
            try
            {
                if (entryUrl == null)
                {
                    Worker.amMaster = true;
                    Console.WriteLine("I am Master at 30001");
                }
                else {
                    Console.WriteLine(entryUrl);
                    ((IJobTracker)Activator.GetObject(typeof(IJobTracker), entryUrl)).connect(getId(),getUrl());
                    Console.WriteLine("Listening on: "+ getUrl()); ;
                }
            }
            catch (SocketException)
            {
                Console.Write("Failed to start on url: " + url);
                System.Environment.Exit(1);
            }
        }


        [STAThread]
        static void Main(string[] args)
        {
            // 1st id of the worker
            // 2nd url of the new worker
            // 3rd (optional) tracker to connect

            if (args.Length == 2) 
                new Worker(int.Parse(args[0]), args[1], null);
            else if (args.Length == 3)
                new Worker(int.Parse(args[0]), args[1], args[2]);
            
            System.Console.WriteLine("Press <enter> to terminate...");
            System.Console.ReadLine();
        }
    }

    class Map
    {
        //public static delegate ISet<KeyValuePair<String, String>> mapDelegate(HashSet<KeyValuePair<String, String>> fun);
        private Type type;
        private object classObj;
        public Map(Type type) {
            this.classObj = Activator.CreateInstance(type);
            this.type = type;
        }

        public ISet<KeyValuePair<String, String>> map(string fileLine) {
            object[] args = new object[] { fileLine };
            return (ISet<KeyValuePair<String, String>>)
                    this.type.InvokeMember("map", BindingFlags.Default | BindingFlags.InvokeMethod, null, this.classObj, args);
        }
    }

    partial class WorkRemote : MarshalByRefObject, IWorker, IJobTracker
    {
        private IClient client;
        private IList<KeyValuePair<String, String>> map;
        private Map mapObj;

        private const string STATUS_IDLE = "idle";
        private const string STATUS_WORK = "working";
        private const string STATUS_BLOCKED = "blocked";


        public string status;

        private Object delayLock = new Object();
        private Object freezeLock = new Object();
        private bool frozen;
        private int id;


        public WorkRemote()
        {
            this.id = Worker.getId();
            client = ((IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:10001/C"));
            slaves.Add(new KeyValuePair<int, IWorker>(id, (IWorker)Activator.GetObject(typeof(IWorker), Worker.getUrl())));
            this.setStatus(STATUS_IDLE);
        }

        public void keepWorkingThread(string map, string filename, WorkStruct? job)
        {
            ISet<KeyValuePair<String, String>> megaList = new HashSet<KeyValuePair<String, String>>();
            String[] splits;
            IJobTracker tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:30001/W");
            this.setStatus(STATUS_WORK);

            while (job != null)
            {
            
      
                megaList.Clear();
                splits = client.getSplit(job.Value.lower, job.Value.higher);
                if (splits == null) break;

                foreach (String s in splits)
                {
                    megaList.UnionWith(mapObj.map(s));
                }


                lock (delayLock) {  };

                while (frozen) {
                    Thread.Sleep(100);
                } 
                
             
                Console.WriteLine("Did: " + job.Value.id);
                client.storeSplit(megaList,job.Value.id);
                job = tracker.hazWorkz();


            }

            tracker.join(); 
            this.setStatus(STATUS_IDLE);
        }

        public void setStatus(string status) 
        {
            this.status = status;
        }

        public void printStatus()
        {
            Console.WriteLine(String.Format("[{0} - {1}]\t{2}", this.id, Worker.getUrl(), this.status));
        }

        public void delay(int seconds)
        {

            lock (delayLock) {
                this.setStatus(STATUS_BLOCKED);
                Thread.Sleep(seconds * 1000); 
            }
            this.setStatus(STATUS_WORK);
        }


        public void freeze() 
        {
            frozen = true;
            this.setStatus(STATUS_BLOCKED);
        }

        public void unfreeze() 
        {
            frozen = false;
            this.setStatus(STATUS_WORK);
        }


        public void startSplit(string map, string filename, WorkStruct? job)
        {
            new Thread(() => keepWorkingThread(map, filename, job)).Start();
        }

        public void createMapper(byte[] code, string className)
        {
            Assembly assembly = Assembly.Load(code);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + className))
                    {
                        this.mapObj = new Map(type);
                    }
                }
            }
        }

    }

}
