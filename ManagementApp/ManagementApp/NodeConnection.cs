using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    [Serializable()]
    public class NodeConnection
    {
        private Point start;
        private Point end;
        private String name;
        private Node from, to;
        private List<int> occupiedSlots = new List<int>();
        private List<int> autoOccupiedSlots = new List<int>();
        private int virtualPortFrom;
        private int virtualPortTo;
        private int localPortFrom;
        private int localPortTo;
        private ConnectionProperties prop;

        public NodeConnection(Node from, int virtualPortFrom, Node to, int virtualPortTo, String name)
        {
            this.virtualPortFrom = virtualPortFrom;
            this.virtualPortTo = virtualPortTo;
            this.From = from;
            this.To = to;
            this.Name = name;
            this.Prop = new ConnectionProperties(from.LocalPort, virtualPortFrom, to.LocalPort, virtualPortTo);

            localPortFrom = from.LocalPort;
            localPortTo = to.LocalPort;
            Start = from.Position;
            End = to.Position;
        }

        public Point Start
        {
            get
            {
                return start;
            }

            set
            {
                start = value;
            }
        }

        public Point End
        {
            get
            {
                return end;
            }

            set
            {
                end = value;
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

        public Node From
        {
            get
            {
                return from;
            }

            set
            {
                from = value;
            }
        }

        public Node To
        {
            get
            {
                return to;
            }

            set
            {
                to = value;
            }
        }

        public int VirtualPortFrom
        {
            get {  return virtualPortFrom; }
            set { virtualPortFrom = value; }
        }

        public int VirtualPortTo
        {
            get {  return virtualPortTo; }
            set { virtualPortTo = value; }
        }

        public int LocalPortFrom
        {
            get { return localPortFrom; }
            set { localPortFrom = value; }
        }

        public int LocalPortTo
        {
            get { return localPortTo; }
            set { localPortTo = value; }
        }

        internal ConnectionProperties Prop
        {
            get
            {
                return prop;
            }

            set
            {
                prop = value;
            }
        }

        public List<int> OccupiedSlots
        {
            get
            {
                return occupiedSlots;
            }

            set
            {
                occupiedSlots = value;
            }
        }
        public List<int> AutoOccupiedSlots
        {
            get
            {
                return autoOccupiedSlots;
            }

            set
            {
                autoOccupiedSlots = value;
            }
        }
    }
}
