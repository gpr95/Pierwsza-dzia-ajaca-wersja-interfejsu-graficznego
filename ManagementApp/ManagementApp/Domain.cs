using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace ManagementApp
{
    [Serializable()]
    public class Domain
    {
        private Point pointTo;
        private Point pointFrom;
        private Size size;
        private int name;
        private Process processHandle;
        private List<String> containedNodes = new List<string>();
        private int managementPort;
        private int controlPort;
        private int numberOfNodes = 0;

        private ManagementHandler managementHandler;

        public Domain(Point pointFrom, Point pointTo, int name)
        {
            this.pointTo = pointTo;
            this.pointFrom = pointFrom;
            this.size = new Size(Math.Abs(pointFrom.X - pointTo.X), Math.Abs(pointFrom.Y - pointTo.Y));
            this.Name = name;
        }

        public Domain(Domain d) : this(d.PointTo, d.PointFrom, d.Name)
        {
        }


        //public Domain(Point from, Point to, int name)
        //{
        //    this.pointFrom = from;
        //    this.pointTo = to;
        //    Size = new Size(pointTo.X - pointFrom.X, pointTo.Y - pointFrom.Y);

        //    //TO DO ???

        //    //containedPoints = new List<Point>();
        //    //int xFrom = from.X >= to.X ? from.X:to.X;
        //    //int xTo = from.X >= to.X ? to.X : from.X;
        //    //int yFrom = from.Y >= to.Y ? from.Y : to.Y;
        //    //int yTo = from.Y >= to.Y ? to.Y : from.Y;
        //    //this.width = xFrom - xTo;
        //    //this.height = yFrom - yTo;
        //    //while (xFrom >= xTo)
        //    //{
        //    //    containedPoints.Add(new Point(xFrom, yFrom));
        //    //    containedPoints.Add(new Point(xFrom, yTo));
        //    //    xFrom -= GAP;
        //    //}
        //    //xFrom = from.X >= to.X ? from.X : to.X;
        //    //while (yFrom > yTo)
        //    //{
        //    //    yFrom -= GAP;
        //    //    containedPoints.Add(new Point(xFrom, yFrom));
        //    //    containedPoints.Add(new Point(xTo, yFrom));
        //    //}
        //}

        internal void setupManagement(int mANAGPORT, int v)
        {
            managementHandler = new ManagementHandler(mANAGPORT, v);
            this.ManagementPort = v;

            ProcessStartInfo startInfo = new ProcessStartInfo("ControlNCC.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            this.NccPort = PortAggregation.NccPort;
            startInfo.Arguments = name + " " + this.NccPort;
            this.ProcessHandle = Process.Start(startInfo);

            startInfo = new ProcessStartInfo("ControlCCRC.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            this.ControlPort = PortAggregation.CcRcPort;
            startInfo.Arguments = name + " " + this.ControlPort;
            this.ProcessHandle = Process.Start(startInfo);
        }

        public Point getPointStart()
        {
            return new Point(pointFrom.X > pointTo.X ? pointTo.X : pointFrom.X,
                pointFrom.Y > pointTo.Y ? pointTo.Y : pointFrom.Y);
        }

        public bool crossingOtherDomain(Domain other)
        {
            if (pointFrom.X < other.pointTo.X && pointTo.X > other.pointFrom.X &&
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

        public Process ProcessHandle
        {
            get
            {
                return processHandle;
            }

            set
            {
                processHandle = value;
            }
        }

        public int Name
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

        public int ManagementPort
        {
            get
            {
                return managementPort;
            }

            set
            {
                managementPort = value;
            }
        }

        public int ControlPort
        {
            get
            {
                return controlPort;
            }

            set
            {
                controlPort = value;
            }
        }

        public int NumberOfNodes
        {
            get
            {
                return numberOfNodes;
            }

            set
            {
                numberOfNodes = value;
            }
        }

        public int NccPort { get; set; }
    }
}
