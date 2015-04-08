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

        void sloww(int workerId, int secs);

        //void freezew(int workerID);

        //void unfreezew(int workerID);

        //void freezec(int workerID);

        //void unfreezec(int workerID);
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

        private IList<KeyValuePair<int,int>> workers = new List<KeyValuePair<int,int>>();

        public void startWorker(int workerId, int port, string targetWorker) 
        {
            workers.Add(new KeyValuePair<int, int>(workerId,port));
            new PADIMapNoReduce.Worker(port);
            //blocked by Worker
        }

        public void submitAJob(string targetWorker, string inputFilePath, string outputDir, int numOfSplits, string mapClass)
        {
            //TODO((IWorker)Activator.GetObject(typeof(IWorker),targetWorker)).startSplit();
            //
        }

        public void wait(int secs) {
            Thread.Sleep(secs * 1000);
        }

        public void status() {
            foreach (KeyValuePair<int, int> k in workers) 
            {
               //((IWorker)Activator.GetObject(typeof(IWorker), "localhost:" + k.Value + "/W"));// .printStatus();
            }

        }

        public void sloww(int workerID, int seconds) 
        {
            foreach (KeyValuePair<int, int> k in workers)
            {
                if(k.Key == workerID){
                    ((IWorker)Activator.GetObject(typeof(IWorker), "localhost:" + k.Value + "/W")).addDelay(seconds);
                    break;
                }
            }
        }

    }
}
