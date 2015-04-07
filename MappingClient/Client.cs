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
        private String codeN;

        public Client(String mapname, String code) 
        {
            this.mapName = mapname;
            this.codeN = code;

            TcpChannel channel = new TcpChannel(8087);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemote), "client", WellKnownObjectMode.Singleton);

  
            this.tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:8086/tracker");

            try
            {
                this.tracker.SendMapper(File.ReadAllBytes(codeN), mapName);
                this.tracker.submitJob(null,@"C:\Users\ruijosepereira\Desktop\ola.txt",2,5);
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

        public static void setFile(string filename) 
        {
            ClientRemote.lines = File.ReadAllLines(filename);
        }

        public String[] getSplit(int lower, int higher)
        {
            if (lines == null) return null;
         
            String[] splitFile = new String[higher - lower];

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

            String folderPath = "MapOutputs"; 

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
