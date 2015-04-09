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


            this.me = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + (10000 + id).ToString() + "/C");
            bool success = false;
            do
            {
                success = this.me.newJob("tcp://localhost:30001/W", "Map", Client.path(@"Mapper\bin\Debug\Mapper.dll"), Client.path("illumhis.txt"), @"H:\Documents\MapNoReduce\Outputs\", 1001);

                if (!success) 
                    Console.WriteLine("Connection failed, retrying...");

            } while (!success);

            Console.WriteLine("<Success>");
            Console.ReadLine();

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
        public bool newJob(string trackerUrl, string mapClass, string mapDll, string inputFilePath, string outputDir, int numOfSplits)
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
