using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CableCloud
{
    class Program
    {
        public static int WINDOW_APP_PORT = 6667;
        static void Main(string[] args)
        {
            CloudLogic logic = new CloudLogic();
            logic.connectToWindowApplication(WINDOW_APP_PORT);
        }
    }
}
