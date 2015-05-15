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
        public IList<KeyValuePair<string, string>> Map(string fileLine)
        {
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>("Key", fileLine));
            return result;
        }
    }
    public class ParadiseCountMapper : IMapper
    {
        static uint lineNo = 0;
        public IList<KeyValuePair<string, string>> Map(string fileLine)
        {
            lineNo++;
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            if (fileLine.Contains("Paradise"))
            {
                result.Add(new KeyValuePair<string, string>(lineNo.ToString(), "1"));
            }
            return result;
        }
    }
    
}
