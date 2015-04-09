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
        private string url;

        public Worker(int id, string url, string trackerUrl) 
        {
            Worker.id = id;
            this.url = url;
            this.start(trackerUrl);
        }


        private int getPort()
        {
            return int.Parse(this.url.Split(':')[2].Split('/')[0]);
        }
        public static int getId(){
            return Worker.id;
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

                    ((IJobTracker)Activator.GetObject(typeof(IJobTracker), entryUrl)).connect(getId(),this.url);
                    Console.WriteLine("Listening on: "+ this.url); ;
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
            //string cmd = Console.ReadLine();

            new Worker(1, "tcp://localhost:30001/W", null);
            //if (cmd.Equals("1")) 
            //else if (cmd.Equals("2")) new Worker(2, "tcp://localhost:30002/W", "tcp://localhost:30001/W");
            //else new Worker(3, "tcp://localhost:30003/W", "tcp://localhost:30001/W");
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
        private int delay = 0;

        private int id;


        public WorkRemote()
        {
            this.id = Worker.getId();
            client = ((IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:10001/C"));
            slaves.Add(new KeyValuePair<int, IWorker>(id, this));   //add myself
        }

        public void keepWorkingThread(string map, string filename, WorkStruct job)
        {
            ISet<KeyValuePair<String, String>> megaList = new HashSet<KeyValuePair<String, String>>();
            String[] splits;
            IJobTracker tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:30001/W");

            do
            {
                megaList.Clear();
                splits = client.getSplit(job.lower, job.higher);
                if (splits == null) break;

                foreach (String s in splits)
                {
                    megaList.UnionWith(mapObj.map(s));
                }


                int del = getDelay();

                if (del > 0)
                {
                    Thread.Sleep(del * 1000);
                }
                Console.WriteLine("Did: " + job.id);
                client.storeSplit(megaList,job.id);
                job = tracker.hazWorkz();


            } while (job.id != -1);
            tracker.join(); // when no more jobs at tracker
        }


        public string printStatus()
        {
            return "alive";
        }

        public void addDelay(int seconds) //delay worker
        {
            lock(this)
            {
                this.delay += seconds;
            }
        }

        private int getDelay()
        {
            lock(this)
            {
                int del = this.delay;
                this.delay = 0;
                return del;
            }

        }

        public void startSplit(string map, string filename, WorkStruct job)
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
