using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        public Client(String mapname, String codePath) 
        {
            this.mapName = mapname;
   

            TcpChannel channel = new TcpChannel(8087);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemote), "client", WellKnownObjectMode.Singleton);

  
            this.tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:8086/tracker");

            try
            {
                this.tracker.SendMapper(File.ReadAllBytes(codePath), "Map");
                int numLines = ClientRemote.setFile(this.inputFilePath);
                this.tracker.submitJob(null, this.inputFilePath, 5, numLines);
            }
            catch (SocketException)
            {
                System.Console.WriteLine("Could not locate server");
            }
            Console.WriteLine("KARANNNNN");
            Console.ReadLine();
        }
          
        public void setFile(string filename)
        {
            ClientRemote.setFile(filename);
        }
        public void submitJob(IMap map, string filename, int numSplits, int numberOfLines) 
        { 
            this.tracker.submitJob( map, filename, numSplits, numberOfLines);
        }
    }

    class ClientRemote : MarshalByRefObject, IClient
    {

       private static String[] lines = null;

        public static int setFile(string filename) 
        {
            ClientRemote.lines = File.ReadAllLines(filename);
            return ClientRemote.lines.Length;
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
        public int numberOfFileLines() {
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
