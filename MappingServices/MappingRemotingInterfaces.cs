using System;
using System.Collections.Generic;


namespace PADIMapNoReduce
{

    public interface IMap {
        ISet<KeyValuePair<String, String>> Map(String fileLine);
    }

    public interface IClient 
    {
        string[] getSplit(int lower, int higher);
        void storeSplit(ISet<KeyValuePair<String, String>> set, int id);
    }
    public interface IJobTracker
    { 
        void submitJob(IMap map, string filename, int numSplits, int numberOfLines);
        WorkStruct hazWorkz();
    }
    public interface IWorker 
    { 
       void startSplit(IMap map, string filename, WorkStruct ws);
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
