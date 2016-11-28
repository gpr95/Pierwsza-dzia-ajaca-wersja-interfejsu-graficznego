using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNode
{
    class Program
    {

        private static ClientNode client = null;
        static void Main(string[] args)
        {
            string[] parameters = new string[] { args[0], args[1], args[2] };
            client = new ClientNode(args);
        }
    }
}
