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
            string[] parameters = new string[] { "1", "8000", "1234" };
            client = new ClientNode(parameters);
        }
    }
}
