using System;
using System.Collections.Generic;


namespace PADIMapNoReduce
{

    public interface IMap {
        ISet<KeyValuePair<String, String>> map(String fileLine);
    }

    public interface IClient 
    {
        string[] getSplit(int lower, int higher);
        void storeSplit(ISet<KeyValuePair<String, String>> set, int id);
        bool newJob(string trackerUrl, string inputFilePath, string outputDir, int numOfSplits, string mapClass, string mapDll);
    }
    public interface IJobTracker
    { 
        void submitJob(string map, string filename, int numSplits, int numberOfLines);
        WorkStruct? hazWorkz();
        void SendMapper(byte[] code, String className);
        void join();
        void connect(int id, string url);
    }
    public interface IWorker 
    { 
       void startSplit(string map, string filename, WorkStruct? ws);
       void createMapper(byte[] code, String className);
       void printStatus();
       //void addDelay(int seconds);
       void freeze();
       void unfreeze();
       void delay(int seconds);
    }
    [Serializable]
    public struct WorkStruct
    {
        public int lower;
        public int higher;
        public int id;

        public WorkStruct(int lower, int higher, int id)
        {
            this.lower = lower;
            this.id = id;
            this.higher = higher;
        }
            

    }
 
}
