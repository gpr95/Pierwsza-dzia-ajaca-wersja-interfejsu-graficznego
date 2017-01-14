using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ManagementApp;

namespace Management
{
    class ManagementPlane
    {
        //private MainWindow mainWindow;
        private DataTable table;
        private readonly int MANAGMENTPORT = 7777;

        private readonly int GAP = 10;
        private int clientNodesNumber = 0;
        private int networkNodesNumber = 0;
        private bool run = true;
        private TcpListener listener;
        private List<Node> nodeList = new List<Node>();
        private List<NodeConnection> connectionList = new List<NodeConnection>();
        //private List<Domain> domainList = new List<Domain>();
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



        //public void addClientNode(int x, int y)
        //{
        //    foreach (Node node in nodeList)
        //        if (node.Position.Equals(new Point(x, y)))
        //        {
        //            mainWindow.errorMessage("There is already node in that position.");
        //            return;
        //        }
        //    ClientNode client = new ClientNode(x, y, "CN." + clientNodesNumber, 8000 + clientNodesNumber);
        //    ++clientNodesNumber;
        //    nodeList.Add(client);
        //    addNodeToTable(client);
        //    mainWindow.addNode(client);
        //}





        //private bool addTrailToTable(Trail t)
        //{
        //    var row = table.NewRow();
        //    //int nodeNumber;
        //    //int.TryParse(n.Name.Split('.')[1], out nodeNumber);
        //    row["id"] = t.StartingSlot;
        //    row["Type"] = "Trail";
        //    row["Name"] = t.Name;
        //    try
        //    {
        //        table.Rows.Add(row);
        //    }
        //    catch(ConstraintException e)
        //    {
        //        mainWindow.errorMessage("This trail alredy exist.");
        //        return false;
        //    }
        //    return true;

        //}







        public void updateDomain(Domain domain)
        {
            //To be implemented later.
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
            List<NodeConnection> possibeNodesConn = connectionList.Where(i => i.From.Equals(n.Name) || i.To.Equals(n.Name)).ToList();
            foreach (NodeConnection con in possibeNodesConn)
            {
                neighborNodes.Add(con.From.Equals(n) ? nodeList.Where(c => c.Name.Equals(con.To)).FirstOrDefault() : nodeList.Where(c => c.Name.Equals(con.From)).FirstOrDefault());
            }
            return neighborNodes;
        }

        public void stopRunning()
        {
            run = false;
            listener.Stop();
        }

        public Trail createTrail(Node from, Node to, bool vc4 = false)
        {
            if (from == null || to == null || from == default(Node) || to == default(Node))
                return null;
            List<List<Node>> paths = findPathsLN(from, true);
            List<Node> path;
            foreach (List<Node> tempPath in paths)
            {
                if (tempPath.Last().Equals(to))
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

            foreach (Node n in nodeList.Where(n => n is ClientNode).ToList())
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
                // mainWindow.errorMessage("Sended trail info to : " +trail.From.Name + "," + protocol.Port);
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
                //mainWindow.errorMessage("Sended trail info to : " + trail.From.Name + "," + protocol.Port);
                send_object = JSON.Serialize(JSON.FromValue(protocol));
                writer.Write(send_object);
                Thread.Sleep(1000);
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
                if (node is ClientNode)
                {
                    List<List<Node>> possiblePaths = new List<List<Node>>();
                    possiblePaths = findPathsLN(node, true);
                    possiblePaths.Reverse();
                    //possiblePaths = possiblePaths.Take(4).ToList();
                    foreach (List<Node> n in possiblePaths)
                    {
                        Trail t = new Trail(n, connectionList, false);
                        trailList.Add(t);
                        //addTrail(t);
                    }
                }
            }

            copyTrailList = new List<Trail>(trailList);

            foreach (Trail t in copyTrailList)
            {
                if (t.From == null || t.To == null)
                    trailList.Remove(t);
            }

            foreach (Trail t in trailList)
            {
                //mainWindow.errorMessage(t.toString());
            }
        }
        //public void showTrailWindow()
        //{
        //    CreatingTrailWindow trailWindow = new CreatingTrailWindow(nodeList, connectionList, this);
        //    trailWindow.TopMost = true;
        //    trailWindow.ShowDialog();
        //}
        //public void addTrail(Trail trail)
        //{
        //    if (trail != null)
        //    {
        //        if (addTrailToTable(trail))
        //        {
        //            mainWindow.errorMessage("The Trail has been added!");
        //            trailList.Add(trail);
        //            mainWindow.errorMessage(trail.toString());
        //            sendOutInformation();
        //        }
        //        else
        //        {
        //            trail.clearTrail(trail);
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        mainWindow.errorMessage("Error during trail creatbion.");
        //    }
        //}

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
            foreach (Trail t in trailList)
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
            foreach (Trail t in trailList)
            {
                if (t.From.Equals(n) || t.To.Equals(n))
                    tList.Add(t);
            }
            return tList;
        }
    }
}
