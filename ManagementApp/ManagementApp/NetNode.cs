using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementApp
{

    public class NetNode : Node
    {
        public NetNode(int x , int y,String name)
        {
            Position = new Point(x, y);
            this.name = name;
        }
    }
}
