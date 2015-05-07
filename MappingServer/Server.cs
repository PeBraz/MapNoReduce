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

        public ISet<KeyValuePair<String, String>> map(string fileLine) {
            object[] args = new object[] { fileLine };
            return (ISet<KeyValuePair<String, String>>)
                    this.type.InvokeMember(methodName, BindingFlags.Default | BindingFlags.InvokeMethod, null, this.classObj, args);
        }
    }

    partial class WorkRemote : MarshalByRefObject, IWorker, IJobTracker, INetwork
    {
        private IClient client;
        //private IList<KeyValuePair<String, String>> map;
       

        private const string STATUS_IDLE = "idle";
        private const string STATUS_WORK = "working";
        private const string STATUS_BLOCKED = "blocked";

        //Counter to know how many tasks have been completed by this worker for each job,
        // (stolen tasks are not counted)
        private IDictionary<int, int> taskCounter = new Dictionary<int, int>();

        public string status;

        private Object delayLock = new Object();
        private Object freezeLock = new Object();
        private bool frozen = false;
        private int id;
        private string url;
        private Queue<Task> queue = new Queue<Task>();

        private IDictionary<string, Map> myMaps = new Dictionary<string,Map>();
        private IDictionary<int, JobMeta> metas = new Dictionary<int, JobMeta>();

        public WorkRemote()
        {
            this.id = Worker.getId();
            this.url = Worker.getUrl();
            Console.WriteLine("Initializing: " + Worker.getBootstrapIp());
            client = ((IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:10001/C"));
            this.setStatus(STATUS_IDLE);

            new Thread(() => mainThread()).Start();
        }
        


        public void mainThread() 
        {
            ISet<KeyValuePair<String, String>> taskOutputs = new HashSet<KeyValuePair<String, String>>();
            IClient client = null;
 

            while (true) 
            {

                while (true)
                {

                    Task? task = this.getTask();
                    if (task == null) break;
                    this.setStatus(STATUS_WORK);

                    JobMeta meta = metas[task.Value.jobId];
                    client = ((IClient)Activator.GetObject(typeof(IClient), meta.clientAddr));

                    String[] splits = client.getSplit(meta.filename, task.Value.lower, task.Value.higher);
                    if (splits == null) break;


                    Map map = myMaps[meta.map];
                    foreach (String s in splits)
                    {
                        taskOutputs.UnionWith(map.map(s));
                    }

                    lock (delayLock) { };

                    while (frozen)
                    {
                        Thread.Sleep(100);
                    }


                    Console.WriteLine("Did: " + task.Value.id);
                    client.storeSplit(meta.filename, taskOutputs, task.Value.id);
                    taskOutputs.Clear();


                    //This supposes that no tasks were stolen from the worker
                    //If a task is stolen then the counter must go down
                    if (--taskCounter[task.Value.jobId] <= 0)
                    {
                        IJobTracker tracker = (IJobTracker)Activator.GetObject(typeof(WorkRemote), meta.trackerAddr);
                        new Thread(() => tracker.finish(task.Value.jobId)).Start();
                    }
                } 
                this.setStatus(STATUS_IDLE);
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
            this.taskCounter[tasks[0].jobId] = tasks.Length;

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

        public void setStatus(string status) 
        {
            this.status = status;
        }

        public void printStatus()
        {
            Console.WriteLine(String.Format("[{0} - {1}]\t{2}", this.id, Worker.getUrl(), this.status));
        }

        public void delay(int seconds)
        {

            lock (delayLock) {
                this.setStatus(STATUS_BLOCKED);
                Thread.Sleep(seconds * 1000); 
            }
            this.setStatus(STATUS_WORK);
        }


        public void freeze() 
        {
            frozen = true;
            this.setStatus(STATUS_BLOCKED);
        }

        public void unfreeze() 
        {
            frozen = false;
            this.setStatus(STATUS_WORK);
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
