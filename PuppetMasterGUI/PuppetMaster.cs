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
using System.Collections;
using System.Runtime.Serialization.Formatters;
using System.Diagnostics;
using System.Net.Sockets;

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

        void freezec(int id);

        void unfreezec(int id);

        void checkMachines();
    }

    class PuppetMaster
    {
        private IPuppetMaster me;

        public PuppetMaster(int id)
        {


            TcpChannel channel = new TcpChannel(20000 + id);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(PuppetMasterRemote), "PM", WellKnownObjectMode.Singleton);

            this.me = ((IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), "tcp://localhost:" +(20000 + id).ToString() +"/PM"));
           
        }

        public void readFile(string filename)
        {
            String[] lines = File.ReadAllLines(filename);

            foreach (var line in lines)
            {
                parse(line);
            }
        }



        public void parse(string line)
        {
            if (line.Length == 0) return;

            String[] words = line.Split(' ');
            int size = words.Length - 1;

            string cmd = words[0].ToLower();
            if (cmd.ElementAt(0).Equals('%')) return;

            if (cmd.Equals("worker"))
            {
                startWorker(int.Parse(words[1]), words[2], words[3], size == 4 ? words[4] : null);
            }
            else if (cmd.Equals("submit"))
            {
                this.me.submitAJob(words[1], words[2], words[3], int.Parse(words[4]), words[5], words[6]);
            }
            else if (cmd.Equals("wait"))
            {
                this.me.wait(int.Parse(words[1]));
            }
            else if (cmd.Equals("status"))
            {
                this.me.checkMachines();
            }
            else if (cmd.Equals("sloww"))
            {
                this.me.sloww(int.Parse(words[1]), int.Parse(words[2]));
            }
            else if (cmd.Equals("freezew"))
            {
                this.me.freezew(int.Parse(words[1]));
            }
            else if (cmd.Equals("unfreezew"))
            {
                this.me.unfreezew(int.Parse(words[1]));
            }
            else if (cmd.Equals("freezec"))
            {
                this.me.freezec(int.Parse(words[1]));
            }
            else if (cmd.Equals("unfreezec"))
            {
                this.me.unfreezec(int.Parse(words[1]));
            }
            else
            {
                Console.WriteLine("erro");
                MessageBox.Show("Error: The command is incorrect");
            }
        }


        public void startWorker(int id, string pmUrl, string serviceUrl, string entryUrl)
        {
            IPuppetMaster goodLife = ((IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), pmUrl));
            goodLife.startWorker(id, serviceUrl, entryUrl);
        }

        public static void initClientProcess() 
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"Client.exe";
            startInfo.Arguments = "1";
            Process.Start(startInfo);
        }

        public static void initWorkerProcess(int id, string url, string trackerUrl)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"ChatServer.exe";
            startInfo.Arguments = id.ToString() + " " + url + " " + (trackerUrl!=null ? trackerUrl : "");
            Process.Start(startInfo);
        }

    }

    public class PuppetMasterRemote : MarshalByRefObject, IPuppetMaster
    {

        private IList<KeyValuePair<int, IWorker>> workers = new List<KeyValuePair<int, IWorker>>();
        private IClient client = null;

        public void startWorker(int workerId, string url, string targetWorker)
        {

            PuppetMaster.initWorkerProcess(workers.Count,url, targetWorker);
            workers.Add(new KeyValuePair<int, IWorker>(workerId, (IWorker)Activator.GetObject(typeof(IWorker), url)));
        }

        public void submitAJob(string targetWorker, string inputFilePath, string outputDir, int numOfSplits, string mapClass, string mapDll)
        {
            int id = 1;


            PuppetMaster.initClientProcess();
            this.client = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + (10000 + id).ToString() + "/C");
            //this call is blocking, call it in a thread to call other commands

            new Thread(() => client.newJob(targetWorker, inputFilePath, outputDir, numOfSplits, mapClass, mapDll)).Start();


        }

        public void wait(int secs)
        {
            Thread.Sleep(secs * 1000);
        }

        public void sloww(int id, int seconds) 
        {
            IWorker w = getWorker(id);
            if (w != null)
                new Thread(()=>w.delay(seconds)).Start();
            else
                Console.WriteLine("Worker not found");
        
        }
        public void freezew(int id) {
            IWorker w = getWorker(id);
            if (w != null)
               new Thread(()=> w.freezeWorker()).Start();
            else
                Console.WriteLine("Worker not found");
        }
        public void unfreezew(int id) 
        {
            IWorker w = getWorker(id);
            if (w != null)
                new Thread(()=>w.unfreezeWorker()).Start();
            else
                Console.WriteLine("Worker not found");
        }


        public void freezec(int id)
        {
            IJobTracker t = (IJobTracker)getWorker(id);
            if (t != null)
                new Thread(() => t.freezeTracker()).Start();
            else
                Console.WriteLine("Treacker not found");

        }

        public void unfreezec(int id)
        {
            IJobTracker t = (IJobTracker)getWorker(id);
            if (t != null)
                new Thread(() => t.unfreezeTracker()).Start();
            else
                Console.WriteLine("Tracker not found");
        }

        private IWorker getWorker(int id) 
        {
            foreach (KeyValuePair<int,IWorker> worker in this.workers) 
            {
                if (worker.Key == id) return worker.Value;
            }
            return null;
        }


        /** 
         * Checks who is working and makes the tracker and workers print their status
         */
        public void checkMachines() 
        {
            String text = "";
            foreach (KeyValuePair<int, IWorker> worker in workers)
            {
                try
                {
                    worker.Value.printStatus();
                    text += worker.Key.ToString() + " - OK ";
                }
                catch (SocketException)
                {
                    text += worker.Key.ToString() + " - DOWN ";
                }

            }
        }
    }
}
