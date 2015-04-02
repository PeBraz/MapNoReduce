using System;
using System.IO;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections.Generic;

using PADIMapNoReduce;

namespace Mapping
{

    class Worker
    {

        private static string endpoint = "tracker";

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

    class WorkRemote : MarshalByRefObject, IWorker
    {
        private IClient client;
        private IList<KeyValuePair<String,String>> map;


        public WorkRemote()
        {
            client = ((IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:8086/client"));
        }

        public void keepWorkingThread( IMap map, string filename, WorkStruct job)
        {
            ISet<KeyValuePair<String, String>> megaList = new HashSet<KeyValuePair<String, String>>();
            String[] splits;
            do
            {
                megaList.Clear();
                splits = client.getSplit(job.lower, job.higher);
                foreach (String s in splits)
                {
                    megaList.UnionWith(map.Map(s));
                }
                client.storeSplit(megaList,job.id);
                job = ((IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:8086/tracker")).hazWorkz();
                
            } while (job.id != -1);
        }

        public void startSplit(IMap map, string filename, WorkStruct job)
        {
             new Thread(() => keepWorkingThread(map, filename, job)).Start();
        }
    }


    class JobTracker : MarshalByRefObject, IJobTracker
    {
        private const Queue queue = new Queue();
        private const List<IWorker> slaves = new List<IWorker>();


        public void submitJob(IMap map, string filename, int numSplits, int numberOfLines)
        {

            slaves.Add(((IWorker)Activator.GetObject(typeof(IWorker), "tcp://localhost:8086/tracker")));
 
            int step = numberOfLines / numSplits;

            for (int i = 0, index = 0; i < numSplits; i++, index+=step)
            {
                WorkStruct ws = new WorkStruct();
                ws.id = i;
                ws.lower = index;
                ws.higher = index + step;
                queue.Enqueue(ws);
            }

            foreach (IWorker slave in slaves)
            {
               slave.startSplit(map, filename, (WorkStruct)queue.Dequeue());
            }

            while(queue.Count != 0)
            {
                Thread.Sleep(1000);
            }
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
