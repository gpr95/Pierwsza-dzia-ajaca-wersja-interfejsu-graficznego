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
    class ManagementPlane
    {
        private MainWindow mainWindow;
        private DataTable table;
        private readonly int MANAGMENTPORT = 7777;
        private readonly int NETNODECONNECTIONS = 21;
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
            public ManagementPlane management;
            public TcpClient client;
        }

        public ManagementPlane()
        {
            listener = new TcpListener(IPAddress.Any, MANAGMENTPORT);
            Thread thread = new Thread(new ParameterizedThreadStart(Listen));
            thread.Start(this);

            mainWindow = new MainWindow(MakeTable(), nodeList, connectionList, domainList);
            mainWindow.Management = this;
            Application.Run(mainWindow);
        }
        public void load()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Save topology";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                String path = openFileDialog.InitialDirectory;
                String fileName = openFileDialog.FileName;
                FileSaver configuration = new FileSaver(path + fileName);


                foreach(Node n in configuration.ReadFromBinaryFileNodes())
                {
                    if (n is ClientNode)
                        nodeList.Add(new ClientNode((ClientNode)n));

                    if (n is NetNode)
                        nodeList.Add(new NetNode((NetNode)n));
                    Thread.Sleep(100);
                }
               
                List<NodeConnection> tmpNodeConnList = new List<NodeConnection>();
                foreach(NodeConnection nc in configuration.ReadFromBinaryFileNodeConnections())
                {
                    
                    foreach (Node realNode in nodeList)
                    {
                        if (realNode.LocalPort == nc.From.LocalPort)
                           nc.From = realNode;
                        if (realNode.LocalPort == nc.To.LocalPort)
                            nc.To = realNode;
                    }
                    mainWindow.bind(nc);
                    tmpNodeConnList.Add(new NodeConnection(nc));
                    Thread.Sleep(100);
                }

                List<Domain> tmpDomainList = new List<Domain>();
                configuration.ReadFromBinaryFileDomains().ForEach(
                  d => {
                      tmpDomainList.Add(new Domain(d)); Thread.Sleep(500);
                    });

                connectionList = new List<NodeConnection>();
                domainList = new List<Domain>();

                connectionList.AddRange(tmpNodeConnList);
                domainList.AddRange(tmpDomainList);

                mainWindow.updateLists(nodeList, domainList);
            }
        }
        private void Listen(Object managementlP)
        {
            listener.Start();
            ThreadPasser tp = new ThreadPasser();
            tp.management = (ManagementPlane)managementlP;

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
            tp.management.allocateNode(nodeName, clienttmp, Thread.CurrentThread, writer);
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
            row["Name"] = n is NetNode ? "NN." + nodeNumber : "CN." + nodeNumber;
            table.Rows.Add(row);
        }

        private bool addTrailToTable(Trail t)
        {
            var row = table.NewRow();
            //int nodeNumber;
            //int.TryParse(n.Name.Split('.')[1], out nodeNumber);
            row["id"] = t.StartingSlot;
            row["Type"] = "Trail";
            row["Name"] = t.Name;
            try
            {
                table.Rows.Add(row);
            }
            catch(ConstraintException e)
            {
                mainWindow.errorMessage("This trail alredy exist.");
                return false;
            }
            return true;
            
        }

        public void addConnection(Node from, int portFrom, Node to, int portTo, bool move = false)
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
                    if(move)
                    {
                        connectionList.Add(new NodeConnection(from, portFrom, to, portTo, from.Name + "-" + to.Name));
                        mainWindow.bind();
                    }
                    else if (connectionList.Where(i => i.From.Equals(to)).ToList().Where(i => i.VirtualPortFrom.Equals(portTo)).Any())
                        mainWindow.errorMessage("Port " + portTo + " in Node: " + to.Name + " is occupited.1");
                    else if (connectionList.Where(i => i.To.Equals(to)).ToList().Where(i => i.VirtualPortTo.Equals(portTo)).Any())
                        //connectionList.Where(i => i.To.Equals(to)).ToList().Where(i => i.VirtualPortTo.Equals(portTo)).Any();
                        mainWindow.errorMessage("Port " + portTo + " in Node: " + to.Name + " is occupited.2");
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
            //To be implemented later.
        }

        public void deleteNode(Node nodeToDelete)
        {
            table.Rows.Remove(table.Rows.Find(nodeToDelete.Name));
            mainWindow.errorMessage("Node " + nodeToDelete.Name + " deleted.");
            nodeList.Remove(nodeToDelete);
            nodeToDelete.ProcessHandle.Dispose();
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

        internal void restartNode(string v)
        {
            Node n = nodeList.Where(s => s.Name.Equals(v)).FirstOrDefault();
            if(n.ProcessHandle.HasExited)
            {
                //nodeList.Remove(n);
                if (n is ClientNode)
                    n = new ClientNode((ClientNode)n);
                else if (n is NetNode)
                    n = new NetNode((NetNode) n);

                //List<string> conL = findElemAtPosition(n.Position.X, n.Position.Y);
                mainWindow.updateConnections(connectionList);
                sendOutInformation();
            }
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
                        if (listOfWhiteNodes.Where(i => i.Name.Equals(nodeProcessing.Name)).Any())
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

        public Trail createTrail(Node from, Node to, bool vc4 = false)
        {
            if (from == null || to == null)
                return null;
            List<List<Node>> paths = findPathsLN(from, true);
            List<Node> path;
            foreach(List<Node> tempPath in paths)
            {
                if(tempPath.Last().Equals(to))
                {
                    path = tempPath;
                    return new Trail(path, connectionList, true, vc4);
                }
            }
            return null;
        }

        public void sendOutInformation(bool clearAutoTrails = true)
        {
            if (clearAutoTrails)
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
            }
            
            Dictionary<Dictionary<String, int>, String> possibleDestinations = new Dictionary<Dictionary<string, int>, String>();
            Dictionary<Dictionary<string, int>, string> listDestinations = new Dictionary<Dictionary<string, int>, string>();
            foreach (Trail trail in trailList)
            {
                if (trail.From != null && trail.To != null)
                {
                    Dictionary<string, int> temp = new Dictionary<string, int>();
                    temp.Add(trail.To.Name, trail.StartingSlot);
                    listDestinations.Add(temp, (trail.From.Name));
                    temp = new Dictionary<string, int>();
                    temp.Add(trail.From.Name, trail.EndingSlot);
                    listDestinations.Add(temp, (trail.To.Name));
                }
            }

            foreach (Node n in nodeList)
            {
                ManagmentProtocol protocol = new ManagmentProtocol();
                protocol.State = ManagmentProtocol.POSSIBLEDESITATIONS;
                protocol.possibleDestinations = new Dictionary<string, int>();
                String send_object = JSON.Serialize(JSON.FromValue(protocol));
                BinaryWriter writer = n.SocketWriter;
                writer.Write(send_object);
            }

            foreach (Trail trail in trailList)
            {
                if (trail.From == null)
                    continue;
                //Fix needed
                BinaryWriter writer = trail.From.SocketWriter;//new BinaryWriter(trail.From.TcpClient.GetStream());
                ManagmentProtocol protocol = new ManagmentProtocol();
                protocol.State = ManagmentProtocol.POSSIBLEDESITATIONS;
                protocol.possibleDestinations = new Dictionary<string, int>();

                foreach (var dest in listDestinations)
                {
                    if (dest.Value == trail.From.Name)
                    {
                        foreach (var temp in dest.Key)
                        {
                            protocol.possibleDestinations.Add(temp.Key, temp.Value);
                        }
                    }
                }

                //protocol.possibleDestinations.Add(trail.To.Name, trail.StartingSlot);
                protocol.Port = trail.PortFrom;
                mainWindow.errorMessage("Sended trail info to : " +trail.From.Name + "," + protocol.Port);
                String send_object = JSON.Serialize(JSON.FromValue(protocol));
                writer.Write(send_object);

                writer = trail.To.SocketWriter;
                protocol = new ManagmentProtocol();
                protocol.State = ManagmentProtocol.POSSIBLEDESITATIONS;
                protocol.possibleDestinations = new Dictionary<string, int>();

                foreach (var dest in listDestinations)
                {
                    if (dest.Value == trail.To.Name)
                    {
                        foreach (var temp in dest.Key)
                        {
                            protocol.possibleDestinations.Add(temp.Key, temp.Value);
                        }
                    }
                }

                protocol.Port = trail.PortFrom;
                mainWindow.errorMessage("Sended trail info to : " + trail.From.Name + "," + protocol.Port);
                send_object = JSON.Serialize(JSON.FromValue(protocol));
                writer.Write(send_object);

                foreach (KeyValuePair<Node, FIB> fib in trail.ComponentFIBs)
                {
                    //continue;
                    writer = fib.Key.SocketWriter; //new BinaryWriter(fib.Key.TcpClient.GetStream());//
                    protocol = new ManagmentProtocol();
                    protocol.State = ManagmentProtocol.ROUTINGENTRY;
                    Console.WriteLine("routingentry");
                    protocol.RoutingEntry = fib.Value;

                    send_object = JSON.Serialize(JSON.FromValue(protocol));
                    writer.Write(send_object);
                    protocol.RoutingEntry = fib.Value.reverse();
                    send_object = JSON.Serialize(JSON.FromValue(protocol));
                    writer.Write(send_object);
                }
            }
        }

        public void createAutoTrails()
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
                        Trail t = new Trail(n, connectionList, false);
                        trailList.Add(t);
                        addTrail(t);
                    }
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
        }
        public void showTrailWindow()
        {
            CreatingTrailWindow trailWindow = new CreatingTrailWindow(nodeList, connectionList, this);
            trailWindow.TopMost = true;
            trailWindow.ShowDialog();
        }
        public void addTrail(Trail trail)
        {
            if(trail != null)
            {
                if (addTrailToTable(trail))
                {
                    mainWindow.errorMessage("The Trail has been added!");
                    trailList.Add(trail);
                    mainWindow.errorMessage(trail.toString());
                    sendOutInformation();
                }
                else
                {
                    trail.clearTrail(trail);
                    return;
                }
            }
            else
            {
                mainWindow.errorMessage("Error during trail creatbion.");
            }
        }

        internal List<Trail> TrailList
        {
            get
            {
                return trailList;
            }

            set
            {
                trailList = value;
            }
        }

        internal void clearAllTrails()
        {
            foreach(Trail t in trailList)
            {
                t.clearTrail(t);
                table.Rows.Remove(table.Rows.Find(t.Name));
            }
            trailList = new List<Trail>();
            sendOutInformation();
        }

        public List<Trail> getTrailForNode(Node n)
        {
            List<Trail> tList = new List<Trail>();
            foreach(Trail t in trailList)
            {
                if (t.From.Equals(n) || t.To.Equals(n))
                    tList.Add(t);
            }
            return tList;
        }
    }
}
