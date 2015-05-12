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


        private int trackerCount;   //number of trackers sharing the same job
        private int trackerId;      //identifier for this tracker, in the set of trackers sharing the same job
        private int runningTrackersCount;
        private JobMeta meta;
        private IJobTracker master;
        //amount of job trackers to be created (> 1)
        public static int JOB_TRACKER_FACTOR = 2;

        /*
         * This constructor is used by the main job tracker
         */
        public Job(string client, string filename, int fileLines, string map, byte[] code)
        {
            this.id = new Random().Next();
            this.fileLines = fileLines;
            this.meta = new JobMeta(this.id, client, filename, map, code);

            this.trackerCount = JOB_TRACKER_FACTOR;
            this.trackerId = 1 ;
            this.runningTrackersCount = JOB_TRACKER_FACTOR;
            this.master = null;
        }

        /*
         * Creates a job using a JobMeta struct, needed if this worker will act as an auxiliary job tracker
         */
        public Job(string masterUrl, JobMeta meta, int filelines, int trackerId, int trackerCount) 
        {
            this.id = meta.jobId;
            this.fileLines = filelines;
            this.meta = meta;
            this.trackerCount = trackerCount;
            this.trackerId = trackerId;
            this.runningTrackersCount = 1;  //if secondary I am the only one
            this.master = (IJobTracker)Activator.GetObject(typeof(IJobTracker), masterUrl);
        }



        public JobMeta getMeta()
        {
            return this.meta;
        }

        /*
         *  Creates tasks according to size number of splits needed and provides a batch of tasks depending on how many workers are needed
         *  
         *  The number of splits will depend on the number of trackers doing the same job and the id of this tracker
         */
        public IList<Task[]> splitSplits(int numSplits, int numWorkers)
        {
            //calculates the starting task id for this worker
            //By knowing how many splits it needs to do
            int taskId = this.getTrackerIndex(this.trackerId, numSplits, this.trackerCount);

            //calculate the starting point for this tracker
            //(how many splits the others will do)
           /* int index = 0;
            for (int i = 1; i < this.trackerId ; i++) 
            {
                index += numSplits / this.trackerCount + (numSplits % this.trackerCount >= i ? 1 : 0);  (this.trackerId - 1);
            }*/

            int index = this.getTrackerStartingLine(this.trackerId, this.fileLines, this.trackerCount);  
            //calculates how many splits needed for this tracker
            numSplits = numSplits / this.trackerCount + (numSplits % this.trackerCount >= trackerId ? 1 : 0);
            //Calculates how many lines needed for each step
            int filelines = this.fileLines / numSplits;

            //create all tasks
            int step = filelines/ numWorkers;

            int remainder = numSplits % numWorkers;

            this.tasks = new Task[numSplits];

            
            for (int i = 0; i < numSplits; i++, index += step + ((remainder > 0) ? 1 : 0), remainder--)
            {
                this.tasks[i] = new Task(index, index + step + ((remainder > 0) ? 1 : 0), taskId++, this.id, Worker.getUrl());
            }

            //divide all tasks by the workers
            IList<Task[]> batches = new List<Task[]>();
            int j = 0;
            step = numSplits / numWorkers;
            remainder = numSplits % numWorkers;
            int size;
            for (int i = 0; i < numWorkers; i++ )
            {
                size = step + ((remainder > 0) ? 1 : 0);
                batches.Add(this.getTaskBatch(j, size));

                j += size;
                remainder--;

            }
            return batches;
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

        public void trackerFinished()
        {
            this.runningTrackersCount -= 1;
        }

        /*
         * A job is finished when all the workers doing the tasks return
         */

        public bool isFinished() 
        {
            //stops at 1 tracker running because it is itself
            // batchesCount jumped from 1 to -1 on a auxiliar server (??)
            return this.batchesCount <= 0 && this.runningTrackersCount <= 1; 
        }

        public bool amMaster()
        {
            return this.trackerId == 1;
        }

        public IJobTracker getMaster() 
        {
            return this.master;
        }
        public int Id 
        {
            get 
            {
                return this.id;
            }
        }
        /*
         *  Gets the number of splits needed for the specified tracker
         */
        public int getTrackerSplitsCount(int trackerId, int numSplits, int numTrackers) 
        {
            return numSplits / numTrackers + (numSplits % numTrackers >= trackerId ? 1 : 0);
        }
        /*
        *  Gets the number of splits needed for the specified Worker
        */
        public int getWorkerSplitsCount(int trackerId, int workerId, int numWorkers, int numSplits, int numTrackers)
        {
            int trackerSplitsCount = getTrackerSplitsCount(trackerId, numSplits, numTrackers);
            return trackerSplitsCount / numWorkers + (trackerSplitsCount % numWorkers >= trackerId ? 1 : 0);
        }
        /*
         *  Gets the initial index for this tracker (the tasks the tracker creates start from this index)
         */
        public int getTrackerIndex(int trackerId, int numSplits, int numTrackers)
        {
            return 1 + (numSplits / numTrackers) * (trackerId - 1) + (trackerId <= (numSplits % numTrackers) ? trackerId - 1 : (numSplits % numTrackers));
        }
        /*
         *  Gets the initial line number for the tracker (the tracker starts splitting from this line)
         */
        public int getTrackerStartingLine(int trackerId, int numlines, int numTrackers)
        {
             return 1 + (numlines/numTrackers)* (trackerId - 1) + ( trackerId <= (numlines%numTrackers) ? trackerId - 1 : (numlines%numTrackers)); 
        }
        public override String ToString()
        {
            return "[JOB:" + id + "]<filename: " + this.meta.filename + "; map: " + this.meta.map + ";> ";
        }
    }


    partial class WorkRemote
    {
        private IDictionary<int, Job> steve = new Dictionary<int, Job>();

        private IList<IJobTracker> auxTrackers;

        private ManualResetEvent trackerMre = new ManualResetEvent(true);

        public void submitJob(int jobId, int numSplits)
        {

            Job job = steve[jobId];
            Console.WriteLine("Started Job: " + job.ToString());


            if (job.amMaster())
            {
                foreach (IJobTracker trackerAux in this.auxTrackers)
                {
                    new Thread(() => trackerAux.submitJob(jobId, numSplits)).Start();
                }
            }


            IList<IWorker> workers = this.getWorkers();
         

            IList<Task[]> batches = job.splitSplits(numSplits, workers.Count);

            int index = 0;
            foreach (IWorker worker in workers)
            {
                try
                {
                    //save some info about the worker
                    worker.startSplit(batches[index++]);
                }
                catch (SocketException)
                {
                    //If a worker died, tasks must be resubmitted
                    Console.WriteLine("A worker died.");
                }
          
            }
            while (!steve[jobId].isFinished())
            {
                trackerMre.WaitOne();
                Thread.Sleep(1000);
            }

            if (!job.amMaster())
            {

                job.getMaster().finishTracker(job.Id);
            }
        }


        /*
         * Sends all information necessary for job execution, all workers will keep this information stored.
         * returns jobId so that the client can send splits
         */
        public int sendMeta(string clientAddr, string filename, int filelines, string map, byte[] code)
        {
            Job j = new Job(clientAddr, filename,filelines, map, code);

            this.steve[j.Id] = j;
            foreach (IWorker worker in this.getWorkers())
                worker.createMeta(j.getMeta());

            this.addAuxTrackers(Job.JOB_TRACKER_FACTOR, j.Id, filelines);

            return j.Id;
        }

        /*
         *  Main Job Tracker chooses other job trackers to help with the job
         */
        public void addAuxTrackers(int trackerCount, int jobId, int filelines) 
        {
            this.auxTrackers = this.getRandomNodes(trackerCount - 1);

            int auxTrackerId = 2;//next auxiliary tracker id
            foreach (IJobTracker tracker in this.auxTrackers)
            {
                //Warning: aux trackers can't fail here
                tracker.beAuxiliar(Worker.getUrl(), jobId, filelines, auxTrackerId++, Job.JOB_TRACKER_FACTOR);
            }
        
        }
        /*
         *  Contacts the trackers about the state of the job being run
         */
        /*
        public void contactTrackers() 
        {
            lock (this.auxTrackers)
            {
                foreach (IJobTracker tracker in this.auxTrackers)
                {
                    tracker.contact();
                }
            }
        }*/

        /*
         * When a worker fails, discovers the tasks to be done and send to someone else 
         *  (the worker can also fail at the start of the split)
         */
        public void getTasks() 
        {
        
        }

        /**
         *  After an auxiliary job tracker ended, it notifies the main tracker
         * 
         */
        public void finishTracker(int jobId)
        { 
            this.steve[jobId].trackerFinished();
        }

        /*
         *
         *
         */
        public void beAuxiliar(string masterUrl, int jobId, int filelines, int trackerId, int trackerFactor )
        {
            //get the job information using the meta from the worker
            JobMeta myMeta = this.metas[jobId];
            Job j = new Job(masterUrl, myMeta, filelines, trackerId, trackerFactor);
            this.steve[j.Id] = j; 
        }

        /**
         *  After a worker did all the jobs, it notifies the job tracker that he is available
         * 
         */
        public string finishWorker(int jobId) 
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

        public void freezeTracker() 
        {
            trackerMre.Reset();
        }
        public void unfreezeTracker()
        {
            trackerMre.Set();
        }

    }

}
