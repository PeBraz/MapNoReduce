using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce;

namespace Mapper
{
    public class Mapper : IMapper 
    {
        public ISet<KeyValuePair<string, string>> Map(string fileLine)
        {
            ISet<KeyValuePair<string, string>> result = new HashSet<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>("Key", fileLine));
            return result;
        }
    }
    
}
