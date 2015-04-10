using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PADIMapNoReduce
{
    partial class WorkRemote
    {
        private int done = 0;   //temporary used by join() to signal worker has no more work to do
        private Queue<WorkStruct> queue = new Queue<WorkStruct>();
        private List<KeyValuePair<int, IWorker>> slaves = new List<KeyValuePair<int, IWorker>>();



        public void submitJob(string map, string filename, int numSplits, int numberOfLines)
        {
            Console.WriteLine("Request for file: " + filename);

            int numSlaves = this.slaves.Count;
            int step = numberOfLines / numSplits;
            int remainder = numberOfLines % numSplits;

            for (int i = 0, index = 0; i < numSplits; i++, index += step + ((remainder > 0) ? 1 : 0))
            {
                WorkStruct ws = new WorkStruct(index, index + step + ((remainder > 0) ? 1 : 0), i);
                queue.Enqueue(ws);
                remainder--;
            }

            foreach (KeyValuePair<int, IWorker> slave in slaves)
            {
                if (queue.Count == 0) break;
                slave.Value.startSplit(map, filename, (WorkStruct)queue.Dequeue());
            }

            while (done < numSlaves)
            {
               Thread.Sleep(10000);
            }

            done = 0;
        }
        /**
         * Request for worker who is job tracker to propagate the code between the known workers 
         */
        public void SendMapper(byte[] code, String className)
        {
            foreach (KeyValuePair<int, IWorker> slave in slaves)
                slave.Value.createMapper(code, className);
        }

        public void connect(int id, string url) 
        { 
            if (!this.amMaster()) return;

            IWorker worker =  (IWorker)Activator.GetObject(typeof(IWorker), url);
            slaves.Add(new KeyValuePair<int, IWorker>(id, worker));
        }

        public void join()
        {
            this.done++;
        }

        private bool amMaster()
        {
            return Worker.amMaster;
        }

        public WorkStruct? hazWorkz()
        {
            lock (this)
            {
                return queue.Count == 0 ? (WorkStruct?)null : (WorkStruct)queue.Dequeue();
            }
        }

    }

}
