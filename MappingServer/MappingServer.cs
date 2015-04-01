using System;
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

            System.Console.WriteLine("Press <enter> to terminate chat server...");
            System.Console.ReadLine();
        }
    }


    class JobTracker : MarshalByRefObject, IJobTracker
    {

        List<IWorker> slaves = new List<IWorker>();

        public void submitJob(IMap map, string filename, int numSplits, string outputFile)
        {
            int splitId = 1;
            int numLines = ((IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:8086/client")).numberOfFileLines();
            int step = numLines / numSplits;
            int index = 0;

            foreach (IWorker slave in slaves)
            {
                slave.startSplit(map, filename, index, index + step, splitId++);
                index += step;
            }
        }

        public bool hazWorkz()
        {
            return false;
        }
    }
}
