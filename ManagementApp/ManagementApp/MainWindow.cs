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
        private NodeType nType;
        private const int GAP = 10;
        private Bitmap containerPoints;
        private List<ContainerElement> elements = new List<ContainerElement>();
        private ContainerElement nodeFrom;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.CLIENT_NODE;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.NETWORK_NODE;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.CONNECTION;
        }


        private void putToGrid(ref int x, ref int y)
        {
            x = GAP * (int)Math.Round((double)x / GAP);
            y = GAP * (int)Math.Round((double)y / GAP);
        }

        private void container_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            putToGrid(ref x, ref y);
            switch (nType)
            {
                case NodeType.CLIENT_NODE:
                    elements.Add(new ClientNode(x, y));
                    textConsole.AppendText("Client Node added at: " + x + "," + y);
                    textConsole.AppendText(Environment.NewLine);
                    break;
                case NodeType.NETWORK_NODE:
                    elements.Add(new NetNode(x, y));
                    textConsole.AppendText("Network Node added at: " + x + "," + y);
                    textConsole.AppendText(Environment.NewLine);
                    break;
            }
            container.Refresh();
        }

        private ContainerElement getNodeFrom(int x, int y)
        {
            return elements.Where(i => i.ContainedPoints.ElementAtOrDefault(0).X == x &&
               i.ContainedPoints.ElementAtOrDefault(0).Y == y).FirstOrDefault(); 
        }

        private void container_Paint_1(object sender, PaintEventArgs e)
        {
            Graphics panel = e.Graphics;
   
            Rectangle rect;
            foreach (var elem in elements.AsParallel().Where(i => i is NetNode))
            {
                rect = new Rectangle(elem.ContainedPoints.ElementAt(0).X - 5,
                    elem.ContainedPoints.ElementAt(0).Y - 5, 11, 11);
                panel.FillEllipse(Brushes.Bisque, rect);
                panel.DrawEllipse(Pens.Black, rect);
            }
            foreach (var elem in elements.AsParallel().Where(i => i is ClientNode))
            {
                rect = new Rectangle(elem.ContainedPoints.ElementAt(0).X - 5,
                   elem.ContainedPoints.ElementAt(0).Y - 5, 11, 11);
                panel.FillEllipse(Brushes.AliceBlue, rect);
                panel.DrawEllipse(Pens.Black, rect);
            }
            foreach (var elem in elements.AsParallel().Where(i => i is NodeConnection))
            {
                // Create pen.
                Pen blackPen = new Pen(Color.Black, 3);
                // Draw line to screen.
                panel.DrawLine(blackPen, elem.ContainedPoints.ElementAt(0), elem.ContainedPoints.ElementAt(1));
            }
              
         

            container.BackgroundImage = containerPoints;
            this.Refresh();
        }

        private void container_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            putToGrid(ref x, ref y);
            if(nType==NodeType.CONNECTION)
                    nodeFrom = getNodeFrom(x, y);
        }

        private void container_MouseUp(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            putToGrid(ref x, ref y);
            if (nType == NodeType.CONNECTION && nodeFrom != null)
            {
                ContainerElement nodeTo = getNodeFrom(x, y);
                if(nodeTo != null)
                    bind(nodeFrom, nodeTo);
            }
            container.Refresh();
        }

        private void bind(ContainerElement nodeFrom, ContainerElement nodeTo)
        {
            elements.Add(new NodeConnection(nodeFrom, nodeTo));
            elements.Add(new NodeConnection(nodeTo, nodeFrom));
            textConsole.AppendText("Connection  added");
            textConsole.AppendText(Environment.NewLine);
        }

        private void Connection_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                this.Cursor = Cursors.Arrow;
                nType = NodeType.NOTHING;
            }
        }
    }

    enum NodeType
    {
        CLIENT_NODE,
        NETWORK_NODE,
        CONNECTION,
        NOTHING
    }
}
