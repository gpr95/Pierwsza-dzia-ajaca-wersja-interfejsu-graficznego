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
        private int clientNodesNumber;
        private int networkNodesNumber;
        private bool run = true;
        private TcpListener listener;
        private static ManagmentProtocol protocol = new ManagmentProtocol();
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
            writer.Write(protocol.WHOIS);
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
                    mainWindow.errorMessage("Client node can have onli one connection!");
                    return;
                }

            if (to is ClientNode)
                if (connectionList.Where(i => i.From.Equals(to) || i.To.Equals(to)).Any())
                {
                    mainWindow.errorMessage("Client node can have onli one connection!");
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

        public void updateElement(ContainerElement elem)
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

        public List<List<String>> findPaths(Node client)
        {
            bool pathInProggress = true;
            //List<Node> listOfAllDestinations = new List<Node>();
            List<Node> listOfAllNodes = new List<Node>();
            List<List<Node>> finder = new List<List<Node>>();
            List<List<String>> found = new List<List<String>>();

            if (client == null)
                return null;

            foreach (Node node in nodeList)
            {
                //if (node is ClientNode)
                //    listOfAllDestinations.Add(node);
                listOfAllNodes.Add(node);
            }
            listOfAllNodes.Remove(client);

            List<Node> tmp = new List<Node>();
            tmp.Add(client);
            finder.Add(tmp);
            while (pathInProggress)
            {
                List<List<Node>> finderCopy = new List<List<Node>>(finder);
                foreach (List<Node> nodeListPath in finderCopy)
                {
                    Node last = nodeListPath.Last();
                    List<NodeConnection> possibeNodesConn = connectionList.Where(i => i.From.Equals(last) || i.To.Equals(last)).ToList();
                    foreach (NodeConnection con in possibeNodesConn)
                    {
                        Node target = con.From.Equals(last) ? con.To : con.From;
                        if (listOfAllNodes.Contains(target))
                        {
                            List<Node> temp = new List<Node>(nodeListPath);
                            temp.Add(target);
                            finder.Add(temp);
                            listOfAllNodes.Remove(target);
                            pathInProggress = true;
                        }
                        else
                            pathInProggress = false;

                    }

                    finder.Remove(nodeList);
                }
                //foreach (List<Node> nodeListPath in finder)
                //{
                //    if (nodeListPath.Last() is ClientNode)
                //        pathInProggress = false;
                //    else
                //        pathInProggress = true;
                //}

            }

            List<List<Node>> finderCopyTwo = new List<List<Node>>(finder);
            foreach (List<Node> nodeListPath in finderCopyTwo)
            {
                if (!(nodeListPath.Last() is ClientNode))
                    finder.Remove(nodeListPath);
                if (nodeListPath.Count() == 1)
                    finder.Remove(nodeListPath);
            }
            foreach (List<Node> nodeListPath in finder)
            {

                List<String> temp = new List<string>();
                foreach (Node node in nodeListPath)
                {
                    temp.Add(node.Name);
                }
                found.Add(temp);
            }
            return found;
        }

        public void stopRunning()
        {
            run = false;
        }
    }
}
