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

namespace API
{
    class Client 
    {
        private IJobTracker tracker;
        private String mapName;

        private string inputFilePath = @"..\..\..\ola.txt"; 


        public Client(String mapname, String codePath, int id) 
        {
            this.mapName = mapname;
   

            TcpChannel channel = new TcpChannel(10000 + id);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemote), "C", WellKnownObjectMode.Singleton);

  
            this.tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:30001/W");

            this.newJob(codePath, inputFilePath, 3);
         
            Console.WriteLine("<Success>");
            Console.ReadLine();
        }



        public void newJob(string codePath, string inputFilePath, int numOfSplits) 
        {
            while (true)
            {
                try
                {
                    this.tracker.SendMapper(File.ReadAllBytes(codePath), "Map");
                    int numLines = ClientRemote.setFile(this.inputFilePath);
                    this.tracker.submitJob(null, inputFilePath, numOfSplits, numLines);
                    break;
                }
                catch (SocketException)
                {
                    System.Console.WriteLine("Could not locate server. Retrying...");
                    Thread.Sleep(1000);
                }
            }
        }


    }

    class ClientRemote : MarshalByRefObject, IClient
    {

       private static String[] lines = null;

        public static int setFile(string filename) 
        {
            ClientRemote.lines = File.ReadAllLines(filename);
            return ClientRemote.numberOfFileLines();
        }

        public String[] getSplit(int lower, int higher)
        {
            if (lines == null) return null;
         
            int arraySize = higher > lines.Length? lines.Length - lower : higher - lower;
            String[] splitFile = new String[arraySize];

            for (int i=lower, index = 0; i < higher && i < lines.Length; i++ , index++)
            {
                splitFile[index] = ClientRemote.lines[i];
            }
            return splitFile;
        }    
        private static int numberOfFileLines() {
            return (lines != null) ? lines.Length : 0;
        }

        public void storeSplit(ISet<KeyValuePair<String, String>> set, int splitID)
        {

            String folderPath = @"MapOutputs\"; 

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            using (StreamWriter file = new StreamWriter(folderPath + splitID.ToString() + ".out"))
            {
                foreach (KeyValuePair<String, String> line in set)
                {
                    file.WriteLine(line.Key + "|" + line.Value);
                }
            }
        }
    }
}
