using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using PADIMapNoReduce;

namespace API
{
    class Client 
    {
        private IJobTracker tracker;

        public Client() 
        {
            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemote), "client", WellKnownObjectMode.Singleton);

            this.tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:8086/tracker");

        }
          
        public void setFile(string filename)
        {
            ClientRemote.setFile(filename);
        }
        public void submitJob(IMap map, string filename, int numSplits, string outputFile) 
        { 
            this.tracker.submitJob( map, filename, numSplits, outputFile);
        }
    }

    class ClientRemote : MarshalByRefObject, IClient
    {

       private static String[] lines = null;

        public static void setFile(string filename) 
        {
            ClientRemote.lines = File.ReadAllLines(filename);
        }

        public  String[] getSplit(int lower, int higher)
        {
            if (lines != null) return null;
         
            String[] splitFile = new String[higher - lower];

            for (int i=lower, index = 0; i < higher || i < lines.Length; i++ , index++)
            {
                splitFile[index] = ClientRemote.lines[i];
            }
            return splitFile;
        }    
        public int numberOfFileLines() {
            return (lines != null) ? lines.Length : 0;
        }
    }
}
