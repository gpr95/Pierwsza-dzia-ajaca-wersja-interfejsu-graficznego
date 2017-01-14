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
            string[] parameters = new string[] { args[0], args[1] };

            ManagementPlane management = new ManagementPlane(parameters);
        }
    }
}
