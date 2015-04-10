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
using System.Windows.Forms;


namespace PADIMapNoReduce
{
    public interface IPuppetMaster
    {
        void startWorker(int workerId, string WorkerUrl, string targetWorker);

        void submitAJob(string targetWorker, string inputFilePath, string outputDir, int numOfSplits, string mapClass, string mapDll);

        void wait(int seconds);

        //void status();
    }

    class PuppetMaster
    {
        private IPuppetMaster me;

        public PuppetMaster()
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(PuppetMasterRemote), "PM", WellKnownObjectMode.Singleton);

            this.me = ((IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), "tcp://localhost:20001/PM"));

            Console.WriteLine("Enter to exit");
            Console.ReadLine();
        }

        public void readFile(string filename)
        {
            String[] lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                parse(line.ToLower());
            }
        }

        public void parse(string line)
        {
            if (line.Length == 0)
            {
                MessageBox.Show("Error: The command is empty");
                return;
            }
            String[] words = line.Split(' ');
            int size = words.Length - 1;

            if (words[0].ElementAt(0).Equals("%")) return;

            else if (words[0].Equals("worker"))
            {
                startWorker(int.Parse(words[1]), words[2], words[3], size == 4 ? words[4] : null);
            }
            else if (words[0].Equals("submit"))
            {
                //submit(words[1], words[2], words[3], words[4], words[5], words[6]);
            }
            else if (words[0].Equals("wait"))
            {
                //wait(words[1]);
            }
            else if (words[0].Equals("status"))
            {
                //status();
            }
            else if (words[0].Equals("sloww"))
            {
                //slow(int.Parse(words[1]));
            }
            else if (words[0].Equals("freezew"))
            {
                //freezew(int.Parse(words[1]));
            }
            else if (words[0].Equals("unfreezew"))
            {
                //unfreezew(int.Parse(words[1]));
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
            MessageBox.Show("Error: The command is incorrect");
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
            client.newJob(targetWorker, mapClass, mapDll, inputFilePath, outputDir, numOfSplits);

            //TODO((IWorker)Activator.GetObject(typeof(IWorker),targetWorker)).startSplit();
            //
        }

        public void wait(int secs)
        {
            Thread.Sleep(secs * 1000);
        }

    }
}
