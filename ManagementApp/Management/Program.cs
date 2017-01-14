using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Management
{
    class Program
    {
        static void Main(string[] args)
        {
            //string[] parameters = new string[] { args[0], args[1] };
            string[] parameters = new string[] { "7777", "7778" };
            ManagementPlane management = new ManagementPlane(parameters);
        }
    }
}
