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
        private MainWindow mainWindow;
        private DataTable table;
        private readonly int MANAGMENTPORT = 7777;
        private readonly int NETNODECONNECTIONS = 4;
        private readonly int GAP = 10;
        private int clientNodesNumber = 0;
        private int networkNodesNumber = 0;
        private bool run = true;
        private TcpListener listener;
        private List<Node> nodeList = new List<Node>();
        private List<NodeConnection> connectionList = new List<NodeConnection>();
        private List<Domain> domainList = new List<Domain>();
        private List<Trail> trailList = new List<Trail>();
        private static ManagmentProtocol protocol = new ManagmentProtocol();


        private class ThreadPasser
        {
            public ControlPlane control;
            public TcpClient client;
        }

        public ControlPlane()
        {
            listener = new TcpListener(IPAddress.Any, MANAGMENTPORT);
            Thread thread = new Thread(new ParameterizedThreadStart(Listen));
            thread.Start(this);

            mainWindow = new MainWindow(MakeTable(), nodeList, connectionList, domainList);
            mainWindow.Control = this;
            Application.Run(mainWindow);
        }
        public void load()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Save topology";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                nodeList = null;
                connectionList = null;
                domainList = null;
                String path = openFileDialog.InitialDirectory;
                String fileName = openFileDialog.FileName;
                FileSaver configuration = new FileSaver(path + fileName);
                nodeList = configuration.ReadFromBinaryFileNodes();
                connectionList = configuration.ReadFromBinaryFileNodeConnections();
                domainList = configuration.ReadFromBinaryFileDomains();
                mainWindow.updateLists(nodeList, connectionList, domainList);
            }
        }
        private void Listen(Object controlP)
        {
            listener.Start();
            ThreadPasser tp = new ThreadPasser();
            tp.control = (ControlPlane)controlP;

            while (run)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    tp.client = client;
                    Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                    clientThread.Start(tp);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private static void ListenThread(Object threadPasser)
        {
            ThreadPasser tp = (ThreadPasser) threadPasser;
            TcpClient clienttmp = tp.client;
            BinaryWriter writer = new BinaryWriter(clienttmp.GetStream());

            ManagmentProtocol toSend = new ManagmentProtocol();
            toSend.State = ManagmentProtocol.WHOIS;
            string data = JSON.Serialize(JSON.FromValue(toSend));
            writer.Write(data);

            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            string received_data = reader.ReadString();
            JSON received_object = JSON.Deserialize(received_data);
            ManagmentProtocol received_Protocol = received_object.Value.ToObject<ManagmentProtocol>();
            String nodeName = received_Protocol.Name;
            tp.control.allocateNode(nodeName, clienttmp, Thread.CurrentThread, writer);
        }

        public void allocateNode(String nodeName, TcpClient nodePort, Thread nodeThreadHandle, BinaryWriter writer)
        {
            Node nodeBeingAllocated = nodeList.Where(i => i.Name.Equals(nodeName)).FirstOrDefault();
            nodeBeingAllocated.ThreadHandle = nodeThreadHandle;
            nodeBeingAllocated.TcpClient = nodePort;
            nodeBeingAllocated.SocketWriter = writer;
        }

        private DataTable MakeTable()
        {
            //Fix needed
            table = new DataTable("threadManagment");
            var column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "id";
            column.AutoIncrement = false;
            column.Caption = "ParentItem";
            column.ReadOnly = true;
            column.Unique = false;
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
            ClientNode client = new ClientNode(x, y, "CN." + clientNodesNumber, 8000 + clientNodesNumber);
            ++clientNodesNumber;
            nodeList.Add(client);
            addNodeToTable(client);
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
            NetNode network = new NetNode(x, y, "NN." + networkNodesNumber, 8500 + networkNodesNumber);
            ++networkNodesNumber;
            nodeList.Add(network);
            addNodeToTable(network);
            mainWindow.addNode(network);
        }

        private void addNodeToTable(Node n)
        {
            var row = table.NewRow();
            int nodeNumber;
            int.TryParse(n.Name.Split('.')[1], out nodeNumber);
            row["id"] = nodeNumber;
            row["Type"] = n is NetNode ? "Network" : "Client";
            row["Name"] = n is NetNode ? "NN" + nodeNumber : "CN" + nodeNumber;
            table.Rows.Add(row);
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
                if (numberOfNodeConnections(from) == NETNODECONNECTIONS)
                {
                    mainWindow.errorMessage("Network node have " + NETNODECONNECTIONS + " ports");
                    return;
                }

            if (to is NetNode)
                if (numberOfNodeConnections(to) == NETNODECONNECTIONS)
                {
                    mainWindow.errorMessage("Network node have " + NETNODECONNECTIONS + " ports");
                    return;
                }
            if (to != null)
                if (isConnectionExist(from, to))
                {
                    mainWindow.errorMessage("That connection alredy exist!");
                }
                else
                {
                    //List<NodeConnection> portList = connectionList.Where(i => i.From.Equals(to) || i.To.Equals(to)).ToList();
                    if (connectionList.Where(i => i.From.Equals(to)).ToList().Where(i => i.VirtualPortFrom.Equals(portTo)).Any())
                        mainWindow.errorMessage("Port " + portTo + " in Node: " + to.Name + " is occupited.1");
                        //to fix
                    //else if (connectionList.Where(i => i.To.Equals(to)).ToList().Where(i => i.VirtualPortTo.Equals(portTo)).Any())
                        //connectionList.Where(i => i.To.Equals(to)).ToList().Where(i => i.VirtualPortTo.Equals(portTo)).Any();
                        //mainWindow.errorMessage("Port " + portTo + " in Node: " + to.Name + " is occupited.2");
                    else if (connectionList.Where(i => i.From.Equals(from)).ToList().Where(i => i.VirtualPortFrom.Equals(portFrom)).Any())
                        mainWindow.errorMessage("Port " + portFrom + " in Node: " + from.Name + " is occupited.3");
                    else if (connectionList.Where(i => i.To.Equals(from)).ToList().Where(i => i.VirtualPortTo.Equals(portFrom)).Any())
                        mainWindow.errorMessage("Port " + portFrom + " in Node: " + from.Name + " is occupited.4");
                    else
                    {
                        connectionList.Add(new NodeConnection(from, portFrom, to, portTo, from.Name + "-" + to.Name));
                        mainWindow.bind();
                    }
                }
        }

        private int numberOfNodeConnections(Node n)
        {
            return connectionList.Where(i => i.From.Equals(n) || i.To.Equals(n)).Count();
        }

        private bool isConnectionExist(Node f, Node t)
        {
            return connectionList.Where(i => (i.From.Equals(f) && i.To.Equals(t)) || (i.From.Equals(t) && i.To.Equals(f))).Any();
        }

        public void isSpaceAvailable(Node node, int x, int y, int maxW, int maxH)
        {
            foreach (Node n in nodeList)
            {
                if (n.Position.Equals(new Point(x, y)))
                {
                    if (x + GAP < maxW - 1)
                        isSpaceAvailable(node, x + GAP, y, maxW, maxH);
                    else
                        isSpaceAvailable(node, x - GAP, y, maxW, maxH);
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
        {
            //To be implrmented later.
        }

        public void deleteNode(Node nodeToDelete)
        {
            table.Rows.Remove(table.Rows.Find(nodeToDelete.Name));
            mainWindow.errorMessage("Node " + nodeToDelete.Name + " deleted.");
            nodeList.Remove(nodeToDelete);
        }

        private int getNumberOfConnectionsBetweenNodes(Node from, Node to)
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

        public List<List<Node>> findPathsLN(Node client, bool onlyClients)
        {
            List<Node> path = new List<Node>();
            List<Node> neighbors = new List<Node>();
            List<Node> listOfWhiteNodes = new List<Node>(nodeList);
            List<Node> listOfGrayNodes = new List<Node>();
            List<Node> listOfBlackNodes = new List<Node>();
            List<List<Node>> finder = new List<List<Node>>();

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
                    foreach (List<Node> pathiInFinder in finder)
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

            return finder;
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
                listener.Stop();
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

        public Trail createTrail(Node from, Node to)
        {
            List<List<Node>> paths = findPathsLN(from, true);
            List<Node> path;
            foreach(List<Node> tempPath in paths)
            {
                if(tempPath.Last().Equals(to))
                {
                    path = tempPath;
                    return new Trail(path, connectionList, true);
                }
            }
            return null;
        }
        public void sendOutInformation()
        {
            List<Trail> copyTrailList = new List<Trail>(trailList);
            foreach (Trail trail in copyTrailList)
            {
                if (!trail.isCreadetByUser())
                {
                    trail.clearTrail(trail);
                    trailList.Remove(trail);
                }
            }
            Dictionary<FIB, String> mailingList = new Dictionary<FIB, string>();
            Dictionary<Dictionary<String, int>, String> possibleDestinations = new Dictionary<Dictionary<string, int>, String>();
            //int portIn, portOut;
            foreach (Node node in nodeList)
            {
                if(node is ClientNode)
                {
                    List<List<Node>> possiblePaths = new List<List<Node>>();
                    possiblePaths = findPathsLN(node, true);
                    possiblePaths.Reverse();
                    //possiblePaths = possiblePaths.Take(4).ToList();
                    foreach(List<Node> n in possiblePaths)
                    {
                        trailList.Add(new Trail(n, connectionList, false));
                    }
                }
            }

            foreach(Trail trail in trailList)
            {
                if (trail.From == null)
                    continue;
                BinaryWriter writer = new BinaryWriter(trail.From.TcpClient.GetStream());
                ManagmentProtocol protocol = new ManagmentProtocol();
                protocol.State = ManagmentProtocol.POSSIBLEDESITATIONS;
                protocol.possibleDestinations = new Dictionary<string, int>();
              
                protocol.possibleDestinations.Add(trail.To.Name, trail.StartingSlot);
                protocol.Port = trail.PortFrom;

                String send_object = JSON.Serialize(JSON.FromValue(protocol));
                writer.Write(send_object);

                foreach(KeyValuePair<Node, FIB> fib in trail.ComponentFIBs)
                {
                    continue;
                    writer = fib.Key.SocketWriter;//new BinaryWriter(fib.Key.TcpClient.GetStream());
                    protocol = new ManagmentProtocol();
                    protocol.State = ManagmentProtocol.ROUTINGENTRY;
                    Console.WriteLine("routingtable");
                    protocol.RoutingEntry = fib.Value;

                    send_object = JSON.Serialize(JSON.FromValue(protocol));
                    writer.Write(send_object);
                }
            }

            copyTrailList = new List<Trail>(trailList);

            foreach (Trail t in copyTrailList)
            {
                if (t.From == null || t.To == null)
                    trailList.Remove(t);
            }

            foreach(Trail t in trailList)
            {
                mainWindow.errorMessage(t.toString());
            }

            //foreach(Node node in nodeList)
            //{
            //    if (node is ClientNode)
            //    {
            //        BinaryWriter writer = new BinaryWriter(node.TcpClient.GetStream());
            //        ManagmentProtocol protocol = new ManagmentProtocol();
            //        protocol.State = ManagmentProtocol.POSSIBLEDESITATIONS;
            //        protocol.possibleDestinations = new Dictionary<string, int>();

            //        foreach (Trail trail in trailList.Where(t => t.From.Equals(node)).ToList())
            //        {
            //            if (trail.From.Equals(node))
            //            {
            //                //foreach(KeyValuePair<string, int> d in dic.Key)
            //                protocol.possibleDestinations.Add(trail.To.Name, trail.StartingSlot);
            //                protocol.Port = trail.PortFrom;
            //            }
            //        }
            //        //if (connectionList.Where(n => n.From.Equals(node)).Any())
            //        //    protocol.Port = connectionList.Where(n => n.From.Equals(node)).FirstOrDefault().VirtualPortFrom;
            //        //else if (connectionList.Where(n => n.To.Equals(node)).Any())
            //        //    protocol.Port = connectionList.Where(n => n.To.Equals(node)).FirstOrDefault().VirtualPortTo;

            //        String send_object = JSON.Serialize(JSON.FromValue(protocol));
            //        writer.Write(send_object);
            //    }
            //    else
            //    {
            //        BinaryWriter writer = new BinaryWriter(node.TcpClient.GetStream());
            //        ManagmentProtocol protocol = new ManagmentProtocol();
            //        protocol.State = ManagmentProtocol.ROUTINGTABLES;
            //        Console.WriteLine("routingtable");
            //        protocol.RoutingTable = mailingList.Where(n => n.Value.Equals(node.Name)).Select(k => k.Key).ToList();

            //        String send_object = JSON.Serialize(JSON.FromValue(protocol));
            //        writer.Write(send_object);
            //    }
            //}   
            //mainWindow.errorMessage("Fibs:");
            //foreach (System.Collections.Generic.KeyValuePair<FIB, string> oneFib in mailingList)
            //{
            //    mainWindow.errorMessage(oneFib.Value + ": " + oneFib.Key.toString());
            //}
        }
        public void showTrailWindow()
        {
            CreatingTrailWindow trailWindow = new CreatingTrailWindow(nodeList, connectionList, this);
            trailWindow.TopMost = true;
            trailWindow.ShowDialog();
        }
        public void addTrail(Trail trail)
        {
            trailList.Add(trail);
        }
    }
}
