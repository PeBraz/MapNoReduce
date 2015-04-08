using System;
using System.IO;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using PADIMapNoReduce;

namespace PADIMapNoReduce
{

    public class Worker
    {

        private static string endpoint = "tracker";

        public Worker(int port) {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkRemote), "W", WellKnownObjectMode.Singleton);

            System.Console.WriteLine("Press <enter> to terminate...");
            System.Console.ReadLine();
        }

        [STAThread]
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(JobTracker), "tracker", WellKnownObjectMode.Singleton);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkRemote), "worker", WellKnownObjectMode.Singleton);

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

    class WorkRemote : MarshalByRefObject, IWorker
    {
        private IClient client;
        private IList<KeyValuePair<String,String>> map;
        private Map mapObj;

        public WorkRemote()
        {
            client = ((IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:8087/client"));
        }

        public void keepWorkingThread( IMap map, string filename, WorkStruct job)
        {
            ISet<KeyValuePair<String, String>> megaList = new HashSet<KeyValuePair<String, String>>();
            String[] splits;
            IJobTracker tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:8086/tracker");

            do
            {
                megaList.Clear();
                splits = client.getSplit(job.lower, job.higher);
                if (splits == null) break;

                foreach (String s in splits)
                {
                    megaList.UnionWith(mapObj.map(s));
                }
                client.storeSplit(megaList,job.id);
                job = tracker.hazWorkz();
            

            } while (job.id != -1);
            tracker.join(); // when no more jobs at tracker
        }

        public void startSplit(IMap map, string filename, WorkStruct job)
        {
             new Thread(() => keepWorkingThread(map, filename, job)).Start();
        }

        public void SendMapper(byte[] code, string className)
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

    class JobTracker : MarshalByRefObject, IJobTracker
    {

        public JobTracker()
        {
            slaves.Add(((IWorker)Activator.GetObject(typeof(IWorker), "tcp://localhost:8086/worker")));
        }

        private int done = 0;   //temporary used by join() to signal worker has no more work to do
        private Queue queue = new Queue();
        private List<IWorker> slaves = new List<IWorker>();


        public void submitJob(IMap map, string filename, int numSplits, int numberOfLines)
        {

            int numSlaves = this.slaves.Count;
            int step = numberOfLines / numSplits;
            int remainder = numberOfLines % numSplits;

            for (int i = 0, index = 0; i < numSplits; i++, index+=step + ((remainder > 0)?1:0))
            {
                WorkStruct ws = new WorkStruct();
                ws.id = i;
                ws.lower = index;
                ws.higher = index + step + ((remainder > 0)?1:0);
                queue.Enqueue(ws);
                remainder--;
            }

            foreach (IWorker slave in slaves)
            {
               slave.startSplit(map, filename, (WorkStruct)queue.Dequeue());
            }


            while (done < numSlaves)
            {
                Thread.Sleep(1000);
            }
            done = 0;
        }

        public void SendMapper(byte[] code, String className)
        {
            foreach (IWorker slave in slaves)
                slave.SendMapper(code,className);
        }

        public void join() {
            this.done++;
        }


        public WorkStruct hazWorkz()
        {
            lock (this)
            {
                return queue.Count == 0 ? new WorkStruct(0, 0, -1) : (WorkStruct)queue.Dequeue();
            }
        }
    }
}
