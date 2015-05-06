using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace PADIMapNoReduce
{
    /**
     *  This class represents a instance of a running job on the job tracker, 
     *  Allow for multiple jobs to be running with different mappers at the same time
     *  
     */
    public class Job
    {

        private int id;
        private string map;
        private Task[] tasks;
        private int workersDone;

        public Job(string map, Task[] tasks, int id, int startingWorkers)
        {
            this.map = map;
            this.tasks = tasks;
            this.id = id;
            this.workersDone = startingWorkers;
        }

        public void workerFinished() 
        {
            this.workersDone -= 1;
        }

        /*
         * A job is finished when all the workers doing the tasks return
         */

        public bool isFinished() 
        {
            lock(this){
                return this.workersDone == 0; 
            }
       }

        public int Id 
        {
            get 
            {
                return this.id;
            }
        }
    }


    partial class WorkRemote
    {
        private int done = 0;   //temporary used by join() to signal worker has no more work to do
        private IDictionary<int, Job> steve = new Dictionary<int, Job>();


        public void submitJob(string map, string filename, int numSplits, int numberOfLines)
        {
            Console.WriteLine("Request for file: " + filename);

            int jobId = new Random().Next();

            int step = numberOfLines / numSplits;
            int remainder = numberOfLines % numSplits;

            Task[] tasks = new Task[numSplits]; 

            for (int i = 0, index = 0; i < numSplits; i++, index += step + ((remainder > 0) ? 1 : 0), remainder--)
            {
                tasks[i] = new Task(index, index + step + ((remainder > 0) ? 1 : 0), i, jobId, map, this.url);
            }

       
           
            IList<IWorker> workers = this.getWorkers();
            int numWorkers = workers.Count;  //need to check if all slaves are alive
            steve[jobId] = new Job(map, tasks, jobId, numWorkers);
            int j = 0;
            step = numSplits / numWorkers;
            remainder = numSplits % numWorkers;

            foreach (IWorker worker in workers)
            {

                Task[] task = new Task[step + ((remainder > 0) ? 1 : 0)];
                Array.Copy(tasks, j, task, 0, step + ((remainder > 0) ? 1 : 0));
         
                j += step + ((remainder > 0) ? 1 : 0);
                remainder--;

                try
                {
                    worker.startSplit(map, filename, task);
                }
                catch (SocketException) 
                {
                    Console.WriteLine("A worker died");
                }
            }


            while (!steve[jobId].isFinished())
            {
               Thread.Sleep(1000);
            }

        }

        /**
         * Request for worker who is job tracker to propagate the code between the known workers 
         */
        public void SendMapper(byte[] code, String className)
        {
            foreach (IWorker worker in this.getWorkers())
                worker.createMapper(code, className);
        }

        /**
         *  After a worker did all the jobs, it notifies the job tracker that he is available
         * 
         */
        public string finish(int jobId) 
        {
            steve[jobId].workerFinished();

            //when a worker is done, it is ready to accept jobs from stragglers
            //should return the address of a straggler worker


            return null;
        }


        private Job getJob(int id) 
        {
            foreach (Job job in steve.Values) 
            {
                if (job.Id == id)
                    return job;
            }
            return null;
        }


        private bool amMaster()
        {
            return Worker.amMaster;
        }

    }

}
