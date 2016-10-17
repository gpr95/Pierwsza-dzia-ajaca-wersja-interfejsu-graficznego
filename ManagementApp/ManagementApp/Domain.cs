using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    class Domain : ContainerElement
    {
        private Point pointFrom;
        private int width;
        private int height;
        private Point pointTo;
        public Domain(Point from, Point to)
        {
            this.pointFrom = from;
            this.pointTo = to;

            containedPoints = new List<Point>();
            int xFrom = from.X >= to.X ? from.X:to.X;
            int xTo = from.X >= to.X ? to.X : from.X;
            int yFrom = from.Y >= to.Y ? from.Y : to.Y;
            int yTo = from.Y >= to.Y ? to.Y : from.Y;
            this.width = xFrom - xTo;
            this.height = yFrom - yTo;
            while (xFrom >= xTo)
            {
                containedPoints.Add(new Point(xFrom, yFrom));
                containedPoints.Add(new Point(xFrom, yTo));
                xFrom -= GAP;
            }
            xFrom = from.X >= to.X ? from.X : to.X;
            while (yFrom > yTo)
            {
                yFrom -= GAP;
                containedPoints.Add(new Point(xFrom, yFrom));
                containedPoints.Add(new Point(xTo, yFrom));
            }
        }

        public Point PointFrom
        {
            get
            {
                return pointFrom;
            }

            set
            {
                pointFrom = value;
            }
        }

        public Point PointTo
        {
            get
            {
                return pointTo;
            }

            set
            {
                pointTo = value;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }

            set
            {
                height = value;
            }
        }
    }
}
