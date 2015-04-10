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
using System.IO;


namespace PADIMapNoReduce
{
    public interface IPuppetMaster
    {
        void startWorker(int workerId, string WorkerUrl, string targetWorker);

        void submitAJob(string targetWorker, string inputFilePath, string outputDir, int numOfSplits, string mapClass, string mapDll);

        void wait(int seconds);

        void sloww(int id, int seconds);

        void freezew(int id);

        void unfreezew(int id);

        //void status();
    }

    class PuppetMaster
    {
        private IPuppetMaster me;

        public PuppetMaster(int id)
        {

            TcpChannel channel = new TcpChannel(10000 + id);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(PuppetMasterRemote), "PM", WellKnownObjectMode.Singleton);

            this.me = ((IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), "tcp://localhost:" +(30000 + id).ToString() +"/PM"));

            Console.WriteLine("Enter to exit");
            Console.ReadLine();
        }

        public void parse(string line)
        {
            String[] words = line.Split(' ');
            int size = words.Length - 1;

            if (words[0].ElementAt(0).Equals("%")) return;

            else if (words[0].Equals("worker"))
            {
                startWorker(int.Parse(words[1]), words[2], words[3], size == 4 ? words[4] : null);
            }
            else if (words[0].Equals("submit"))
            {
                this.me.submitAJob(words[1], words[2], words[3], int.Parse(words[4]), words[5], words[6]);
            }
            else if (words[0].Equals("wait"))
            {
                this.me.wait(int.Parse(words[1]));
            }
            else if (words[0].Equals("status"))
            {
                
            }
            else if (words[0].Equals("sloww"))
            {
                this.me.sloww(int.Parse(words[1]), int.Parse(words[2]));
            }
            else if (words[0].Equals("freezew"))
            {
                this.me.freezew(int.Parse(words[1]));
            }
            else if (words[0].Equals("unfreezew"))
            {
                this.me.unfreezew(int.Parse(words[1]));
            }
            else if (words[0].Equals("freezec"))
            {
                //freezec(int.Parse(words[1]));
            }
            else if (words[0].Equals("unfreezec"))
            {
                //unfreezec(int.Parse(words[1]));
            }
            else Console.WriteLine("erro");
        }

        public void readFile(string filename)
        {
            String[] lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                parse(line.ToLower());
            }
        }

        public void startWorker(int id, string pmUrl, string serviceUrl, string entryUrl)
        {
            IPuppetMaster goodLife = ((IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), pmUrl));
            goodLife.startWorker(id, serviceUrl, entryUrl);
        }

    }

    public class PuppetMasterRemote : MarshalByRefObject, IPuppetMaster
    {

        private IList<KeyValuePair<int, IWorker>> workers = new List<KeyValuePair<int, IWorker>>();

        public void startWorker(int workerId, string url, string targetWorker)
        {
            workers.Add(new KeyValuePair<int, IWorker>(workerId, (IWorker)new Worker(workerId,url,targetWorker)));
        }

        public void submitAJob(string targetWorker, string inputFilePath, string outputDir, int numOfSplits, string mapClass, string mapDll)
        {
            int id = 1;
            new Client(id);

            IClient client = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + (10000 + id).ToString() + "/C");
            client.newJob(targetWorker, inputFilePath, outputDir, numOfSplits, mapClass, mapDll);

        }

        public void wait(int secs)
        {
            Thread.Sleep(secs * 1000);
        }

        public void sloww(int id, int seconds) 
        {
            IWorker w = getWorker(id);
            if (w != null)
                w.delay(seconds);
            else
                Console.WriteLine("Worker not found");
        
        }
        public void freezew(int id) {
            IWorker w = getWorker(id);
            if (w != null)
                w.freeze();
            else
                Console.WriteLine("Worker not found");
        }
        public void unfreezew(int id) 
        {
            IWorker w = getWorker(id);
            if (w != null)
                w.unfreeze();
            else
                Console.WriteLine("Worker not found");
        }

        private IWorker getWorker(int id) 
        {
            foreach (KeyValuePair<int,IWorker> worker in this.workers) 
            {
                if (worker.Key == id) return worker.Value;
            }
            return null;
        }

    }
}
