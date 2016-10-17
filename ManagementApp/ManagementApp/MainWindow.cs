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
        private int x;
        private int y;
        private NodeType nType;
        private const int GAP = 10;
        private List<ContainerElement> elements = new List<ContainerElement>();
        private List<Point> clientNodes = new List<Point>();
        private List<Point> networkNodes = new List<Point>();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            nType = NodeType.CLIENT_NODE;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            nType = NodeType.NETWORK_NODE;
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
                 //   clientNodes.Add(pt);
                    break;
                case NodeType.NETWORK_NODE:
                    elements.Add(new NetNode(x, y));
                 //   networkNodes.Add(pt);
                    break;
            }
            container.Refresh();
        }

        private void container_Paint_1(object sender, PaintEventArgs e)
        {
            Graphics panel = e.Graphics;
            Bitmap bm = new Bitmap(
            container.ClientSize.Width,
            container.ClientSize.Height);
            for (int x = 0; x < container.ClientSize.Width;
                x += GAP)
            {
                for (int y = 0; y < container.ClientSize.Height;
                    y += GAP)
                {
                    bm.SetPixel(x, y, Color.Black);
                }
            }
            //foreach (var pt in clientNodes)
            //{
            //    Rectangle rect = new Rectangle(pt.X - 5, pt.Y - 5, 11, 11);
            //    panel.FillEllipse(Brushes.AliceBlue, rect);
            //    panel.DrawEllipse(Pens.Black, rect);
            //}
            //foreach (var pt in networkNodes)
            //{
            //    Rectangle rect = new Rectangle(pt.X - 5, pt.Y - 5, 11, 11);
            //    panel.FillEllipse(Brushes.Bisque, rect);
            //    panel.DrawEllipse(Pens.Black, rect);
            //}
            Rectangle rect;
            foreach (var elem in elements)
            {
                if(elem is NetNode)
                {
                    rect = new Rectangle(elem.ContainedPoints.ElementAt(0).X- 5, 
                        elem.ContainedPoints.ElementAt(0).Y - 5, 11, 11);
                    panel.FillEllipse(Brushes.Bisque, rect);
                    panel.DrawEllipse(Pens.Black, rect);

                }
                else if(elem is ClientNode)
                {
                     rect = new Rectangle(elem.ContainedPoints.ElementAt(0).X - 5,
                        elem.ContainedPoints.ElementAt(0).Y - 5, 11, 11);
                    panel.FillEllipse(Brushes.AliceBlue, rect);
                    panel.DrawEllipse(Pens.Black, rect);
                }
            }

            container.BackgroundImage = bm;
            this.Refresh();
        }
    }

    enum NodeType
    {
        CLIENT_NODE,
        NETWORK_NODE,
        NOTHING
    }
}
