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
        private Task[] tasks;
        private int batchesCount; //number of batches required for the job to be completed
        private int fileLines;
        private JobMeta meta;


        public Job(string client, string tracker , string filename, int fileLines, string map, byte[] code)
        {
            this.id = new Random().Next();
            this.fileLines = fileLines;
            this.meta = new JobMeta(this.id, client, tracker, filename, map, code);
        }

        public JobMeta getMeta()
        {
            return this.meta;
        }

        /*
         *  Creates tasks according to size number of splits neededs
         */
        public void splitSplits(int numSplits)
        {
            int step = this.fileLines / numSplits;
            int remainder = this.fileLines % numSplits;

            this.tasks = new Task[numSplits]; 

            for (int i = 0, index = 0; i < numSplits; i++, index += step + ((remainder > 0) ? 1 : 0), remainder--)
            {
                this.tasks[i] = new Task(index, index + step + ((remainder > 0) ? 1 : 0), i, this.id);
            }
        }

        public Task[] getTaskBatch(int offset, int size)
        {
            Task[] task = new Task[size];
            Array.Copy(this.tasks, offset, task, 0, size);

            this.batchesCount += 1;
            return task;
        }

        public void workerFinished() 
        {
            this.batchesCount -= 1;
        }

        /*
         * A job is finished when all the workers doing the tasks return
         */

        public bool isFinished() 
        {
                return this.batchesCount == 0; 
        }

        public int Id 
        {
            get 
            {
                return this.id;
            }
        }

        public override String ToString()
        {
            return "[JOB:" + id + "]<filename: " + this.meta.filename + "; map: " + this.meta.map + ";> ";
        }
    }


    partial class WorkRemote
    {
        private IDictionary<int, Job> steve = new Dictionary<int, Job>();
       
        public void submitJob(int jobId, int numSplits)
        {
       

            Job job = steve[jobId];
            Console.WriteLine("Started Job: " + job.ToString() );

            job.splitSplits(numSplits);

            IList<IWorker> workers = this.getWorkers();
         
            int numWorkers = workers.Count;
            int j = 0;
            int step = numSplits / numWorkers;
            int remainder = numSplits % numWorkers;
            int size;
            foreach (IWorker worker in workers)
            {
                size = step + ((remainder > 0) ? 1 : 0);

                try
                {
                    worker.startSplit(job.getTaskBatch(j,size));
                }
                catch(SocketException)
                {
                    //I a worker died, replication must work
                    Console.WriteLine("A worker died.");
                }

                j += size;
                remainder--;
          
            }


            while (!steve[jobId].isFinished())
            {
               Thread.Sleep(1000);
            }

        }


        /*
         * Sends all information necessary for job execution, all workers will keep this information stored.
         * returns jobId so that the client can send splits
         */
        public int sendMeta(string clientAddr, string filename, int filesize, string map, byte[] code)
        {
            Job j = new Job(clientAddr, this.url, filename,filesize, map, code);
            this.steve[j.Id] = j;
            foreach (IWorker worker in this.getWorkers())
                worker.createMeta(j.getMeta());
         
            return j.Id;
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
