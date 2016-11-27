using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementApp
{
    public class ClientNode : Node
    {
        public ClientNode(int x, int y,String name, int input, int output)
            {
                string parameters = "127.0.0.1" + " " + input + " " + output;
                //System.Diagnostics.Process.Start("ClientNode.exe", parameters);
                Position = new Point(x, y);
                this.Name = name;
            }
    }
}
