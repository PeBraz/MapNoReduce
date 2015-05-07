using System;
using System.Collections.Generic;


namespace PADIMapNoReduce
{

    public interface IMapper {
        ISet<KeyValuePair<String, String>> Map(String fileLine);
    }

    public interface IClient 
    {
        string[] getSplit(string filename, int lower, int higher);
        void storeSplit(string filename, ISet<KeyValuePair<String, String>> set, int id);
        bool newJob(string trackerUrl, string inputFilePath, string outputDir, int numOfSplits, string mapClass, string mapDll);
    }
    public interface IJobTracker
    { 
        void submitJob(int jobId, int numSplits);
        string finish(int jobId);
        int sendMeta(string clientAddr, string filename, int filesize, string map, byte[] code);
       
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

       void startSplit(Task[] batch);
       void createMeta(JobMeta meta);
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

        public Task(int lower, int higher, int id, int jobId)
        {
            this.lower = lower;
            this.id = id;
            this.higher = higher;
            this.jobId = jobId;
        }
            

    }

    [Serializable]
    public struct JobMeta
    {
        public int jobId;
        public string clientAddr;
        public string trackerAddr;
        public string filename;
        public string map;
        public byte[] code;

        public JobMeta (int jobId, string clientAddr, string trackerAddr, string filename, string map, byte[] code)
        {
            this.jobId = jobId;
            this.clientAddr = clientAddr;
            this.trackerAddr = trackerAddr;
            this.filename = filename;
            this.map = map;
            this.code = code;
        }


    }
    
}
