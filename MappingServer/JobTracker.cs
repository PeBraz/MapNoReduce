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
        private ISet<int> finishedTasks = new HashSet<int>();

        public Job(string map, Task[] tasks, int id) 
        {
            this.map = map;
            this.tasks = tasks;
            this.id = id;
        }


        public void finished(int taskId)
        {
            lock (this)
            {
                this.finishedTasks.Add(taskId);
            }
        }
        public bool isFinished() 
        {
            lock(this){
                return this.tasks.Length == this.finishedTasks.Count; 
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
        private List<KeyValuePair<int, IWorker>> slaves = new List<KeyValuePair<int, IWorker>>();
        private List<Job> steve;

        private const int HEARTBEAT_INTERVAL = 100;

        /**
         * 
         * 
         * 
         *  map is the name of the Imapper instance to be used in the worker nodes,
         */ 
        public void submitJob(string map, string filename, int numSplits, int numberOfLines)
        {
            Console.WriteLine("Request for file: " + filename);

            int jobId = new Random().Next();

            int numSlaves = this.slaves.Count;  //need to check if all slaves are alive
            int step = numberOfLines / numSplits;
            int remainder = numberOfLines % numSplits;

            Task[] tasks = new Task[numSplits]; 

            for (int i = 0, index = 0; i < numSplits; i++, index += step + ((remainder > 0) ? 1 : 0))
            {
                tasks[i] = new Task(index, index + step + ((remainder > 0) ? 1 : 0), i, jobId);
                remainder--;
            }

            Job job = new Job(map, tasks, jobId);
            steve.Add(job);


            int j = 0;
            int left = numSplits / numSlaves + numSlaves % numSlaves; 

            foreach (KeyValuePair<int, IWorker> slave in slaves)
            {

                Task[] task = new Task[left];
                Array.Copy(tasks, j, task, 0, left);

                j += left;
                left -= left;

                try
                {
                    slave.Value.startSplit(map, filename, task);
                }
                catch (SocketException) 
                {
                    Console.WriteLine("Node died: " + slave.Key);
                }
            }


            while (!job.isFinished())
            {
               Thread.Sleep(1000);
            }

        }

        /**
         * Request for worker who is job tracker to propagate the code between the known workers 
         */
        public void SendMapper(byte[] code, String className)
        {
            foreach (KeyValuePair<int, IWorker> slave in slaves)
                slave.Value.createMapper(code, className);
        }



        public void heartbeatThread() 
        {
            while (true)
            {
                Thread.Sleep(HEARTBEAT_INTERVAL);
                  
                List<KeyValuePair<int,IWorker>> downSlaves = new List<KeyValuePair<int, IWorker>>();

                KeyValuePair<int, int>[] tasks;
                foreach (KeyValuePair<int,IWorker> slave in this.slaves)
                {
                    try
                    {
                        tasks = slave.Value.heartbeat();
                        //do somethin about state of the job
                    }
                    catch (SocketException)
                    {
                        downSlaves.Add(slave);  
                    }
                }
                foreach (KeyValuePair<int, IWorker> slave in downSlaves) 
                {
                    this.slaves.Remove(slave);
                }
                downSlaves.Clear();
            }
        
        
        }


        public string finish() 
        {
            //when a worker is done, it is ready to accept jobs from stragglers
            //should return the address of a straggler worker
            return null;
        }


        public void connect(int id, string url) 
        { 
            if (!this.amMaster()) return;

            IWorker worker = (IWorker)Activator.GetObject(typeof(IWorker), url);
            slaves.Add(new KeyValuePair<int, IWorker>(id, worker));
        }


        private Job getJob(int id) 
        {
            foreach (Job job in steve) 
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
