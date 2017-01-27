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
            /** ARGS
             * 0 - domain ID
             * 1 - NCC Listener
             * 2 - Management Port
             * */
            NetworkCallControl ncc = new NetworkCallControl(args);
        }
    }
}
