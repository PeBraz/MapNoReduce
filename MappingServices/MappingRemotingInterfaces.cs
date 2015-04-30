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
        //WorkStruct? hazWorkz(); 
        void SendMapper(byte[] code, String className);
        string finish();
       
        void connect(int id, string url);
    }
    public interface IWorker 
    { 
       void startSplit(string map, string filename, Task[] ws);

       void createMapper(byte[] code, String className);
       // after the job is done the mapper used can be deleted
       void freeMapper(String className);
       KeyValuePair<int, int>[] heartbeat();
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
 
}
