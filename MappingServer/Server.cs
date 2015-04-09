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
        public static int PORT = 30000;

        private static int id;
        private int masterId = 1;
        private int port;
        private static string endpoint = "tracker";

        public Worker(int id) {

            setId(id);
            this.port = Worker.PORT + getId();

            this.start();
        }
        
        private void start()
        {
            try
            {
                if (getId() == masterId)
                {
                    Console.WriteLine("I am Master at 30001");
                    TcpChannel channel = new TcpChannel(Worker.PORT + getId());
                    ChannelServices.RegisterChannel(channel, false);
                    RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkRemote), "W", WellKnownObjectMode.Singleton);
                }
                else {
                    string masterUrl = "tcp://localhost:" + (PORT + this.masterId).ToString() + "/W";
                    ((IJobTracker)Activator.GetObject(typeof(IJobTracker), masterUrl)).connect(getId());
                
                
                }
            }
            catch (SocketException)
            {
                Console.Write("Failed to start on url: tcp://localhost:" + port + "/W");
                System.Environment.Exit(1);
            }
        }
        public static int getId() 
        {
            return Worker.id;
        }
        public static void setId(int id) 
        {
            Worker.id = id;
        }

        [STAThread]
        static void Main(string[] args)
        {
            Worker w = new Worker(1);

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
            slaves.Add(new KeyValuePair<int, IWorker>(id, this));   //add nyself
        }

        public void keepWorkingThread(IMap map, string filename, WorkStruct job)
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

        public void startSplit(IMap map, string filename, WorkStruct job)
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
