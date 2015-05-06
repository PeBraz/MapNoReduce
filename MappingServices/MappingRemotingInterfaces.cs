using System;
using System.Collections.Generic;


namespace PADIMapNoReduce
{

    public interface IMapper {
        ISet<KeyValuePair<String, String>> Map(String fileLine);
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
        void SendMapper(byte[] code, String className);
        string finish(int jobId);
       
    }


    public interface INetwork
    {
        IDictionary<int,string> join(int id, string ip);
        void start(string bootIp);
        void addNode(int id, string ip);
        void heartbeat();
    }


    public interface IWorker 
    {

       void startSplit(string map, string filename, Task[] ws);

       void createMapper(byte[] code, String className);
       // after the job is done the mapper used can be deleted
       void freeMapper(String className);
       void printStatus();
       //void addDelay(int seconds);
       void freeze();
       void unfreeze();
       void delay(int seconds);
    }
    [Serializable]
    public struct Task
    {
        public int lower;
        public int higher;
        public int id;
        public int jobId;
        public string map;
        public string trackerUrl;

        public Task(int lower, int higher, int id, int jobId, string map, string trackerUrl)
        {
            this.lower = lower;
            this.id = id;
            this.higher = higher;
            this.jobId = jobId;
            this.map = map;
            this.trackerUrl = trackerUrl;
        }
            

    }
 
}
