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
    partial class CreatingTrailWindow : Form
    {
        ManagementPlane management;
        List<Node> nodeList;
        List<NodeConnection> connectionList;
        DataTable table;
        Node a, b;
        public CreatingTrailWindow(List<Node> nodeList, List<NodeConnection> connectionList, ManagementPlane c)
        {
            this.management = c;
            this.nodeList = nodeList;
            this.connectionList = connectionList;
            MakeTable();
            InitializeComponent();
            foreach(Node client in nodeList)
            {
                if (client is ClientNode)
                {
                    startComboBox.Items.Add(client.Name);
                    stopComboBox.Items.Add(client.Name);
                }        
            }
        }

        private DataTable MakeTable()
        {
            //Fix needed
            table = new DataTable("connectionToTrail");
            var column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Slot";
            column.AutoIncrement = false;
            column.Caption = "ParentItem";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);


            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Connection";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            var dtSet = new DataSet();
            dtSet.Tables.Add(table);

            return table;
        }

        private void calculateBtn_Click(object sender, EventArgs e)
        {
            if (startComboBox.SelectedIndex == stopComboBox.SelectedIndex)
                return;
            else
            {
                table.Rows.Clear();
                
                a = nodeList.Where(n => n.Name.Equals(startComboBox.Text)).FirstOrDefault();
                b = nodeList.Where(n => n.Name.Equals(stopComboBox.Text)).FirstOrDefault();
                if (a == default(Node) || b == default(Node))
                    return;
                Trail t = management.createTrail(a, b, checkBox1.Checked);
                foreach(var con in t.ConnectionDictionary)
                {
                    var row = table.NewRow();
                    row["Slot"] = con.Value;
                    row["Connection"] = con.Key.Name;
                    table.Rows.Add(row);
                }

                this.connectionsDataGridView.RowHeadersVisible = false;
                this.connectionsDataGridView.AllowUserToAddRows = false;
                this.connectionsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.connectionsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                var bSource = new BindingSource();
                bSource.DataSource = table;
                connectionsDataGridView.DataSource = bSource;
                connectionsDataGridView.Update();
                connectionsDataGridView.Refresh();
                t.clearTrail(t);
            }
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            Trail t = management.createTrail(a, b, checkBox1.Checked);
            if (t.From == null || t.To == null || t.StartingSlot == -1)
                return;
            management.addTrail(t);
            this.Dispose();
        }
    }
}
