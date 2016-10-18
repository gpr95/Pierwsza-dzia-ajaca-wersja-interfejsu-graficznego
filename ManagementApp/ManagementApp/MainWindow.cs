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
        private int clientNodesNumber;
        private int networkNodesNumber;

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
            switch (nType)
            {
                case NodeType.CLIENT_NODE:
                    elements.Add(new ClientNode(x, y, "CN" + clientNodesNumber++));
                    consoleTextBox.AppendText("Client Node added at: " + x + "," + y);
                    consoleTextBox.AppendText(Environment.NewLine);
                    break;
                case NodeType.NETWORK_NODE:
                    elements.Add(new NetNode(x, y, "NN" + networkNodesNumber++));
                    consoleTextBox.AppendText("Network Node added at: " + x + "," + y);
                    consoleTextBox.AppendText(Environment.NewLine);
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
            if (nType == NodeType.DOMAIN)
            {
                domainFrom = new Point(x, y);
                isDrawing = true;
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
            if (nType == NodeType.DOMAIN && domainFrom != null && domainFrom.X < x && domainFrom.Y < y)
            {
                Point domainTo = new Point(x, y);
                Domain toAdd = new Domain(domainFrom, domainTo);
                addDomainToElements(toAdd);
            }
            containerPictureBox.Refresh();
        }
        private void containerPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
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
                myGraphics.DrawRectangle(new Pen(Color.PaleVioletRed, 3), domainFrom.X, 
                    domainFrom.Y, e.X - domainFrom.X, e.Y - domainFrom.Y);
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
            elements.Add(new NodeConnection(nodeFrom, nodeTo,nodeFrom.Name + "-" + nodeTo.Name));
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
    }



    enum NodeType
    {
        CLIENT_NODE,
        NETWORK_NODE,
        CONNECTION,
        DOMAIN,
        DELETE,
        NOTHING
    }
}
