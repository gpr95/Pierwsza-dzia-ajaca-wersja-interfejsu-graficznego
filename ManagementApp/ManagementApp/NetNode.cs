using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{

    class NetNode : ContainerElement
    {
        public NetNode(int x , int y,String name)
        {
            containedPoints = new List<Point>();
            containedPoints.Add(new Point(x, y));
            this.name = name;
        }
    }
}
