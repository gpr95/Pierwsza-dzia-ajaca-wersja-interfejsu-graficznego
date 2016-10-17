using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    class ClientNode : ContainerElement
    {
        public ClientNode(int x, int y)
        {
            containedPoints = new List<Point>();
            containedPoints.Add(new Point(x, y));
        }
    }
}
