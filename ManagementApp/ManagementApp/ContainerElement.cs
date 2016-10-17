using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    class ContainerElement
    {
        protected List<Point> containedPoints;
        public List<Point> ContainedPoints
        {
            get
            {
                return containedPoints;
            }

            set
            {
                containedPoints = value;
            }
        }
    }
}
