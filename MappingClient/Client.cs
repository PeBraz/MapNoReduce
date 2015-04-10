﻿using System;
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
        private IClient me;

        private string inputFilePath = @"..\..\..\ola.txt";


        public Client(int id)
        {

            TcpChannel channel = new TcpChannel(10000 + id);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemote), "C", WellKnownObjectMode.Singleton);

        }

        [STAThread]
        static void Main(string[] args)
        {
            //string cmd = Console.ReadLine();

            new Client(1);
            IClient me = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + (10000 + 1).ToString() + "/C");
            bool success = false;
            do
            {
                success = me.newJob("tcp://localhost:30001/W", Client.path("illumhis.txt"), Client.path(@"Outputs\"), 1001, "Map", Client.path(@"Mapper\bin\Debug\Mapper.dll"));
                if (!success)
                    Console.WriteLine("Connection failed, retrying...");

            } while (!success);

            Console.WriteLine("<Success>");
            Console.ReadLine();
            //if (cmd.Equals("1")) 
            //else if (cmd.Equals("2")) new Worker(2, "tcp://localhost:30002/W", "tcp://localhost:30001/W");
            //else new Worker(3, "tcp://localhost:30003/W", "tcp://localhost:30001/W");
            System.Console.WriteLine("Press <enter> to terminate...");
            System.Console.ReadLine();
        }
        
        public static string path(string file)
        {
            return @"..\..\..\" + file;
        }

    }

    class ClientRemote : MarshalByRefObject, IClient
    {

        private static String[] lines = null;
        private IJobTracker tracker = null;
        private string outputDir;

        public static int setFile(string filename)
        {
            ClientRemote.lines = File.ReadAllLines(filename);
            return ClientRemote.numberOfFileLines();
        }

        public String[] getSplit(int lower, int higher)
        {
            if (lines == null) return null;

            int arraySize = higher > lines.Length ? lines.Length - lower : higher - lower;
            String[] splitFile = new String[arraySize];

            for (int i = lower, index = 0; i < higher && i < lines.Length; i++, index++)
            {
                splitFile[index] = ClientRemote.lines[i];
            }
            return splitFile;
        }

        private static int numberOfFileLines()
        {
            return (lines != null) ? lines.Length : 0;
        }

        public void storeSplit(ISet<KeyValuePair<String, String>> set, int splitID)
        {

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
         *  returns success of connecting to tracker
         */

        public bool newJob(string trackerUrl, string inputFilePath, string outputDir, int numOfSplits,string mapClass,string mapDll)
        {
            this.tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), trackerUrl);

            try
            {
                this.outputDir = outputDir;
                this.tracker.SendMapper(File.ReadAllBytes(mapDll), mapClass);
                int numLines = ClientRemote.setFile(inputFilePath);
                this.tracker.submitJob(mapClass, inputFilePath, numOfSplits, numLines);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }

        }


    }
}
