using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    [Serializable()]
    class Trail
    {
        private Node from;
        private Node to;
        private Priority priority;
        private String name;
        private int startingSlot;
        private int endingSlot;
        private int portFrom;
        private int portTo;
        private Dictionary<NodeConnection, int> connectionDictionary = new Dictionary<NodeConnection, int>();
        private List<Node> componentNodes;
        private List<Point> points = new List<Point>();
        private Dictionary<Node, FIB> componentFIBs = new Dictionary<Node, FIB>();
        //private Dictionary<Node, FIB> componentFIBsTo = new Dictionary<Node, FIB>();

        enum Priority
        {
            USER_CREATED,
            AUTO
        }

        public bool isCreadetByUser()
        {
            if (priority == Priority.USER_CREATED)
                return true;
            else
                return false;
        }


        public Trail(bool createdByUser)
        {
            if (createdByUser)
                priority = Priority.USER_CREATED;
            else
                priority = Priority.AUTO;
        }

        public Trail(List<Node> path,
            List<NodeConnection> con,
            bool createdByUser,
            bool vc4 = false)
        {
            if (path == null)
                return;
            this.from = path.First();
            this.to = path.Last();
            this.Name = from.Name + "<>" + to.Name;
            this.componentNodes = new List<Node>(path);
            if (createdByUser)
            {
                priority = Priority.USER_CREATED;

                int portIn, portOut;
                int slot = 0;
                for (int n = 0; n < path.Count(); n++)
                {
                    points.Add(path.ElementAt(n).Position);
                    if (n == 0)
                    {
                        //Start of path
                        portFrom = findConnection(path.ElementAt(0), path.ElementAt(1), con).From.Equals(path.ElementAt(0)) ?
                            findConnection(path.ElementAt(0), path.ElementAt(1), con).VirtualPortFrom :
                            findConnection(path.ElementAt(0), path.ElementAt(1), con).VirtualPortTo;
                        StartingSlot = findFirstFreeSlot(findConnection(from, path.ElementAt(n + 1), con), vc4);
                        slot = StartingSlot;
                        if (slot == -1)
                        {
                            clearTrail(this);
                            break;
                        }
                        findConnection(from, path.ElementAt(n + 1), con).OccupiedSlots.Add(slot);
                        findConnection(from, path.ElementAt(n + 1), con).AutoOccupiedSlots.Add(slot);
                        connectionDictionary.Add(findConnection(from, path.ElementAt(n + 1), con), slot);
                        continue;
                    }
                    if (n == path.Count() - 1)
                    {
                        //End of path
                        portTo = findConnection(path.ElementAt(n - 1), path.ElementAt(n), con).To.Equals(path.ElementAt(n)) ?
                            findConnection(path.ElementAt(n - 1), path.ElementAt(n), con).VirtualPortTo :
                            findConnection(path.ElementAt(n - 1), path.ElementAt(n), con).VirtualPortFrom;
                        continue;
                    }
                    NodeConnection conIn = findConnection(path.ElementAt(n - 1), path.ElementAt(n), con);
                    NodeConnection conOut = findConnection(path.ElementAt(n), path.ElementAt(n + 1), con);
                    portIn = conIn.To.Equals(path.ElementAt(n)) ? conIn.VirtualPortTo : conIn.VirtualPortFrom;
                    portOut = conOut.From.Equals(path.ElementAt(n)) ? conOut.VirtualPortFrom : conOut.VirtualPortTo;
                    int slotTemp = findFirstFreeSlot(conOut, vc4);
                    if (slotTemp == -1)
                    {
                        clearTrail(this);
                        break;
                    }

                    if (slot == -1)
                    {
                        clearTrail(this);
                        break;
                    }
                    endingSlot = slotTemp;
                    //StartingSlot = startinS;
                    FIB newFibFrom = new FIB(portIn, slot, portOut, slotTemp);
                    FIB newFibTo = new FIB(portOut, slotTemp, portIn, slot);
                    slot = slotTemp;
                    findConnection(path.ElementAt(n), path.ElementAt(n + 1), con).OccupiedSlots.Add(slot);
                    findConnection(path.ElementAt(n), path.ElementAt(n + 1), con).AutoOccupiedSlots.Add(slot);
                    connectionDictionary.Add(findConnection(path.ElementAt(n), path.ElementAt(n + 1), con), slot);
                    ComponentFIBs.Add(path.ElementAt(n), newFibFrom);
                    //ComponentFIBs.Add(path.ElementAt(n), newFibTo);
                }
            }
            else
            {
                priority = Priority.AUTO;

                int portIn, portOut;
                int slot = 0;
                for (int n = 0; n < path.Count(); n++)
                {
                    points.Add(path.ElementAt(n).Position);
                    if (n == 0)
                    {
                        //Start of path
                        portFrom = findConnection(path.ElementAt(0), path.ElementAt(1), con).From.Equals(path.ElementAt(0)) ?
                           findConnection(path.ElementAt(0), path.ElementAt(1), con).VirtualPortFrom :
                           findConnection(path.ElementAt(0), path.ElementAt(1), con).VirtualPortTo;
                        StartingSlot = findFirstAutoFreeSlot(findConnection(from, path.ElementAt(n + 1), con));
                        slot = StartingSlot;
                        NodeConnection connection =
                        findConnection(from, path.ElementAt(n + 1), con);
                        connection.AutoOccupiedSlots.Add(slot);
                        connectionDictionary.Add(connection, slot);
                        continue;
                    }
                    if (n == path.Count() - 1)
                    {
                        //End of path
                        portTo = findConnection(path.ElementAt(n - 1), path.ElementAt(n), con).To.Equals(path.ElementAt(n)) ?
                            findConnection(path.ElementAt(n - 1), path.ElementAt(n), con).VirtualPortTo :
                            findConnection(path.ElementAt(n - 1), path.ElementAt(n), con).VirtualPortFrom;
                        continue;
                    }
                    NodeConnection conIn = findConnection(path.ElementAt(n - 1), path.ElementAt(n), con);
                    NodeConnection conOut = findConnection(path.ElementAt(n), path.ElementAt(n + 1), con);
                    portIn = conIn.To.Equals(path.ElementAt(n)) ? conIn.VirtualPortTo : conIn.VirtualPortFrom;
                    portOut = conOut.From.Equals(path.ElementAt(n)) ? conOut.VirtualPortFrom : conOut.VirtualPortTo;
                    int slotTemp = findFirstAutoFreeSlot(conOut);
                    if (slotTemp == -1)
                    {
                        clearTrail(this);
                        break;
                    }

                    if (slot == -1)
                    {
                        clearTrail(this);
                        break;
                    }
                    //StartingSlot = startinS;
                    FIB newFib = new FIB(portIn, slot, portOut, slotTemp);
                    slot = slotTemp;
                    NodeConnection connectionTemp2 =
                    findConnection(path.ElementAt(n), path.ElementAt(n + 1), con);
                    connectionTemp2.AutoOccupiedSlots.Add(slot);
                    connectionDictionary.Add(connectionTemp2, slot);
                    ComponentFIBs.Add(path.ElementAt(n), newFib);
                    
                }
            }
            
        }

        private NodeConnection findConnection(Node start, Node end, List<NodeConnection> con)
        {
            if (con.Where(n => n.From.Equals(start) && n.To.Equals(end)).Any())
                return con.Where(n => n.From.Equals(start) && n.To.Equals(end)).FirstOrDefault();
            else
                return con.Where(n => n.From.Equals(end) && n.To.Equals(start)).FirstOrDefault();
        }

        private int findFirstFreeSlot(NodeConnection connection, bool vc4 = false)
        {
            if (vc4)
            {
                if (!connection.OccupiedSlots.Any())
                    return 1;
                else
                    if (connection.OccupiedSlots.Min() != 1)
                        return 1;
                    else
                        return -1;
            }
            else
            {
                if (!connection.OccupiedSlots.Any())
                    return 11;
                else if (connection.OccupiedSlots.Max() >= 13)
                    return -1;
                else if (connection.OccupiedSlots.Min() == 1)
                    return -1;
                else
                    return connection.OccupiedSlots.Max() + 1;
            }

        }

        private int findFirstAutoFreeSlot(NodeConnection connection)
        {
            if (!connection.AutoOccupiedSlots.Any())
                return 11;
            else if (connection.AutoOccupiedSlots.Max() >= 13)
                return -1;
            else
                return connection.AutoOccupiedSlots.Max() + 1;
        }

        public void clearTrail(Trail trail)
        {
            if (!componentFIBs.Any())
            {
                trail.from = null;
                trail.to = null;
                //return;
            }
            foreach (KeyValuePair<NodeConnection, int> kvp in trail.connectionDictionary)
            {
                kvp.Key.OccupiedSlots.Remove(kvp.Value);
                kvp.Key.AutoOccupiedSlots.Remove(kvp.Value);
            }
            trail.from = null;
        }

        public String toString()
        {
            String o = null;
            if (this.Name != null)
                o = "Trail: " + this.Name;
            else
                o = "Trail was not created.";
            foreach (KeyValuePair<Node, FIB> ff in componentFIBs)
            {
                o = o + System.Environment.NewLine + ff.Key.Name + ":: " + ff.Value.toString();
            }
            return o;
        }

        //Getters and Setters
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

        public Dictionary<NodeConnection, int> ConnectionDictionary
        {
            get
            {
                return connectionDictionary;
            }

            set
            {
                connectionDictionary = value;
            }
        }

        public List<Point> Points
        {
            get
            {
                return points;
            }

            set
            {
                points = value;
            }
        }

        public int PortFrom
        {
            get
            {
                return portFrom;
            }

            set
            {
                portFrom = value;
            }
        }

        public int PortTo
        {
            get
            {
                return portTo;
            }

            set
            {
                portTo = value;
            }
        }

        public List<Node> ComponentNodes
        {
            get
            {
                return componentNodes;
            }

            set
            {
                componentNodes = value;
            }
        }

        public int StartingSlot
        {
            get
            {
                return startingSlot;
            }

            set
            {
                startingSlot = value;
            }
        }

        public Dictionary<Node, FIB> ComponentFIBs
        {
            get
            {
                return componentFIBs;
            }

            set
            {
                componentFIBs = value;
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

        public int EndingSlot
        {
            get
            {
                return endingSlot;
            }

            set
            {
                endingSlot = value;
            }
        }
    }
}
