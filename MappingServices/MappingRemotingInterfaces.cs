using System;
using System.Collections.Generic;


namespace PADIMapNoReduce
{

    public interface IMapper {
        IList<KeyValuePair<String, String>> Map(String fileLine);
    }

    public interface IClient 
    {
        string[] getSplit(string filename, int lower, int higher);
        void storeSplit(string filename, IList<KeyValuePair<String, String>> set, int id);
        bool newJob(string trackerUrl, string inputFilePath, string outputDir, int numOfSplits, string mapClass, string mapDll);
    }
    public interface IJobTracker
    {
        int sendMeta(string clientAddr, string filename, int filelines, string map, byte[] code);
        void submitJob(int jobId, int numSplits);
        string finishWorker(int jobId);
        void beAuxiliar(string masterUrl, int jobId, int filelines, int trackerId, int trackerFactor);
        void finishTracker(int jobId);
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
        public string trackerUrl;   //who serves the task

        public Task(int lower, int higher, int id, int jobId, string trackerUrl)
        {
            this.lower = lower;
            this.id = id;
            this.higher = higher;
            this.jobId = jobId;
            this.trackerUrl = trackerUrl;
        }
            

    }

    [Serializable]
    public struct JobMeta
    {
        public int jobId;
        public string clientAddr;
        public string filename;
        public string map;
        public byte[] code;

        public JobMeta (int jobId, string clientAddr,  string filename, string map, byte[] code)
        {
            this.jobId = jobId;
            this.clientAddr = clientAddr;
            this.filename = filename;
            this.map = map;
            this.code = code;
        }


    }
    
}
