using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiPingEnumerator
{
    // Used to store concentrator data for file export
    public class ConcentratorReport
    {
        public string IpAddress { get; set; }
        public bool IsOnline { get; set; }
        public double Latency { get; set; }
        public List<string> Collectors { get; set; } = new List<string>();
    }
}
