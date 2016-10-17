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
        protected String name;
        protected const int GAP = 10;
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

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
    }
}
