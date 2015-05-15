using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net.Sockets;
using PADIMapNoReduce;

namespace PADIMapNoReduce
{
    public class Client
    {
        public static IClient me;
        public static string addr;
        public static string trackerUrl;
        public Client(int id)
        {
            
            TcpChannel channel = new TcpChannel(10000 + id);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemote), "C", WellKnownObjectMode.Singleton);
            Client.addr = "tcp://localhost:" + (10000 + id).ToString() + "/C";
            Client.me = (IClient)Activator.GetObject(typeof(IClient), Client.addr);

        }

        public void init(int trackerId) 
        {
            Client.trackerUrl = "tcp://localhost:" + (30000 + trackerId).ToString() + "/W";
        
        }


        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
                new Client(int.Parse(args[0]));
            else 
            {
                Console.Write("Pick my id >>  ");
                new Client(int.Parse(Console.ReadLine().Trim()));

                while (true)
                {
                    Console.WriteLine("Choose a worker port to start a test job [ex: 30001]: ");

                    Client.me.newJob("tcp://localhost:" + Console.ReadLine().Trim() + "/W",
                                    @"..\log.txt",
                                    "Outputs",
                                    100,
                                    "ParadiseCountMapper",
                                    "Mapper.dll");

                }

            }
            System.Console.WriteLine("Press <enter> to terminate...");
            System.Console.ReadLine();
        }
        

    }

    class ClientRemote : MarshalByRefObject, IClient
    {

        private IDictionary<string,string[]> files = new Dictionary<string,string[]>();
        private IDictionary<string,string> outputs = new Dictionary<string,string>();
 
        /*
         *  The Client does not know which file the split belongs to 
         * 
         * 
         * 
         */


        public String[] getSplit(string filename, int lower, int higher)
        {
            string[] lines = this.files[filename];

            //Console.WriteLine("lower: " + lower + "; higher: " + higher + "; lines.length: " + lines.Length);
            int arraySize = higher > lines.Length ? lines.Length - lower : higher - lower;
            String[] splitFile = new String[arraySize];

            for (int i = lower, index = 0; i < higher && i < lines.Length; i++, index++)
            {
                splitFile[index] = lines[i];
            }
            return splitFile;
        }


        public void storeSplit(string filename, IList<KeyValuePair<String, String>> set, int splitID)
        {
            string outputDir = outputs[filename];
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            using (StreamWriter file = new StreamWriter(outputDir + splitID.ToString() + ".out"))
            {
                foreach (KeyValuePair<String, String> line in set)
                {
                    file.WriteLine(line.Key + "|" + line.Value);
                }
            }
        }

        /**
         *  The process of starting a job has 2 phases
         *  In the first phase the tracker gets all metadada and code to be used needed for job execution
         *  In the second phase the job is submitted
         *  
         */

        public bool newJob(string trackerUrl, string inputFilePath, string outputDir, int numOfSplits,string mapClass,string mapDll)
        {
            IJobTracker tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), trackerUrl);
            try
            {
                if (!outputDir.EndsWith("\\")) outputDir += "\\";

                this.openFile(inputFilePath);
                int id = tracker.sendMeta(Client.addr, inputFilePath, this.fileLinesCount(inputFilePath), mapClass,File.ReadAllBytes(mapDll));
                this.outputs[inputFilePath] = outputDir;

                tracker.submitJob(id, numOfSplits);
                this.closeFile(inputFilePath);
                Console.WriteLine("<Job ended>");
                return true;
            }
            catch (SocketException)
            {
                return false;
            }

        }
        public void openFile(string filename) 
        {
            this.files.Add(filename, File.ReadAllLines(filename));
        }
        public int fileLinesCount(string filename)
        {
            return this.files[filename].Length;
        }

        public void closeFile(string filename)
        {
            this.files.Remove(filename);
        }
    }
}
