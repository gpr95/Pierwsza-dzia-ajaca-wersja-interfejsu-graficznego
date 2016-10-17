using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    class NodeConnection : ContainerElement
    {
        public NodeConnection(ContainerElement from, ContainerElement to)
        {
            containedPoints = new List<Point>();
            containedPoints.Add(from.ContainedPoints.ElementAt(0));
            containedPoints.Add(to.ContainedPoints.ElementAt(0));
        }
    }
}
