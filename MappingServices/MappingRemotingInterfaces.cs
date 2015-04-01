using System;
using System.Collections.Generic;


namespace PADIMapNoReduce
{

	public interface IMappingServer {
        List<string> RegisterClient(string NewClientPort);
		void SubmitMapping(string message);
	}

	public interface IMappingClient {
		void MsgToClient(string message);
	}

    public interface IMap {
        //Returns a set of key-value pairs for each input key-value pair
        IList<KeyValuePair<String, String>> Map(String filename);
    }

    public interface IClient 
    {
        int numberOfFileLines();
        string[] getSplit(int lower, int higher);
    }
    public interface IJobTracker
    { 
        void submitJob(IMap map, string filename, int numSplits, string outputFile);
        bool hazWorkz();
    }
    public interface IWorker 
    { 
       void startSplit(IMap map, string filename, int lower, int higher, int splitId);
    }


}
