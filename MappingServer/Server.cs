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

        public Worker(int id, string url, string trackerUrl) 
        {
            Worker.id = id;
            Worker.url = url;
            this.start(trackerUrl);
        }

        private int getPort()
        {
            return int.Parse(Worker.url.Split(':')[2].Split('/')[0]);
        }
        public static int getId(){
            return Worker.id;
        }
        public static string getUrl() 
        {
            return Worker.url;
        }
        private void start(string entryUrl)
        {
            TcpChannel channel = new TcpChannel(getPort());
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkRemote), "W", WellKnownObjectMode.Singleton);
           
            try
            {
                if (entryUrl == null)
                {
                    Worker.amMaster = true;
                    Console.WriteLine("I am Master at 30001");
                }
                else {
                    Console.WriteLine(entryUrl);
                    ((IJobTracker)Activator.GetObject(typeof(IJobTracker), entryUrl)).connect(getId(),getUrl());
                    Console.WriteLine("Listening on: "+ getUrl()); ;
                }
            }
            catch (SocketException)
            {
                Console.Write("Failed to start on url: " + url);
                System.Environment.Exit(1);
            }
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

    partial class WorkRemote : MarshalByRefObject, IWorker, IJobTracker
    {
        private IClient client;
        //private IList<KeyValuePair<String, String>> map;
       

        private const string STATUS_IDLE = "idle";
        private const string STATUS_WORK = "working";
        private const string STATUS_BLOCKED = "blocked";


        public string status;

        private Object delayLock = new Object();
        private Object freezeLock = new Object();
        private bool frozen;
        private int id;

        private Queue<Task> queue; 
        private IList<KeyValuePair<int,int>> tasksDone;
        private IList<KeyValuePair<string, Map>> myMaps;

        public WorkRemote()
        {
            this.id = Worker.getId();
            client = ((IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:10001/C"));
            slaves.Add(new KeyValuePair<int, IWorker>(id, (IWorker)Activator.GetObject(typeof(IWorker), Worker.getUrl())));
            this.setStatus(STATUS_IDLE);
            this.queue = new Queue<Task>();
            this.tasksDone = new List<KeyValuePair<int,int>>();
            this.myMaps = new List<KeyValuePair<string,Map>>();
        }
        
        /*
         * This thread will be created each time this worker is assigned  a group of tasks, change it
         * If there is a thread doing the job then use that one
         * The queue should be unique for all the tasks (from different jobs) being performed in the worker, 
         * there should be a way to correspond a task to a certain job (jobId?) to know which mapper to use
         */
        public void workingThread(string map, string filename, Task[] tasks)
        {
            ISet<KeyValuePair<String, String>> megaList = new HashSet<KeyValuePair<String, String>>();
            IJobTracker tracker = (IJobTracker)Activator.GetObject(typeof(IJobTracker), "tcp://localhost:30001/W");

            this.setStatus(STATUS_WORK);
            this.storeTasks(tasks);

            KeyValuePair<string, Map>? nullMapper = this.getMap(map);
            if (nullMapper == null) return;
            Map mapper = nullMapper.Value.Value;

            Console.WriteLine("Starting tasks; " + tasks.Length + " to do.");
            while (true){
                
                Task? task = this.getTask(); 
                if (task == null) break;
          
                
                String[] splits = client.getSplit(task.Value.lower, task.Value.higher);
                if (splits == null) break;

                foreach (String s in splits)
                {
                    megaList.UnionWith(mapper.map(s));
                }

                lock (delayLock) { };

                while (frozen)
                {
                    Thread.Sleep(100);
                }


                Console.WriteLine("Did: " + task.Value.id);
                this.storeTask(task.Value);
                client.storeSplit(megaList, task.Value.id);

            }

            Console.WriteLine("Ended now, going to steal");
            this.freeMapper(map); //free mapper after all tasks done
            tracker.finish(); 
            this.setStatus(STATUS_IDLE);
        }

        /**
         *  Thread calls this when a task was completed
         */
        private void storeTask(Task task) 
        {
            lock (this.tasksDone)
            {
                this.tasksDone.Add(new KeyValuePair<int,int>(task.jobId,task.id));
            }
        }
        /**
         *  Job tracker asks for tasks that were completed
         */
        public KeyValuePair<int,int>[] heartbeat() 
        {
            lock (this.tasksDone) 
            {
                KeyValuePair<int,int>[] arr = new KeyValuePair<int,int>[this.tasksDone.Count];
                this.tasksDone.CopyTo(arr, 0);
                this.tasksDone.Clear();
                return arr;
            }
        }


        private void storeTasks(Task[] tasks) 
        {
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


        public void startSplit(string map, string filename, Task[] tasks)
        {
            new Thread(() => workingThread(map, filename, tasks)).Start();
        }

        public void createMapper(byte[] code, string className)
        {
            Assembly assembly = Assembly.Load(code);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + className))
                    {
                        //doesn't check duplicates
                        this.myMaps.Add(new KeyValuePair<string, Map>(className, new Map(type)));
                    }
                }
            }
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
