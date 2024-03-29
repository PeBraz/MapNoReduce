using System;
using System.IO;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Net.Sockets;
using PADIMapNoReduce;

namespace PADIMapNoReduce
{

    public class Worker
    {
        public static int id;
        public static bool amMaster = false;
        private static string url;
        private static string bootIp;

        public Worker(int id, string url, string bootIp) 
        {
            // Static Worker variables for Remote Service initialization
            Worker.id = id;
            Worker.url = url;
            Worker.bootIp = bootIp;

            TcpChannel channel = new TcpChannel(getPort());
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkRemote), "W", WellKnownObjectMode.Singleton);

            ((INetwork)Activator.GetObject(typeof(WorkRemote), Worker.getUrl())).start(Worker.bootIp);

            Console.WriteLine("Started on: " + getUrl());
        }

        

        private int getPort()
        {
            return int.Parse(Worker.url.Split(':')[2].Split('/')[0]);
        }
        public static int getId(){
            return Worker.id;
        }
        public static string getBootstrapIp() 
        {
            return Worker.bootIp;
        }

        public static string getUrl() 
        {
            return Worker.url;
        }



        [STAThread]
        static void Main(string[] args)
        {
            // 1st id of the worker
            // 2nd url of the new worker
            // 3rd (optional) tracker to connect

            if (args.Length == 2)
                new Worker(int.Parse(args[0]), args[1], null);
            else if (args.Length == 3)
                new Worker(int.Parse(args[0]), args[1], args[2]);
            else{

                Console.WriteLine("No arguments given. Starting in command line mode");
                Console.Write("[id] [my-port-id 0-9999] [tracker-port-id 0-9999] >>   ");
                string[] cmd = Console.ReadLine().Trim().Split();


                if (cmd.Length == 2)
                    new Worker(int.Parse(cmd[0]), "tcp://localhost:" +(30000+int.Parse(cmd[1])).ToString()+"/W", null);
                else if (cmd.Length == 3)
                    new Worker(int.Parse(cmd[0]), "tcp://localhost:" +(30000+int.Parse(cmd[1])).ToString()+"/W",
                        "tcp://localhost:" +(30000+int.Parse(cmd[2])).ToString()+"/W");
            }
            System.Console.WriteLine("Press <enter> to terminate...");
            System.Console.ReadLine();
        }
    }

    class Map
    {
 
        private Type type;
        private object classObj;
        private string methodName =  "Map";

        public Map(Type type) {
            this.classObj = Activator.CreateInstance(type);
            this.type = type;
        }

        public IList<KeyValuePair<String, String>> map(string fileLine) {
            object[] args = new object[] { fileLine };
            return (IList<KeyValuePair<String, String>>)
                    this.type.InvokeMember(methodName, BindingFlags.Default | BindingFlags.InvokeMethod, null, this.classObj, args);
        }
    }

    partial class WorkRemote : MarshalByRefObject, IWorker, IJobTracker, INetwork
    {
        //private IList<KeyValuePair<String, String>> map;
       

        private const string STATUS_IDLE = "idle";
        private const string STATUS_WORK = "working";
        private const string STATUS_BLOCKED = "blocked";

        //Counter to know how many tasks have been completed by this worker for each job,
        // (stolen tasks are not counted)
        //private IDictionary<int, int> taskCounter = new Dictionary<int, int>();
        //first key: jobId, secondKey: trackerUrl
        private IDictionary<int, IDictionary<string, int>> taskCounter = new Dictionary<int, IDictionary<string, int>>();
        public string status;

        private Object delayLock = new Object();
        private Object freezeLock = new Object();
        private int id;
        private string url;
        private Queue<Task> queue = new Queue<Task>();

        private IDictionary<string, Map> myMaps = new Dictionary<string,Map>();
        private IDictionary<int, JobMeta> metas = new Dictionary<int, JobMeta>();

        private ManualResetEvent workerMre = new ManualResetEvent(true);

        private Task? runningTask = null;

        public WorkRemote()
        {
            this.id = Worker.getId();
            this.url = Worker.getUrl();
            Console.WriteLine( Worker.getBootstrapIp() != null ? "Connecting to: " + Worker.getBootstrapIp() : "<New network Created>");
            this.statusIdle();

            new Thread(() => mainThread()).Start();
        }
        


        public void mainThread() 
        {
            List<KeyValuePair<String, String>> taskOutputs = new List<KeyValuePair<String, String>>();
            IClient client = null;
 

            while (true) 
            {

                while (true)
                {

          

                    Task? task = this.getTask();
                    if (task == null) break;
                    this.statusWorking(task.Value);


                    JobMeta meta = metas[task.Value.jobId];
                    client = ((IClient)Activator.GetObject(typeof(IClient), meta.clientAddr));

                    String[] splits = client.getSplit(meta.filename, task.Value.lower, task.Value.higher);
                    if (splits == null) break;

                    Map map = myMaps[meta.map];
                    foreach (String s in splits)
                    {
                        taskOutputs.AddRange(map.map(s));
                    }

                    workerMre.WaitOne();

                    Console.WriteLine("Did: " + task.Value.id + "; Tracker: " + task.Value.trackerUrl);
                    client.storeSplit(meta.filename, taskOutputs, task.Value.id);
                    taskOutputs.Clear();


                    //This supposes that no tasks were stolen from the worker
                    //If a task is stolen then the counter must go down
                    if (--taskCounter[task.Value.jobId][task.Value.trackerUrl] <= 0)
                    {
                        IJobTracker tracker = (IJobTracker)Activator.GetObject(typeof(WorkRemote), task.Value.trackerUrl);
                        new Thread(() => tracker.finishWorker(task.Value.jobId)).Start();
                    }

                } 
                this.statusIdle();
                Thread.Sleep(100);  //sleep while no jobs in queue

            }
                
        }


      
        /**
         *  Job tracker asks for tasks that were completed
         */
        /**public KeyValuePair<int,int>[] heartbeat() 
        {
            lock (this.tasksDone) 
            {
                KeyValuePair<int,int>[] arr = new KeyValuePair<int,int>[this.tasksDone.Count];
                this.tasksDone.CopyTo(arr, 0);
                this.tasksDone.Clear();
                return arr;
            }
        }*/


        private void storeTasks(Task[] tasks) 
        {
            lock (this.taskCounter)
            {
                if (!this.taskCounter.ContainsKey(tasks[0].jobId))
                {
                    this.taskCounter.Add(tasks[0].jobId, new Dictionary<string, int>());
                }
            }

            lock(this.taskCounter[tasks[0].jobId]) 
            {
                if (!this.taskCounter[tasks[0].jobId].ContainsKey(tasks[0].trackerUrl))
                {
                    this.taskCounter[tasks[0].jobId].Add(tasks[0].trackerUrl, 0);
                }          
            }

            this.taskCounter[tasks[0].jobId][tasks[0].trackerUrl] += tasks.Length;
            lock(this.queue){
                foreach(Task task in tasks)
                {
                    this.queue.Enqueue(task);
                }
            }
        }

        private Task? getTask() 
        {
            lock (this.queue) 
            {
                if (this.queue.Count > 0)
                    return this.queue.Dequeue();
            }
            return null;
        }
        public void statusWorking()
        {
            this.status = STATUS_WORK;
        }

        public void statusWorking(Task? task)
        {
            this.runningTask = task;
            this.status = STATUS_WORK;
        }

        public void statusFrozen() 
        {
            this.status = STATUS_BLOCKED;
        }

        public void statusIdle() 
        {
            this.runningTask = null;
            this.status = STATUS_IDLE;
        }

        public void printStatus()
        {
            Console.WriteLine(String.Format("[{0} - {1}]\t{2}", this.id, Worker.getUrl(), this.status));
        }

        public void delay(int seconds)
        {
            workerMre.Reset();
            Thread.Sleep(seconds * 1000); 
            workerMre.Set();
            this.statusWorking();
        }


        public void freezeWorker() 
        {
            workerMre.Reset();
            this.statusFrozen();
        }

        public void unfreezeWorker() 
        {
            workerMre.Set();
            this.statusWorking();
        }


        public void startSplit(Task[] tasks)
        {
            new Thread(() => storeTasks(tasks)).Start();
        }

        public Map createMapper(byte[] code, string className)
        {
            Assembly assembly = Assembly.Load(code);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + className))
                    {
                        return new Map(type);
                    }
                }
            }
            return null;
        }

        public void createMeta(JobMeta meta) 
        {
            this.metas[meta.jobId] =  meta;
            this.myMaps[meta.map] = createMapper(meta.code, meta.map);
        }

        /*
         *  Deletes a mapper from the list
         */
        public void freeMapper(string name) 
        {
            KeyValuePair<string, Map>? map = this.getMap(name);
            if (map != null)
                this.myMaps.Remove(map.Value);
        }


        public KeyValuePair<string,Map>? getMap(string name) 
        {
            foreach (KeyValuePair<string, Map> map in this.myMaps)
            {
                if (map.Key.Equals(name))
                    return map;
            }
            return null;
        }
    }

}
