using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    class WindowController
    {
        private CloudCableHandler cableHandler;
        private MainWindow mainWindow;
        private ManagementHandler managHandler;

        private readonly int MANAGPORT = PortAggregation.ManagementPort;
        private readonly int CLOUDPORT = PortAggregation.CableCloudPort;
        private readonly int nodeConnectionPort = PortAggregation.NetPort;
        public readonly int NETNODECONNECTIONS = 21;

        private int clientNodesNumber = 0;
        private int networkNodesNumber = 0;
        private int domainNumber = 0;
        private int subNumber = 0;
        private List<int> subNetworkNumber = new List<int>();
        private List<Node> nodeList = new List<Node>();
        private List<Domain> domainList = new List<Domain>();
        private List<Subnetwork> subnetworkList = new List<Subnetwork>();
        private List<NodeConnection> connectionList = new List<NodeConnection>();

        public WindowController(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            mainWindow.setLists(nodeList, domainList, subnetworkList, connectionList);
            cableHandler = new CloudCableHandler(connectionList, CLOUDPORT);
            managHandler = new ManagementHandler(MANAGPORT, nodeConnectionPort);
        }

        internal void addClient(Point point)
        {
            foreach (Node node in nodeList)
                if (node.Position.Equals(point))
                {
                    mainWindow.errorMessage("There is already node in that position.");
                    return;
                }
            Address a;
            Node client;
            Domain d = checkWhatDomain(point);
            if(d == null)
            {
                a = new Address(true, 0, 0, clientNodesNumber);
                client = new Node(point, Node.NodeType.CLIENT, a.getName(), 8000 + clientNodesNumber, 0, 0);
            } else
            {
                List<Subnetwork> temporaryListOfSubnetworks = checkWhatSubnetwork(point);
                Subnetwork up = default(Subnetwork);
                if (temporaryListOfSubnetworks.Any())
                {
                    up = temporaryListOfSubnetworks.ElementAt(0);
                    foreach (Subnetwork s in temporaryListOfSubnetworks)
                    {
                        if (s.Size.Height < up.Size.Height &&
                            s.Size.Width < up.Size.Width)
                        {
                            up = s;
                        }
                    }
                }
                d.NumberOfNodes++;
                if (up == default(Subnetwork))
                {
                    a = new Address(false, d.Name, 0, d.NumberOfNodes);
                }
                else
                {
                    a = new Address(false, d.Name, up.Name - 100, d.NumberOfNodes);
                }
                client = new Node(point, Node.NodeType.CLIENT, a.getName(), 8000 + clientNodesNumber, d.ManagementPort, 0, d.NccPort);
            }
            ++clientNodesNumber;
            nodeList.Add(client);
            mainWindow.addNodeToTable(client);
            mainWindow.addNode(client);
        }

        internal void addNetwork(Point point)
        {
            foreach (Node node in nodeList)
                if (node.Position.Equals(point))
                {
                    mainWindow.errorMessage("There is already node in that position.");
                    return;
                }
            Address a;
            Node network;
            Domain d = checkWhatDomain(point);
            if (d == null)
            {
                a = new Address(false, 0, 0,networkNodesNumber);
                network = new Node(point, Node.NodeType.NETWORK, a.getName(), 8500 + networkNodesNumber, MANAGPORT, 0);
            }
            else
            {
                List<Subnetwork> temporaryListOfSubnetworks = checkWhatSubnetwork(point);
                Subnetwork up = default(Subnetwork);
                if (temporaryListOfSubnetworks.Any())
                {
                    up = temporaryListOfSubnetworks.ElementAt(0);
                    foreach (Subnetwork s in temporaryListOfSubnetworks)
                    {
                        if (s.Size.Height < up.Size.Height &&
                            s.Size.Width < up.Size.Width)
                        {
                            up = s;
                        }
                    }
                }
                d.NumberOfNodes++;
                if(up == default(Subnetwork))
                {
                    a = new Address(false, d.Name, 0, d.NumberOfNodes);
                    network = new Node(point, Node.NodeType.NETWORK, a.getName(), 8500 + networkNodesNumber, d.ManagementPort, d.ControlPort);
                }
                else
                {
                    a = new Address(false, d.Name, up.Name - 100, d.NumberOfNodes);
                    network = new Node(point, Node.NodeType.NETWORK, a.getName(), 8500 + networkNodesNumber, d.ManagementPort, up.ControlPort);
                }
                   
            }
            ++networkNodesNumber;
            nodeList.Add(network);
            mainWindow.addNodeToTable(network);
            mainWindow.addNode(network);
        }

        public Boolean isNumberOfNodeConnectionsLessThenPossible(Node n)
        {
            return connectionList.Where(i => i.From.Equals(n.Name) || i.To.Equals(n.Name)).Count() >= NETNODECONNECTIONS;
        }

        //CONNECTIONS
        internal void deleteConnection(NodeConnection con)
        {
            cableHandler.deleteConnection(con);
            removeConnection(con);
        }

        public void removeConnection(NodeConnection conn)
        {
            connectionList.RemoveAt(connectionList.IndexOf(conn));
        }

        internal void removeConnection(int idxOfElement)
        {
            cableHandler.deleteConnection(connectionList.ElementAt(idxOfElement));
            removeConnection(connectionList.ElementAt(idxOfElement));
        }

        internal void updateCableCloud()
        {
            cableHandler.updateConnections(connectionList);
        }

        internal void updateOneConnection()
        {
            cableHandler.updateOneConnection();
        }

        //CLOSING APPLICATION
        internal void formClosing()
        {
            managHandler.stopRunning();
            cableHandler.stopRunning();
        }

        internal void restartNode(string v)
        {
            Node n = nodeList.Where(s => s.Name.Equals(v)).FirstOrDefault();
            if (n.ProcessHandle.HasExited)
            {
                nodeList.Remove(n);
                if (n is Node)
                    n = new Node((Node)n);
                nodeList.Add(n);
                //List<string> conL = findElemAtPosition(n.Position.X, n.Position.Y);
                foreach (NodeConnection c in connectionList)
                {
                    if (c.From.Equals(n.Name))
                        c.From = n.Name;
                    if (c.To.Equals(n.Name))
                        c.To = n.Name;
                }
                updateCableCloud();

                //TODO Update management
            }
        }

        internal void addDomainToQueue(Point domainFrom, Point domainTo)
        {
            bool add = true;
            //int GAP = GAP;
            Domain toAdd = new Domain(domainFrom, domainTo, ++domainNumber);
            foreach (Domain d in domainList)
            {
                if (toAdd.crossingOtherDomain(d))
                {
                    add = false;
                    break;
                }
            }
            if (toAdd.Size.Width < MainWindow.GAP || toAdd.Size.Height < MainWindow.GAP)
                add = false;
            if (add)
            {
                checkDomainContent(toAdd);
                //if (domainNumber == 1)
                //    managHandler.killManagement();
                toAdd.setupManagement(MANAGPORT + toAdd.Name, PortAggregation.ManagementNodePort);
                domainList.Add(toAdd);
                mainWindow.consoleWriter("Domain added");
            }
            else
            {
                mainWindow.errorMessage("Domains can't cross each others or domain too small for rendering.");
            }
        }

        private void checkDomainContent(Domain domain)
        {
            Rectangle domainRect = new Rectangle(domain.getPointStart(), domain.Size);
            foreach (Node n in nodeList)
            {
                if (domainRect.Contains(n.Position))
                    mainWindow.errorMessage(n.Name + " is in domain.");
            }

        }

        private Domain checkWhatDomain(Point p)
        {
            foreach (Domain d in domainList)
            {
                Rectangle domainRect = new Rectangle(d.getPointStart(), d.Size);
                if (domainRect.Contains(p))
                    return d;
            }
            return null;

        }

        internal Boolean addSubnetworkToQueue(Point subFrom, Point subTo)
        {
            bool add = true;
            
            Domain subnetworkStart = checkWhatDomain(subFrom);
            Domain subnetworkStop = checkWhatDomain(subTo);
            if (subnetworkStart == null || subnetworkStop == null)
            {
                return false;
            }
            if(subnetworkStart.Name != subnetworkStop.Name)
                add = false;
            
            List<Subnetwork> tempListOfSubnetworks = checkWhatSubnetwork(subFrom, new Point(subFrom.X, subTo.Y), subTo, new Point(subTo.X, subFrom.Y));
            if (tempListOfSubnetworks == null) { add = false;}
            Size tempSize = new Size(Math.Abs(subFrom.X - subTo.X), Math.Abs(subFrom.Y - subTo.Y));
            if (tempSize.Width < MainWindow.GAP || tempSize.Height < MainWindow.GAP)
                add = false;
            if (add)
            {
                ++subNumber;
                Subnetwork toAdd = new Subnetwork(subFrom, subTo, subnetworkStart.Name * 100 + subNumber);
                checkSubnetworkContent(toAdd);
                Subnetwork up = default(Subnetwork);
                if(tempListOfSubnetworks.Any())
                {
                    up = tempListOfSubnetworks.ElementAt(0);
                    foreach (Subnetwork s in tempListOfSubnetworks)
                    {
                        if (s.Size.Height < up.Size.Height &&
                            s.Size.Width < up.Size.Width)
                        {
                            up = s;
                        }
                    }
                }
                if(up == default(Subnetwork))
                    toAdd.setupControl(subnetworkStart);
                else
                    toAdd.setupControl(up);
                subnetworkList.Add(toAdd);
                mainWindow.consoleWriter("Subnetwork added " + toAdd.Name);
                return true;
            }
            else
            {
                mainWindow.errorMessage("Subnetwork can't cross each others or domain too small for rendering.");
                return false;
            }
        }

        private void checkSubnetworkContent(Subnetwork domain)
        {
            Rectangle domainRect = new Rectangle(domain.getPointStart(), domain.Size);
            foreach (Node n in nodeList)
            {
                if (domainRect.Contains(n.Position))
                    mainWindow.errorMessage(n.Name + " is in Subnetwork.");
            }
        }

        private List<Subnetwork> checkWhatSubnetwork(Point p)
        {
            List<Subnetwork> output = new List<Subnetwork>();
            foreach (Subnetwork d in subnetworkList)
            {
                Rectangle domainRect = new Rectangle(d.getPointStart(), d.Size);
                if (domainRect.Contains(p))
                    output.Add(d);
            }
            return output;

        }

        private List<Subnetwork> checkWhatSubnetwork(Point a, Point b, Point c, Point d)
        {
            List<Subnetwork> output = new List<Subnetwork>();
            foreach (Subnetwork s in subnetworkList)
            {
                Rectangle domainRect = new Rectangle(s.getPointStart(), s.Size);
                if (domainRect.Contains(a) &&
                    domainRect.Contains(b) &&
                    domainRect.Contains(c) &&
                    domainRect.Contains(d))
                    output.Add(s);
                else if (domainRect.Contains(a) ||
                    domainRect.Contains(b) ||
                    domainRect.Contains(c) ||
                    domainRect.Contains(d))
                    return null;
            }
            return output;
        }
    }
}
