using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KMB_Coin.Models
{ 
    public class Input
    {
        public DateTime Timestamp { get; set; }
        public long Amount { get; set; }
        public string Address { get; set; }
        public string Signature { get; set; }


        public Input()
        {
            //Console.WriteLine("");

        }
    }
}
