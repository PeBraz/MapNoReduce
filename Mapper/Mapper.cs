using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce;

namespace Mapper
{
    public class Map : IMap 
    {
        public ISet<KeyValuePair<string, string>> map(string fileLine)
        {
            ISet<KeyValuePair<string, string>> result = new HashSet<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>("testKey1", fileLine));
            return result;
        }
    }
    
}
