using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using PADIMapNoReduce;
using System.Threading;


namespace PADIMapNoReduce
{
   public interface IPuppetMaster 
    {
        void startWorker(int workerId, int WorkerUrl, string targetWorker); 

        void submitAJob(string targetWorker, string inputFilePath, string outputDir, int numOfSplits, string mapClass);

        void wait(int seconds);

        void status();
    }

    class PuppetMaster
    {
        public PuppetMaster() 
        {

            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(PuppetMasterRemote), "PM", WellKnownObjectMode.Singleton);

            Console.WriteLine("Enter to exit");
            Console.ReadLine();
        }


    }
    class PuppetMasterRemote : MarshalByRefObject, IPuppetMaster
    {

        private int idCount = 1;
        private IList<KeyValuePair<int,IWorker>> slaves = new List<KeyValuePair<int,IWorker>>();
        private Client client;

        public PuppetMasterRemote() 
        {
            this.client = new Client(1);   
        }

        void startWorker(int workerId, string newWorkerUrl, string targetWorker) 
        {

            IWorker slave = (IWorker)new Worker(workerId, newWorkerUrl, targetWorker);
            slaves.Add(new KeyValuePair<int, IWorker>(workerId, slave));
          
        }

        void submitAJob(string targetWorker, string inputFilePath, string outputDir, int numOfSplits, string mapClass)
        {
            //TODO((IWorker)Activator.GetObject(typeof(IWorker),targetWorker)).startSplit();
            //
        }

        void wait(int secs) {
            Thread.Sleep(secs * 1000);
        }
        void status() {
            foreach (KeyValuePair<int, IWorker> slave in slaves) 
            {
                slave.Value.printStatus();
            }

        }
        void sloww() 
        { 
        
        }

    }
}
