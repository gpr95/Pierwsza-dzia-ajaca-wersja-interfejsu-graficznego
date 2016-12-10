using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace ManagementApp
{
    [Serializable()]
    public class Domain
    {
        private const int GAP = 10;
        private int width;
        private int height;
        private Point pointTo;
        private Point pointFrom;
        private Size size;
        private String name { get; set; }

        public Domain(int width, int height, Point pointTo, Point pointFrom, Size size, String name)
        {
            this.width = width;
            this.height = height;
            this.pointTo = pointTo;
            this.pointFrom = pointFrom;
            this.size = size;
            this.name = name;
        }

        public Domain(Domain d) : this(d.Width, d.Height, d.PointTo, d.PointFrom, d.Size, d.name)
        {
        }


        public Domain(Point from, Point to)
        {
            this.pointFrom = from;
            this.pointTo = to;
            Size = new Size(pointTo.X - pointFrom.X, pointTo.Y - pointFrom.Y);
            //containedPoints = new List<Point>();
            //int xFrom = from.X >= to.X ? from.X:to.X;
            //int xTo = from.X >= to.X ? to.X : from.X;
            //int yFrom = from.Y >= to.Y ? from.Y : to.Y;
            //int yTo = from.Y >= to.Y ? to.Y : from.Y;
            //this.width = xFrom - xTo;
            //this.height = yFrom - yTo;
            //while (xFrom >= xTo)
            //{
            //    containedPoints.Add(new Point(xFrom, yFrom));
            //    containedPoints.Add(new Point(xFrom, yTo));
            //    xFrom -= GAP;
            //}
            //xFrom = from.X >= to.X ? from.X : to.X;
            //while (yFrom > yTo)
            //{
            //    yFrom -= GAP;
            //    containedPoints.Add(new Point(xFrom, yFrom));
            //    containedPoints.Add(new Point(xTo, yFrom));
            //}
        }

        public bool crossingOtherDomain(Domain other)
        {
            if (pointFrom.X > other.pointFrom.X && pointTo.X < other.pointTo.X &&
                pointFrom.Y > other.pointFrom.Y && pointTo.Y < other.pointTo.Y)
                return false;
            else if (pointFrom.X < other.pointTo.X && pointTo.X > other.pointFrom.X &&
                pointFrom.Y < other.pointTo.Y && pointTo.Y > other.pointFrom.Y)
                return true;
            else
                return false;
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

        public Size Size
        {
            get
            {
                return size;
            }

            set
            {
                size = value;
            }
        }
    }
}
