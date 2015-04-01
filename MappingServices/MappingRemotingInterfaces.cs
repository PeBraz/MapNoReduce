using System;
using System.Collections.Generic;
//using MappingServices;

namespace MappingRemotingInterfaces
{
	public interface IMappingServer {
        List<string> RegisterClient(string NewClientPort);
		void SubmitMapping(string message);
	}

	public interface IMappingClient {
		void MsgToClient(string message);
	}

    public interface IMap {
        //Returns a set of key-value pairs for each input key-value pair
        KeyValuePair<String, String> Map(String key, String value);
    }
}
