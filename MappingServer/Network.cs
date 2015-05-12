using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
namespace PADIMapNoReduce
{


    /**
     * Simple Network, every node knows eachother, whenever a node joins the network, the node contacted must contact all other nodes and 
     * return all the nodes in the network to the new node.
     */
   partial class WorkRemote
    {
        IDictionary<int,INetwork> network;
        IDictionary<int, string> peers;
  

        //public static const int REPLICATION_FACTOR = 1;
        private const int HEARTBEAT_INTERVAL = 100;
       
       /**
        *   A node asks to join the network, advertise it in the network and return all the nodes in the network 
        * 
        */
        public IDictionary<int,string> join(int id, string ip)
        {
            IDictionary<int, string> nodes = new Dictionary<int,string>();
            nodes.Add(Worker.id, Worker.getUrl()); //add myself
          
            foreach (int key in peers.Keys)
            {
                try
                {
                    ((INetwork)Activator.GetObject(typeof(INetwork), peers[key])).addNode(id,ip);
                    nodes.Add(key, peers[key]);
                }
                catch (SocketException) 
                {
                    Console.Write("Could not contact node with ip: " + peers[key]);
                }
            }
            this.addNode(id, ip);
            return nodes;        
        }

       /*
        *   This also initializes the WorkRemote constructor
        */
        public void start(string bootIp)
        {
            this.networkInit(bootIp);
        }

       /*
        *   Initializes the network component of the worker, requires a node already in the network to join to
        *   If bootstrap ip is null then just initialize data structures.
        */


        public void networkInit(string bootstrapIp)
        {
            this.network = new Dictionary<int,INetwork>();

            if (bootstrapIp == null)
            {
                this.peers = new Dictionary<int, string>();
            }
            else
            {
                try
                {
                    this.peers = ((INetwork)Activator.GetObject(typeof(INetwork), bootstrapIp)).join(this.id,this.url);
                }
                catch (SocketException) 
                {
                    Console.WriteLine("Could not find node with ip: " + bootstrapIp + "; Exiting now.");
                    Environment.Exit(-1);
                }
                foreach (int id in peers.Keys)
                {
                    this.network.Add(id, (INetwork)Activator.GetObject(typeof(WorkRemote), peers[id]));
                }
            }
            new Thread(()=>this.heartbeatThread()).Start();
        }

        private IDictionary<int, string> getPeers() 
        {
            return peers;
        }

        private IList<IWorker> getWorkers()
        {
            IList<IWorker> workers = network.Values.Cast<IWorker>().ToList();
            workers.Add((IWorker)Activator.GetObject(typeof(WorkRemote), this.url));
            return workers;
        }

        private IList<INetwork> getNodes()
        {
            return this.network.Values.ToList<INetwork>();
        }


       /**
        * Returns a specific amount of random workers from the network
        * 
        */
        private IList<IJobTracker> getRandomNodes(int numOfWorkers) 
        {


            IList<IJobTracker> randWorkers = new List<IJobTracker>();
            IList<IJobTracker> workers = this.getNodes().Cast<IJobTracker>().ToList();
            IJobTracker worker = null;

            while (numOfWorkers > 0 ) {
                if (workers.Count == randWorkers.Count) break;  //if the number of workers specified is bigger than the number of workers available, exit the loop  

                worker = workers[(int)new Random().Next(workers.Count)];
                if (randWorkers.Contains(worker)) continue;

                randWorkers.Add(worker);
                numOfWorkers--;
  
            }

            return randWorkers;
        }


        public void addNode(int id, string ip) 
        {
            Console.WriteLine("Worker Joined: " + id);
            this.peers.Add(id,ip);
            this.network.Add(id, (INetwork)Activator.GetObject(typeof(WorkRemote), ip));
        }
        public void removeNode(int id) 
        {
            Console.WriteLine("Worker left: " + id);
            this.peers.Remove(id);
            this.network.Remove(id);
        }

       /**
        *  Contacts all other nodes in the network to check if they are still alive
        */
        public void heartbeatThread() 
        {
            while (true)
            {
                Thread.Sleep(HEARTBEAT_INTERVAL);

                IList<int> downSlaves = new List<int>();

                foreach (int key in this.network.Keys)
                {
                    try
                    {
                        network[key].heartbeat();
          
                    }
                    catch (SocketException)
                    {
                        downSlaves.Add(key);
                    }
                }
                foreach (int worker in downSlaves)
                {
                    this.removeNode(worker);
                }
                downSlaves.Clear();
            }


        }

        public void heartbeat() 
        {
            return;
        }
    }
}
