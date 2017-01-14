using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Management
{
    [Serializable()]
    public class NodeConnection
    {
        private String name;

        private String from;
        private String to;

        [NonSerialized]
        private List<int> occupiedSlots = new List<int>();
        [NonSerialized]
        private List<int> autoOccupiedSlots = new List<int>();
        private int virtualPortFrom;
        private int virtualPortTo;
        private int localPortFrom;
        private int localPortTo;

        public NodeConnection(Node from, int virtualPortFrom, Node to, int virtualPortTo, String name)
        {
            this.virtualPortFrom = virtualPortFrom;
            this.virtualPortTo = virtualPortTo;
            this.From = from.Name;
            this.To = to.Name;
            this.Name = name;

            localPortFrom = from.LocalPort;
            localPortTo = to.LocalPort;
        }

        public NodeConnection(String from, int virtualPortFrom, String to, int virtualPortTo, String name)
        {
            this.virtualPortFrom = virtualPortFrom;
            this.virtualPortTo = virtualPortTo;
            this.From = from;
            this.To = to;
            this.Name = name;
        }

        public NodeConnection(NodeConnection nc) : this(nc.From, nc.virtualPortFrom, nc.To, nc.VirtualPortTo, nc.Name) { }

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

        public String From
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

        public String To
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
