using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlNCC
{
    class Program
    {
        static void Main(string[] args)
        {
            string domainNumber = "1";
            NetworkCallControl ncc = new NetworkCallControl(domainNumber);
        }
    }
}
