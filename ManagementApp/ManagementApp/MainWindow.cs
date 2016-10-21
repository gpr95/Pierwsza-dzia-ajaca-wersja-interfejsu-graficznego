using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagementApp
{
    public partial class MainWindow : Form
    {
        // CONSTS
        private const int GAP = 10;

        // LOGICAL VARS
        private NodeType nType;
        private List<ContainerElement> elements = new List<ContainerElement>();
        private List<ContainerElement> elementsTemp = new List<ContainerElement>();
        private int clientNodesNumber;
        private int networkNodesNumber;
        private DataTable table;

        // PAINTING VARS
        private Bitmap containerPoints;
        private ContainerElement nodeFrom;
        private ContainerElement virtualNodeTo;
        private bool isDrawing = false;
        private Point domainFrom;
        private Graphics myGraphics;

        public MainWindow()
        {
            InitializeComponent();
            MakeTable();
            clientNodesNumber = 0;
            networkNodesNumber = 0;
        }

        // MAIN WINDOW ACTIONS
        private void MainWindow_Load(object sender, EventArgs e)
        {
            containerPoints = new Bitmap(containerPictureBox.ClientSize.Width, containerPictureBox.ClientSize.Height);
            for (int x = 0; x < containerPictureBox.ClientSize.Width;
                x += GAP)
            {
                for (int y = 0; y < containerPictureBox.ClientSize.Height;
                    y += GAP)
                {
                    containerPoints.SetPixel(x, y, Color.Black);
                }
            }
            myGraphics = containerPictureBox.CreateGraphics();
        }
        private void MakeTable()
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

            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // CONTAINER ACTIONS
        private void containerPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics panel = e.Graphics;

            Rectangle rect;
            foreach (var elem in elements.AsParallel().Where(i => i is NetNode))
            {
                rect = new Rectangle(elem.ContainedPoints.ElementAt(0).X - 5,
                    elem.ContainedPoints.ElementAt(0).Y - 5, 11, 11);
                panel.FillEllipse(Brushes.CadetBlue, rect);
                panel.DrawEllipse(Pens.Black, rect);
                panel.DrawString(elem.Name, new Font("Arial", 5), Brushes.Black, new Point(elem.ContainedPoints.ElementAt(0).X + 3,
                    elem.ContainedPoints.ElementAt(0).Y + 3));
            }
            foreach (var elem in elements.AsParallel().Where(i => i is ClientNode))
            {
                rect = new Rectangle(elem.ContainedPoints.ElementAt(0).X - 5,
                   elem.ContainedPoints.ElementAt(0).Y - 5, 11, 11);
                panel.FillEllipse(Brushes.ForestGreen, rect);
                panel.DrawEllipse(Pens.Black, rect);
                panel.DrawString(elem.Name, new Font("Arial", 5), Brushes.Black, new Point(elem.ContainedPoints.ElementAt(0).X + 3,
                    elem.ContainedPoints.ElementAt(0).Y + 3));
            }
            foreach (var elem in elements.AsParallel().Where(i => i is NodeConnection))
            {
                Pen blackPen = new Pen(Color.Black, 2);
                Point from = elem.ContainedPoints.ElementAt(0);
                Point to = elem.ContainedPoints.ElementAt(1);
                panel.DrawLine(blackPen, from, to);
                panel.DrawString(elem.Name, new Font("Arial", 5), Brushes.Black, new Point((from.X + to.X) / 2 + 3,
                   (from.Y + to.Y) / 2 + 3));
            }
            foreach (var elem in elements.AsParallel().Where(i => i is Domain))
            {
                Domain tmp = (Domain)elem;
                Point from = tmp.PointFrom;
                rect = new Rectangle(from.X, from.Y, tmp.Width, tmp.Height);
                panel.DrawRectangle(new Pen(Color.PaleVioletRed, 3), rect);
            }
            containerPictureBox.BackgroundImage = containerPoints;
        }
        private void containerPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            putToGrid(ref x, ref y);
            var row = table.NewRow();
            var bSource = new BindingSource();
            switch (nType)
            {
                case NodeType.CLIENT_NODE:
                    var clientNodeCurrentNumber = clientNodesNumber;
                    elements.Add(new ClientNode(x, y, "CN" + clientNodesNumber++));
                    consoleTextBox.AppendText("Client Node added at: " + x + "," + y);
                    consoleTextBox.AppendText(Environment.NewLine);
                    row["id"] = clientNodeCurrentNumber;
                    row["Type"] = "Client";
                    row["Name"] = "CN" + clientNodeCurrentNumber;
                    table.Rows.Add(row);
                    bSource.DataSource = table;
                    dataGridView1.DataSource = bSource;
                    dataGridView1.Update();
                    dataGridView1.Refresh();
                    break;
                case NodeType.NETWORK_NODE:
                    var networkNodeCurrentNumber = networkNodesNumber;
                    elements.Add(new NetNode(x, y, "NN" + networkNodesNumber++));
                    consoleTextBox.AppendText("Network Node added at: " + x + "," + y);
                    consoleTextBox.AppendText(Environment.NewLine);
                    row["id"] = networkNodeCurrentNumber;
                    row["Type"] = "Network";
                    row["Name"] = "NN" + networkNodeCurrentNumber;
                    table.Rows.Add(row);
                    bSource.DataSource = table;
                    dataGridView1.DataSource = bSource;
                    dataGridView1.Update();
                    dataGridView1.Refresh();
                    break;
                case NodeType.DELETE:
                    List<String> atPosition = findElementsByPosition(x, y).Select(i => i.Name).ToList();
                    foreach (String toDelete in atPosition)
                        deleteListBox.Items.Add(toDelete);
                    if (deleteListBox.Items != null && atPosition.Count > 1)
                    {
                        deleteListBox.Items.Add("Cancel");
                        deleteListBox.Location = new Point(x, y);
                        deleteListBox.Visible = true;
                        deleteListBox.Enabled = true;
                        autofit();
                    }
                    else
                    {
                        int idxOfElement = elements.IndexOf(elements.Where(
                            i => i.Name.Equals(atPosition.First())
                            ).FirstOrDefault());
                        if (idxOfElement != -1)
                            elements.RemoveAt(idxOfElement);
                    }
                    break;
            }
            containerPictureBox.Refresh();
        }
        private void containerPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            putToGrid(ref x, ref y);
            if (nType == NodeType.CONNECTION)
            {
                nodeFrom = getNodeFrom(x, y);
                isDrawing = true;
            }
            else if (nType == NodeType.DOMAIN)
            {
                domainFrom = new Point(x, y);
                isDrawing = true;
            }
            else if (nType == NodeType.MOVE)
            {
                isDrawing = true;
                //Delete node and connections before move
                if (findElementsByPosition(x, y).Count > 0)
                    nodeFrom = findElementsByPosition(x, y).ElementAt(0);
                if (nodeFrom != null)
                {
                    int idxOfElement = elements.IndexOf(elements.Where(i => i.Name.Equals(nodeFrom.Name)).FirstOrDefault());
                    if (idxOfElement != -1)
                        elements.RemoveAt(idxOfElement);
                    List<String> atPosition = findElementsByPosition(x, y).Select(i => i.Name).ToList();
                    foreach (String toMove in atPosition)
                    {
                        idxOfElement = elements.IndexOf(elements.Where(i => i.Name.Equals(toMove)).FirstOrDefault());
                        Console.WriteLine(toMove);
                        if (idxOfElement != -1)
                        {
                            elementsTemp.Add(elements.Where(i => i.Name.Equals(toMove)).FirstOrDefault());
                            elements.RemoveAt(idxOfElement);
                        }

                    }
                }
                System.Threading.Thread.Sleep(10);
                containerPictureBox.Refresh();
            }
        }
        private void containerPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            putToGrid(ref x, ref y);
            isDrawing = false;
            if (nType == NodeType.CONNECTION && nodeFrom != null)
            {
                ContainerElement nodeTo = getNodeFrom(x, y);
                if (nodeTo != null)
                    bind(nodeFrom, nodeTo);
                else if (virtualNodeTo != null)
                    bind(nodeFrom, virtualNodeTo);
            }
            else if (nType == NodeType.DOMAIN && domainFrom != null)
            {
                Point domainTo = new Point(x,y);
                if (domainFrom.X > x && domainFrom.Y < y)
                {
                    Point tmpFrom = new Point(domainTo.X, domainFrom.Y);
                    Point tmpTo = new Point(domainFrom.X, domainTo.Y);
                    domainFrom = tmpFrom;
                    domainTo = tmpTo;
                }
                else if (domainFrom.X > x && domainFrom.Y > y)
                {
                    domainTo = domainFrom;
                    domainFrom = new Point(x, y);
                }
                else if (domainFrom.X < x && domainFrom.Y > y)
                {
                    Point tmpFrom = new Point(domainFrom.X, domainTo.Y);
                    Point tmpTo = new Point(domainTo.X, domainFrom.Y);
                    domainFrom = tmpFrom;
                    domainTo = tmpTo;
                }

                Domain toAdd = new Domain(domainFrom, domainTo);
                addDomainToElements(toAdd);
            }
            else if (nType == NodeType.MOVE && nodeFrom != null)
            {
                if (x > containerPictureBox.Size.Width)
                {
                    x = containerPictureBox.Size.Width;
                    if (y > containerPictureBox.Size.Height)
                        y = containerPictureBox.Size.Height;
                    else if (y < 0)
                        y = 0;
                }
                else if (x < 0)
                {
                    x = 0;
                    if (y > containerPictureBox.Size.Height)
                        y = containerPictureBox.Size.Height;
                    else if (y < 0)
                        y = 0;
                }


                virtualNodeTo = new ClientNode(x, y, nodeFrom.Name);
                elements.Add(virtualNodeTo);
                foreach (var elem in elementsTemp.AsParallel().Where(i => i is NodeConnection))
                    if (elem.ContainedPoints.ElementAt(0).Equals(nodeFrom.ContainedPoints.ElementAt(0)))
                        bind(getNodeFrom(elem.ContainedPoints.ElementAt(1).X, elem.ContainedPoints.ElementAt(1).Y), virtualNodeTo);
                    else if (elem.ContainedPoints.ElementAt(1).Equals(nodeFrom.ContainedPoints.ElementAt(0)))
                        bind(getNodeFrom(elem.ContainedPoints.ElementAt(0).X, elem.ContainedPoints.ElementAt(0).Y), virtualNodeTo);

                consoleTextBox.AppendText("Client Node moved from: " + nodeFrom.ContainedPoints.ElementAt(0).X + "," + nodeFrom.ContainedPoints.ElementAt(0).Y + " to:" +
                    x + "," + y);
                consoleTextBox.AppendText(Environment.NewLine);
                nodeFrom = null;
                elementsTemp.Clear();
            }
            containerPictureBox.Refresh();
        }
        private void containerPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            //if (nodeFrom == null)
            //    return;
            if (isDrawing && nodeFrom != null && nType == NodeType.CONNECTION)
            {
                containerPictureBox.Refresh();
                Point fromNode = new Point(nodeFrom.ContainedPoints.ElementAt(0).X, nodeFrom.ContainedPoints.ElementAt(0).Y);
                Point to = new Point(e.X, e.Y);

                double distance = Double.PositiveInfinity;
                double temporartDistance = Double.PositiveInfinity;

                foreach (var elem in elements.AsParallel().Where(i => i is NetNode && i != nodeFrom))
                {
                    temporartDistance = Math.Round(Math.Sqrt(Math.Pow(elem.ContainedPoints.ElementAt(0).X - e.X, 2) + Math.Pow(elem.ContainedPoints.ElementAt(0).Y - e.Y, 2)), 2);
                    if (temporartDistance < distance)
                    {
                        distance = temporartDistance;
                        virtualNodeTo = elem;
                    }
                    temporartDistance = Double.PositiveInfinity;
                }
                foreach (var elem in elements.AsParallel().Where(i => i is ClientNode && i != nodeFrom))
                {
                    temporartDistance = Math.Round(Math.Sqrt(Math.Pow(elem.ContainedPoints.ElementAt(0).X - e.X, 2) + Math.Pow(elem.ContainedPoints.ElementAt(0).Y - e.Y, 2)), 2);
                    if (temporartDistance < distance)
                    {
                        distance = temporartDistance;
                        virtualNodeTo = elem;
                    }
                    temporartDistance = Double.PositiveInfinity;
                }
                Pen blackPen = new Pen(Color.Black, 3);
                if (distance > 100)
                    //ControlPaint.DrawReversibleLine(fromNode, to, Color.Black);
                    myGraphics.DrawLine(blackPen, fromNode, to);
                else
                {
                    Point end = new Point(virtualNodeTo.ContainedPoints.ElementAt(0).X, virtualNodeTo.ContainedPoints.ElementAt(0).Y);
                    myGraphics.DrawLine(blackPen, fromNode, end);
                }
                System.Threading.Thread.Sleep(10);
            }
            else if (isDrawing && nType == NodeType.DOMAIN)
            {
                containerPictureBox.Refresh();

                if (e.X - domainFrom.X < 0 && e.Y - domainFrom.Y < 0)
                {
                    myGraphics.DrawRectangle(new Pen(Color.PaleVioletRed, 3), e.X,
                        e.Y, domainFrom.X - e.X, domainFrom.Y - e.Y);
                }
                else if (e.X - domainFrom.X < 0)
                {
                    myGraphics.DrawRectangle(new Pen(Color.PaleVioletRed, 3), e.X,
                        domainFrom.Y, domainFrom.X - e.X, e.Y - domainFrom.Y);
                }
                else if (e.Y - domainFrom.Y < 0)
                {
                    myGraphics.DrawRectangle(new Pen(Color.PaleVioletRed, 3), domainFrom.X,
                        e.Y, e.X - domainFrom.X, domainFrom.Y - e.Y);
                }
                else
                {
                    myGraphics.DrawRectangle(new Pen(Color.PaleVioletRed, 3), domainFrom.X,
                        domainFrom.Y, e.X - domainFrom.X, e.Y - domainFrom.Y);
                }

                System.Threading.Thread.Sleep(10);
            }
            else if (isDrawing && nType == NodeType.MOVE && nodeFrom != null)
            {
                Rectangle rect;
                containerPictureBox.Refresh();

                rect = new Rectangle(e.X - 5, e.Y - 5, 11, 11);
                myGraphics.FillEllipse(Brushes.ForestGreen, rect);
                myGraphics.DrawEllipse(Pens.Black, rect);
                myGraphics.DrawString(nodeFrom.Name, new Font("Arial", 5), Brushes.Black, new Point(e.X + 3, e.Y + 3));

                foreach (var elem in elementsTemp.AsParallel().Where(i => i is NodeConnection))
                {
                    if (elem.ContainedPoints.Contains(nodeFrom.ContainedPoints.ElementAt(0)))
                    {
                        Point from = elem.ContainedPoints.ElementAt(0).Equals(nodeFrom.ContainedPoints.ElementAt(0)) ?
                            elem.ContainedPoints.ElementAt(1) : elem.ContainedPoints.ElementAt(0);
                        Point to = new Point(e.X, e.Y);
                        myGraphics.DrawLine(new Pen(Color.Black, 2), from, to);
                        myGraphics.DrawString(elem.Name, new Font("Arial", 5), Brushes.Black, new Point((from.X + to.X) / 2 + 3,
                           (from.Y + to.Y) / 2 + 3));
                    }

                }
                System.Threading.Thread.Sleep(10);
            }
        }
        // DELETE LISTBOX ACTIONS
        private void deleteListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int idxOfElement = elements.IndexOf(elements.Where(
                           i => i.Name.Equals(deleteListBox.SelectedItem)
                           ).FirstOrDefault());
            if (idxOfElement != -1)
                elements.RemoveAt(idxOfElement);
            deleteListBox.Visible = false;
            deleteListBox.Enabled = false;
            deleteListBox.Items.Clear();
            containerPictureBox.Refresh();
        }
        // Auto wigth adding to listbox
        private void autofit()
        {
            int width = deleteListBox.Width;
            using (Graphics g = deleteListBox.CreateGraphics())
            {
                for (int i1 = 0; i1 < deleteListBox.Items.Count; i1++)
                {
                    int itemWidth = Convert.ToInt32(g.MeasureString(Convert.ToString(deleteListBox.Items[i1]), deleteListBox.Font).Width);
                    width = Math.Max(width, itemWidth);
                }
            }
            deleteListBox.Width = width;
        }
        // Validate adding domain to container
        private void addDomainToElements(Domain toAdd)
        {
            bool add = true;
            foreach (Domain d in elements.AsParallel().Where(i => i is Domain))
            {
                if (toAdd.crossingOtherDomain(d))
                {
                    add = false;
                    break;
                }
            }
            if (add)
            {
                elements.Add(toAdd);
                consoleTextBox.AppendText("Domain added");
                consoleTextBox.AppendText(Environment.NewLine);
            }
            else
            {
                consoleTextBox.AppendText("Domains can't cross each others.");
                consoleTextBox.AppendText(Environment.NewLine);
            }
        }
        // Returns index of element in elements by position
        private List<ContainerElement> findElementsByPosition(int x, int y)
        {
            List<ContainerElement> result = elements.AsParallel().Where(
                i => i.ContainedPoints.Where(p => p.Equals(new Point(x,y))).FirstOrDefault() != default(Point)
                ).ToList();
            return result;
        }
        // Returns x,y to closest grid point
        private void putToGrid(ref int x, ref int y)
        {
            x = GAP * (int)Math.Round((double)x / GAP);
            y = GAP * (int)Math.Round((double)y / GAP);
        }
        // Returns ContainterElement associated with given x,y
        private ContainerElement getNodeFrom(int x, int y)
        {
            return elements.Where(i => i.ContainedPoints.ElementAtOrDefault(0).X == x &&
               i.ContainedPoints.ElementAtOrDefault(0).Y == y).FirstOrDefault();
        }
        // Binding from  Node A to Node B with NodeConnection
        private void bind(ContainerElement nodeFrom, ContainerElement nodeTo)
        {
            elements.Add(new NodeConnection(nodeFrom, nodeTo, nodeFrom.Name + "-" + nodeTo.Name));
            consoleTextBox.AppendText("Connection  added");
            consoleTextBox.AppendText(Environment.NewLine);
        }


        private void clientNodeBtn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            nType = NodeType.CLIENT_NODE;
        }
        private void networkNodeBtn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            nType = NodeType.NETWORK_NODE;
        }

        private void connectionBtn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.CONNECTION;
        }

        private void domainBtn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.DOMAIN;
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            nType = NodeType.DELETE;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            nType = NodeType.MOVE;
        }
    }



    enum NodeType
    {
        CLIENT_NODE,
        NETWORK_NODE,
        CONNECTION,
        DOMAIN,
        DELETE,
        MOVE,
        NOTHING
    }
}
