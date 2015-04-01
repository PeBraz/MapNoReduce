using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections.Generic;

using MappingRemotingInterfaces;

namespace Mapping{
	// Summary description for Class1.
	class Server
	{
		// The main entry point for the application.
		[STAThread]
		static void Main(string[] args) {
			TcpChannel channel = new TcpChannel(8086);
			ChannelServices.RegisterChannel(channel,false);
			RemotingConfiguration.RegisterWellKnownServiceType(	typeof(MappingServerServices), "ChatServer", WellKnownObjectMode.Singleton);
			System.Console.WriteLine("Press <enter> to terminate chat server...");
			System.Console.ReadLine();
		}
	}
	
	class MappingServerServices : MarshalByRefObject, IMappingServer {
        List<IMappingClient> clients;
		List<string> messages;

		MappingServerServices() {
            clients = new List<IMappingClient>();
            messages = new List<string>();
		} // <construtor> incializa : lista com os clientes do chat + lista com as mensagens para serem transmitidas (pedidos)

        public List<string> RegisterClient(string NewClientName) {
			Console.WriteLine("New client listening at " + ".tcp://localhost:" + NewClientName + "/ChatClient");
			IMappingClient newClient = (IMappingClient) Activator.GetObject(typeof(IMappingClient), "tcp://localhost:" + NewClientName + "/ChatClient");
			clients.Add(newClient);
			return messages;
		}

		public void SubmitMapping(string mensagem){
			messages.Add(mensagem);
			ThreadStart ts = new ThreadStart(this.BroadcastMessage);
			Thread t = new Thread(ts);
			t.Start();
		}

		private void BroadcastMessage() {
            string MsgToBcast;
            lock (this) {
                MsgToBcast = messages[messages.Count - 1];
            }
			for (int i = 0; i < clients.Count ; i++) {
				try {
                    ((IMappingClient)clients[i]).MsgToClient(MsgToBcast);}
				catch (Exception e) {
                    Console.WriteLine("Failed sending message to client. Removing client. " + e.Message);
					clients.RemoveAt(i);
				}
			}
		}
	}
}
