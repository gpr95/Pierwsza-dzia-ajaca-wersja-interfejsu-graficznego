using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CableCloud
{
    class Program
    {
        static void Main(string[] args)
        {   
            CloudLogic logic = new CloudLogic();
            // logic.connectToWindowApplication(int.Parse(args[0]));
            logic.connectToNodes(10001, 1, 10002, 1);
        }
    }
}
