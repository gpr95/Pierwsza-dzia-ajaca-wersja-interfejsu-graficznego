using NetNode;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagementApp
{
    class ControlPlane
    {
        MainWindow mainWindow;

        private DataTable table;
        private readonly int MANAGMENTPORT = 7777;
        private readonly int NETNODECONNECTIONS = 4;
        private int clientNodesNumber;
        private int networkNodesNumber;
        private bool run = true;
        private TcpListener listener;
        private static ManagmentProtocol protocol = new ManagmentProtocol();
        private FileSaver configuration = new FileSaver();
        //private List<ClientNode> clientNodeList = new List<ClientNode>();
        //private List<NetNode> networkNodeList = new List<NetNode>();
        private List<Node> nodeList = new List<Node>();
        private List<NodeConnection> connectionList = new List<NodeConnection>();
        private List<Domain> domainList = new List<Domain>();

        private class threadPasser
        {
            public ControlPlane control;
            public TcpClient client;
        }

        public ControlPlane()
        {
            mainWindow = new MainWindow(MakeTable(), nodeList, connectionList, domainList);
            mainWindow.Control = this;
            Application.Run(mainWindow);
            clientNodesNumber = 0;
            networkNodesNumber = 0;
            

            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), MANAGMENTPORT);
            Thread thread = new Thread(new ParameterizedThreadStart(Listen));
            thread.Start(this);
        }
        public void load()
        {
            nodeList = null;
            connectionList = null;
            domainList = null;
            nodeList = configuration.ReadFromBinaryFileNodes();
            connectionList = configuration.ReadFromBinaryFileNodeConnections();
            domainList = configuration.ReadFromBinaryFileDomains();
            mainWindow.updateLists(nodeList, connectionList, domainList);
        }
        private void Listen(Object controlP)
        {
            listener.Start();

            while (run)
            {
                TcpClient client = listener.AcceptTcpClient();
                threadPasser tp = new threadPasser();
                tp.client = client;
                tp.control = (ControlPlane) controlP;
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                clientThread.Start(tp);
            }
        }

        private static void ListenThread(Object threadPasser)
        {
            threadPasser tp = (threadPasser) threadPasser;
            TcpClient clienttmp = tp.client;
            BinaryWriter writer = new BinaryWriter(clienttmp.GetStream());
            //protocol.State = protocol.WHOIS;
            writer.Write(ManagmentProtocol.WHOIS);
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            string received_data = reader.ReadString();
            JSON received_object = JSON.Deserialize(received_data);
            ManagmentProtocol received_Protocol = received_object.Value.ToObject<ManagmentProtocol>();
            String nodeName = received_Protocol.Name;
            tp.control.allocateNode(nodeName, clienttmp, Thread.CurrentThread);
        }

        public void allocateNode(String nodeName, TcpClient nodePort, Thread nodeThreadHandle)
        {
            Node currentNode = nodeList.Where(i => i.Name.Equals(nodeName)).FirstOrDefault();
            currentNode.ThreadHandle = nodeThreadHandle;
            currentNode.TcpClient = nodePort;
        }

        private DataTable MakeTable()
        {
            table = new DataTable("threadManagment");
            var column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "id";
            column.AutoIncrement = false;
            column.Caption = "ParentItem";
            column.ReadOnly = true;
            column.Unique = false;
            // Add the column to the table.
            table.Columns.Add(column);


            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Type";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Name";
            column.ReadOnly = true;
            column.Unique = true;
            table.Columns.Add(column);

            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns["Name"];
            table.PrimaryKey = PrimaryKeyColumns;
            var dtSet = new DataSet();
            dtSet.Tables.Add(table);

            return table;
        }

        public void addClientNode(int x, int y)
        {
            foreach (Node node in nodeList)
                if (node.Position.Equals(new Point(x, y)))
                {
                    mainWindow.errorMessage("There is already node in that position.");
                    return;
                }
            ClientNode client = new ClientNode(x, y, "CN" + clientNodesNumber, 8000 + clientNodesNumber);

            nodeList.Add(client);
            var row = table.NewRow();
            row["id"] = clientNodesNumber;
            row["Type"] = "Client";
            row["Name"] = "CN" + clientNodesNumber++;
            table.Rows.Add(row);
            mainWindow.addNode(client);
        }

        public void addNetworkNode(int x, int y)
        {
            foreach (Node node in nodeList)
                if (node.Position.Equals(new Point(x, y)))
                {
                    mainWindow.errorMessage("There is already node in that position.");
                    return;
                }
            NetNode network = new NetNode(x, y, "NN" + networkNodesNumber, 8500 + networkNodesNumber);
            network.LocalPort = 8500 + networkNodesNumber;
            nodeList.Add(network);
            var row = table.NewRow();
            row["id"] = networkNodesNumber;
            row["Type"] = "Network";
            row["Name"] = "NN" + networkNodesNumber++;
            table.Rows.Add(row);
            mainWindow.addNode(network);
        }

        public void addConnection(Node from, int portFrom, Node to, int portTo)
        {
            if (from is ClientNode)
                if (connectionList.Where(i => i.From.Equals(from) || i.To.Equals(from)).Any())
                {
                    mainWindow.errorMessage("Client node can have only one connection!");
                    return;
                }

            if (to is ClientNode)
                if (connectionList.Where(i => i.From.Equals(to) || i.To.Equals(to)).Any())
                {
                    mainWindow.errorMessage("Client node can have only one connection!");
                    return;
                }
            if (from is NetNode)
                if (connectionList.Where(i => i.From.Equals(from) || i.To.Equals(from)).Count() == NETNODECONNECTIONS)
                {
                    mainWindow.errorMessage("Network node have " + NETNODECONNECTIONS + " ports");
                    return;
                }

            if (to is NetNode)
                if (connectionList.Where(i => i.From.Equals(to) || i.To.Equals(to)).Count() == NETNODECONNECTIONS)
                {
                    mainWindow.errorMessage("Network node have " + NETNODECONNECTIONS + " ports");
                    return;
                }
            if (to != null)
                if (connectionList.Where(i => (i.From.Equals(from) && i.To.Equals(to)) || (i.From.Equals(to) && i.To.Equals(from))).Any())
                {
                    mainWindow.errorMessage("That connection alredy exist!");
                }
                else
                {
                    //List<NodeConnection> portList = connectionList.Where(i => i.From.Equals(to) || i.To.Equals(to)).ToList();
                    if (connectionList.Where(i => i.From.Equals(to)).ToList().Where(i => i.VirtualPortFrom.Equals(portTo)).Any())
                        mainWindow.errorMessage("Port " + portTo + " in Node: " + to.Name + " is occupited.");
                    else if (connectionList.Where(i => i.To.Equals(to)).ToList().Where(i => i.VirtualPortTo.Equals(portTo)).Any())
                        mainWindow.errorMessage("Port " + portTo + " in Node: " + to.Name + " is occupited.");
                    else if (connectionList.Where(i => i.From.Equals(from)).ToList().Where(i => i.VirtualPortFrom.Equals(portFrom)).Any())
                        mainWindow.errorMessage("Port " + portFrom + " in Node: " + from.Name + " is occupited.");
                    else if (connectionList.Where(i => i.To.Equals(from)).ToList().Where(i => i.VirtualPortTo.Equals(portFrom)).Any())
                        mainWindow.errorMessage("Port " + portFrom + " in Node: " + from.Name + " is occupited.");
                    else
                    {
                        connectionList.Add(new NodeConnection(from, portFrom, to, portTo, from.Name + "-" + to.Name));
                        mainWindow.bind();
                    }
                }
        }

        public void isSpaceAvailable(Node node, int x, int y, int maxW, int maxH)
        {
            foreach (Node n in nodeList)
            {
                if (n.Position.Equals(new Point(x, y)))
                {
                    if (x + 10 < maxW - 1)
                        isSpaceAvailable(node, x + 10, y, maxW, maxH);
                    else
                        isSpaceAvailable(node, x - 10, y, maxW, maxH);
                    return;
                }
            }
            updateNode(node, x, y);
        }

        public void updateNode(Node node, int x, int y)
        {
            node.Position = new Point(x, y);
        }

        public void updateDomain(Domain domain)
        { }

        public void deleteNode(Node nodeToDelete)
        {
            table.Rows.Remove(table.Rows.Find(nodeToDelete.Name));
            mainWindow.errorMessage("Node " + nodeToDelete.Name + " deleted.");
            nodeList.Remove(nodeToDelete);
        }

        private int getNumberOfConnections(Node from, Node to)
        {
            return connectionList.Where(i => (
                        i.Start.Equals(from.Position) &&
                        i.Start.Equals(to.Position)) || (
                        i.Start.Equals(to.Position) &&
                        i.Start.Equals(from.Position))
                        ).Count();
        }

        private Node getNodeFrom(int x, int y)
        {
            Node n = nodeList.Where(i => i.Position.Equals(new Point(x, y))).FirstOrDefault();
            return n;
        }

        public List<String> findElemAtPosition(int x, int y)
        {
            List<String> atPosition = findConnectionsByPosition(x, y).Select(i => i.Name).ToList();
            Node n = getNodeFrom(x, y);
            if (n == null)
                return null; ;

            atPosition.Add(n.Name);
            return atPosition;
        }

        private List<NodeConnection> findConnectionsByPosition(int x, int y)
        {
            List<NodeConnection> result = new List<NodeConnection>();
            NodeConnection ifExist = connectionList.FirstOrDefault(
                i => (i.Start.Equals(new Point(x,y))) || (i.End.Equals(new Point(x,y))));
            if (ifExist != null)
                result = connectionList.AsParallel().Where(
                    i => (i.Start.Equals(new Point(x, y))) || (i.End.Equals(new Point(x, y)))
                    ).ToList();

            return result;
        }

        public void removeConnection(NodeConnection conn)
        {
            connectionList.RemoveAt(connectionList.IndexOf(conn));
        }

        public List<List<String>> findPaths(Node client, bool onlyClients)
        {
            List<Node> path = new List<Node>();
            List<Node> neighbors = new List<Node>();
            List<Node> listOfWhiteNodes = new List<Node>(nodeList);
            List<Node> listOfGrayNodes = new List<Node>();
            List<Node> listOfBlackNodes = new List<Node>();
            List<List<Node>> finder = new List<List<Node>>();
            List<List<String>> found = new List<List<String>>();

            if (client == null)
                return null;

            listOfWhiteNodes.Remove(client);
            listOfGrayNodes.Add(client);
            path.Add(client);
            finder.Add(path);
            while (listOfGrayNodes.Any())
            {
                List<Node> copyOflistOfGrayNodes = new List<Node>(listOfGrayNodes);
                foreach (Node nodeInCurrentStep in copyOflistOfGrayNodes)
                {
                    neighbors = findNeighborNodes(nodeInCurrentStep);
                    foreach(List<Node> pathiInFinder in finder)
                    {
                        if (pathiInFinder.Last().Equals(nodeInCurrentStep))
                            path = pathiInFinder;
                    }
                    
                    foreach (Node nodeProcessing in neighbors)
                    {
                        if (listOfWhiteNodes.Where(i => i.Equals(nodeProcessing)).Any())
                        {
                            List<Node> newPath = new List<Node>(path);
                            listOfGrayNodes.Add(nodeProcessing);
                            listOfWhiteNodes.Remove(nodeProcessing);
                            newPath.Add(nodeProcessing);
                            finder.Add(newPath);
                        }
                    }
                    listOfBlackNodes.Add(nodeInCurrentStep);
                    listOfGrayNodes.Remove(nodeInCurrentStep);
                }
            }


            if (onlyClients)
            {
                List<List<Node>> copyOfFinder = new List<List<Node>>(finder);
                foreach (List<Node> nodeListPath in copyOfFinder)
                {
                    if (!(nodeListPath.Last() is ClientNode))
                        finder.Remove(nodeListPath);
                    if (nodeListPath.Count() == 1)
                        finder.Remove(nodeListPath);
                }
            }

            foreach (List<Node> nodeListPath in finder)
            {
                List<String> nodeName = new List<string>();
                foreach (Node node in nodeListPath)
                {
                    nodeName.Add(node.Name);
                }
                found.Add(nodeName);
            }
            return found;
        }

        private List<Node> findNeighborNodes(Node n)
        {
            List<Node> neighborNodes = new List<Node>();
            List<NodeConnection> possibeNodesConn = connectionList.Where(i => i.From.Equals(n) || i.To.Equals(n)).ToList();
            foreach (NodeConnection con in possibeNodesConn)
            {
                neighborNodes.Add(con.From.Equals(n) ? con.To : con.From);
            }
            return neighborNodes;
        }

        public void stopRunning()
        {
            run = false;
        }

        public int getPort(Node node)
        {
            int port1, port2;
            if (connectionList.Where(i => i.From.Equals(node)).Select(c => c.VirtualPortFrom).Any())
                port1 = connectionList.Where(i => i.From.Equals(node)).Select(c => c.VirtualPortFrom).Max();
            else
                port1 = 0;
            if (connectionList.Where(i => i.To.Equals(node)).Select(c => c.VirtualPortTo).Any())
                port2 = connectionList.Where(i => i.To.Equals(node)).Select(c => c.VirtualPortTo).Max();
            else
                port2 = 0;
            return port1 > port2 ? ++port1 : ++port2;
        }

        public void sendOutInformation()
        {
            Dictionary<FIB, String> mailingList = new Dictionary<FIB, string>();
            Dictionary<FIB, String> possibleDestinations = new Dictionary<FIB, string>();
            int portIn, portOut;
            foreach (Node node in nodeList)
            {
                if(node is ClientNode)
                {
                    List<List<String>> possiblePaths = new List<List<String>>();
                    possiblePaths = findPaths(node, true);
                    possiblePaths.Reverse();
                    possiblePaths.Take(3);
                    int in_cout = 0;
                    foreach(List<String> nodeName in possiblePaths)
                    {
                        for(int i = 0; i < nodeName.Count(); i++)
                        {
                            if (i == 0)
                            {
                                //Start of path
                                FIB destination1 = new FIB(-1, -1, 0, in_cout);
                                possibleDestinations.Add(destination1, nodeName.ElementAt(i));
                                continue;
                            }
                            if (i == nodeName.Count() - 1)
                            {
                                //End of path
                                FIB destination2 = new FIB(0, in_cout, -1, -1);
                                possibleDestinations.Add(destination2, nodeName.ElementAt(i));
                                continue;
                            }

                            NodeConnection conIn = connectionList.Where(n =>
                            n.From.Name.Equals(nodeName.ElementAt(i - 1)) &&
                            n.To.Name.Equals(nodeName.ElementAt(i))
                            ).FirstOrDefault();
                            if(conIn == default(NodeConnection))
                            {
                                conIn = connectionList.Where(n =>
                                n.To.Name.Equals(nodeName.ElementAt(i - 1)) &&
                                n.From.Name.Equals(nodeName.ElementAt(i))
                                ).FirstOrDefault();
                                portIn = conIn.VirtualPortFrom;
                            }
                            else
                            {
                                portIn = conIn.VirtualPortTo;
                            }

                            NodeConnection conOut = connectionList.Where(n =>
                            n.From.Name.Equals(nodeName.ElementAt(i)) &&
                            n.To.Name.Equals(nodeName.ElementAt(i + 1))
                            ).FirstOrDefault();
                            if (conOut == default(NodeConnection))
                            {
                                conOut = connectionList.Where(n =>
                                n.To.Name.Equals(nodeName.ElementAt(i)) &&
                                n.From.Name.Equals(nodeName.ElementAt(i + 1))
                                ).FirstOrDefault();
                                portOut = conOut.VirtualPortTo;
                            }
                            else
                            {
                                portOut = conOut.VirtualPortFrom;
                            }

                                FIB newFib = new FIB(portIn, in_cout, portOut, in_cout);
                                mailingList.Add(newFib, nodeName.ElementAt(i));
                        }
                        in_cout++;
                    }
                }
            }
            mainWindow.errorMessage("Possible destinations:");
            foreach(Node node in nodeList)
            {
                BinaryWriter writer = new BinaryWriter(node.TcpClient.GetStream());
                ManagmentProtocol protocol = new ManagmentProtocol();
                protocol.State = ManagmentProtocol.ROUTINGTABLES;
                protocol.RoutingTable = mailingList.Where(n => n.Value.Equals(node.Name)).Select(k => k.Key).ToList();
               
                String send_object = JSON.Serialize(JSON.FromValue(protocol));
                writer.Write(send_object);
                //ManagmentProtocol received_Protocol = send_object.Value.ToObject<ManagmentProtocol>();
                //all node to setup
                //foreach(FIB f in mailingList.Where(n => n.Value.Equals(str)).Select(k => k.Key).ToList())
                //{
                //    mainWindow.errorMessage(str + ": " + f.toString());
                //}
                //mainWindow.errorMessage(str);
            }
            //foreach (String name in nodeList.Select(n => n.Name))
            //{
            //    List<FIB> tempList = malinigList.Where(n => n.Value.All(k => k.Equals(name))).Select(m => m.Key).ToList();
            //    foreach(FIB f in tempList)
            //    {
            //        String t = "Test";
            //        malinigList.TryGetValue(f,out t);
            //        mainWindow.errorMessage(t + ": " + f.toString());
            //    }
                
            //}
            mainWindow.errorMessage("Fibs:");
            foreach (System.Collections.Generic.KeyValuePair<FIB, string> oneFib in mailingList)
            {
                mainWindow.errorMessage(oneFib.Value + ": " + oneFib.Key.toString());
            }
        }
    }
}
