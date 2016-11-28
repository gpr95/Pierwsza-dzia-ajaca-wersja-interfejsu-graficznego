using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagementApp
{
    interface IControl
    {

    }
    class ControlPlane
    {
        MainWindow mainWindow;

        private DataTable table;

        private int clientNodesNumber;
        private int networkNodesNumber;

        //private List<ClientNode> clientNodeList = new List<ClientNode>();
        //private List<NetNode> networkNodeList = new List<NetNode>();
        private List<Node> nodeList = new List<Node>();
        private List<NodeConnection> connectionList = new List<NodeConnection>();
        private List<Domain> domainList = new List<Domain>();

        public ControlPlane()
        {
            mainWindow = new MainWindow(MakeTable(), nodeList, connectionList, domainList);
            mainWindow.Control = this;
            Application.Run(mainWindow);
            clientNodesNumber = 0;
            networkNodesNumber = 0;
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

        public void addClientNode(int x , int y)
        {
            foreach (Node node in nodeList)
                if (node.Position.Equals(new Point(x, y)))
                {
                    mainWindow.errorMessage("There is already node in that position.");
                    return;
                }
            ClientNode client = new ClientNode(x, y, "CN" + clientNodesNumber, 6000 + clientNodesNumber, 6050 + clientNodesNumber);
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
            foreach(Node node in nodeList)
                if(node.Position.Equals(new Point(x, y)))
                {
                    mainWindow.errorMessage("There is already node in that position.");
                    return;
                }
            NetNode network = new NetNode(x, y, "NN" + networkNodesNumber);
            nodeList.Add(network);
            var row = table.NewRow();
            row["id"] = networkNodesNumber;
            row["Type"] = "Network";
            row["Name"] = "NN" + networkNodesNumber++;
            table.Rows.Add(row);
            mainWindow.addNode(network);
        }

        public void addConnection(Node from, Node to)
        {
            if (to != null)
                if (connectionList.Where(i => (i.From.Equals(from) && i.To.Equals(to))||(i.From.Equals(to) && i.To.Equals(from))).Any())
                    {
                    mainWindow.errorMessage("That connection alredy exist!");
                }
                else
                {
                    connectionList.Add(new NodeConnection(from, to, from.Name + "-" + to.Name));
                    mainWindow.bind();
                }
                    
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
            List<Node> listOfAllNodes = new List<Node>();
            List<Node> listOfAllDestinations = new List<Node>();
            //List<Node> path = new List<Node>();
            List<List<Node>> finder = new List<List<Node>>();
            bool pathInProggress = true;
            List<List<String>> found = new List<List<String>>();
            int noc = 0;

            foreach (Node node in nodeList)
            {
                if (node is ClientNode)
                    listOfAllDestinations.Add(node);
                listOfAllNodes.Add(node);
            }
            listOfAllNodes.Remove(client);
            //path.Add(client);
            List<Node> tmp = new List<Node>();
            tmp.Add(client);
            finder.Add(tmp);
            int j = 0;
            while(pathInProggress && j < 100000)
            {
                List<List<Node>> finderCopy = new List<List<Node>>(finder);
                foreach (List<Node> nodeList in finderCopy)
                {
                    List<NodeConnection> possibeNodesConn = connectionList.Where(i => i.From.Equals(nodeList.Last())).ToList();
                    foreach (NodeConnection con in possibeNodesConn)
                    {
                        //if con.to is on the list
                        if (listOfAllNodes.Contains(con.To))
                        {
                            List<Node> temp = new List<Node>(nodeList);
                            temp.Add(con.To);
                            finder.Add(temp);
                            listOfAllNodes.Remove(con.To);
                        }
                        
                    }

                    //remove nodeList

                    possibeNodesConn.Clear();
                    possibeNodesConn = connectionList.Where(i => i.To.Equals(nodeList.Last())).ToList();
                    foreach (NodeConnection con in possibeNodesConn)
                    {
                        //if con.to is on the list
                        if (listOfAllNodes.Contains(con.From))
                        {
                            List<Node> temp = new List<Node>(nodeList);
                            temp.Add(con.From);
                            finder.Add(temp);
                            listOfAllNodes.Remove(con.From);
                        }

                    }
                    finder.Remove(nodeList);
                }
                foreach(List<Node> nodeList in finder)
                {
                    if (nodeList.Last() is ClientNode)
                        pathInProggress = false;
                    else
                        pathInProggress = true;
                }
            }

            foreach (List<Node> nodeList in finder)
            {
                List<String> temp = new List<string>();
                foreach (Node node in nodeList)
                {
                    temp.Add(node.Name);
                }
                found.Add(temp);
            }
            j++;
        return found;
        }
    }
}
