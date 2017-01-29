using ManagementApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientWindow
{
    public partial class ClientWindow : Form
    {
        public string virtualIP;
        private TcpListener listener;
        private TcpClient managmentClient;
        private static BinaryWriter writer;
        private static bool cyclic_sending = false;
        string[] args2 = new string[3];
        private int currentSpeed = 3;
        private static string path;
        private Dictionary<String, int> possibleDestinations = new Dictionary<string, int>();
        private int virtualPort = 1;
        private int managementPort;
        private CPCC controlAgent;
        public List<int> slots;
        Random r;


        public ClientWindow(string[] args)
        {
            virtualIP = args[0];
            int cloudPort = Convert.ToInt32(args[1]);
            managementPort = Convert.ToInt32(args[2]);
            slots = new List<int>();

            string fileName = virtualIP.Replace(".", "_") + "_" + DateTime.Now.ToLongTimeString().Replace(":", "_") + "_" + DateTime.Now.ToLongDateString().Replace(" ", "_");
            path = System.IO.Directory.GetCurrentDirectory() + @"\logs\" + fileName + ".txt";

            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), cloudPort);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
            Thread managementThreadad = new Thread(new ParameterizedThreadStart(initManagmentConnection));
            managementThreadad.Start(managementPort);
            InitializeComponent();
            this.Text = virtualIP;
            Log2("INFO", "START LOG");
            Log2("DEBUG", "cloud port: "+cloudPort);
            controlAgent = new CPCC(this, args[3]);
            initSpeedComboBox();
            r = new Random();
        }

        private void initSpeedComboBox()
        {
            for(int i=1; i<4; i++)
            speedComboBox.Items.Add(i);

        }

        private void Listen()
        {
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                clientThread.Start(client);
            }
        }


        private void ListenThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            writer = new BinaryWriter(clienttmp.GetStream());
            while (true)
            {
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(Signal))
                {
                    Signal received_signal = received_object.Value.ToObject<Signal>();
                    if (received_signal.stm1 != null)
                    {
                        STM1 received_frame = received_signal.stm1;
                        if (received_frame.vc4.vc3List.Count > 0)
                        {
                            slots.Clear();
                            foreach (KeyValuePair<int, VirtualContainer3> v in received_frame.vc4.vc3List)
                            {

                                receivedTextBox.AppendText(DateTime.Now.ToLongTimeString() + " : " + v.Value.C3);
                                receivedTextBox.AppendText(Environment.NewLine);
                                Log1("IN", virtualIP, received_signal.port.ToString(), v.Key, "VC-3", v.Value.POH.ToString(), v.Value.C3);
                                slots.Add(v.Key);
                            }

                        }
                        else
                        {
                           
                            receivedTextBox.AppendText(DateTime.Now.ToLongTimeString() + " : " + received_frame.vc4.C4);
                            receivedTextBox.AppendText(Environment.NewLine);
                            Log1("IN", virtualIP, received_signal.port.ToString(), 1, "VC-4", received_frame.vc4.POH.ToString(), received_frame.vc4.C4);
                        }

                        logTextBox.AppendText(Environment.NewLine);
                        logTextBox.Paste("Path: \n");
                        foreach(var log in received_signal.path)
                        {
                            logTextBox.Paste(log + " -> ");
                        }
                        logTextBox.Paste(this.virtualIP);
                    }
                    else if (received_signal.lrmProtocol != null)
                    {
                        string lrmProtocol = received_signal.lrmProtocol;
                        int port = received_signal.port;
                        string[] temp = lrmProtocol.Split(' ');
                        if (temp[0] == "whoyouare")
                        {
                            string message = "iam " + this.virtualIP;
                            Signal signal = new Signal(port, message);
                            string data = JMessage.Serialize(JMessage.FromValue(signal));
                            writer.Write(data);
                        }
                    }
                }
                else
                {

                    Log2("ERR", "Received unknown data type from client");
                }
            }

            // reader.Close();
        }

        private void initManagmentConnection(Object managementPort)
        {
            try
            {
                managmentClient = new TcpClient("127.0.0.1", (int)managementPort);
                BinaryReader reader = new BinaryReader(managmentClient.GetStream());
                BinaryWriter writer = new BinaryWriter(managmentClient.GetStream());
                while (true)
                {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type == typeof(Management.ManagmentProtocol))
                    {
                        Management.ManagmentProtocol management_packet = received_object.Value.ToObject<Management.ManagmentProtocol>();
                        if (management_packet.State == Management.ManagmentProtocol.WHOIS)
                        {
                            Management.ManagmentProtocol packet_to_management = new Management.ManagmentProtocol();
                            packet_to_management.Name = virtualIP;
                            String send_object = JMessage.Serialize(JMessage.FromValue(packet_to_management));
                            writer.Write(send_object);
                        }
                        else if (management_packet.State == Management.ManagmentProtocol.POSSIBLEDESITATIONS)
                        {
                            foreach(var c in management_packet.PossibleDestinations)
                            {
                                if (!c.Key.Equals(virtualIP))
                                    comboBox1.Items.Add(c.Key);
                            }
                            //this.possibleDestinations = management_packet.PossibleDestinations;
                            //this.virtualPort = management_packet.Port;
                            ////logTextBox.AppendText("Virtual Port: " + virtualPort);
                            //List<string> destinations = new List<string>(this.possibleDestinations.Keys);

                            //sendComboBox.Items.Clear();
                            //sendComboBox.Refresh();
                            //for (int i = 0; i < destinations.Count; i++)
                            //{
                            //    //DEBUG
                            //    //Log2("MAGAGEMENT INFO", destinations[i] + " : " + possibleDestinations[destinations[i]]);
                            //    sendComboBox.Items.Add(destinations[i]);
                            //}
                        }

                    }
                    else
                    {
                        Log2("ERR", "Received unknown data type from management");
                    }
                }

            }
            catch (Exception e)
            {

                //debug
                // Console.WriteLine(e.Message);
                Log2("ERR", "Could not connect on management interface");
                Thread.Sleep(4000);
                Environment.Exit(0);

            }
        }

        private void sendSwiched(string message)
        {
            try
            {
                Dictionary<int, VirtualContainer3> vc3List = new Dictionary<int, VirtualContainer3>();
                foreach (int slot in slots)
                {
                    Log2("DEBUG", "sloty od controla: "+slot);
                    VirtualContainer3 vc3 = new VirtualContainer3(adaptation(), "Slot"+slot+" :"+message);
                    vc3List.Add(slot, vc3);
                }
                    STM1 frame = new STM1(adaptation(),vc3List);
                    //SYGNAL
                    List<string> temp = new List<string>();
                    temp.Add(this.virtualIP);
                    Signal signal = new Signal(virtualPort, frame, temp);
                    string data = JMessage.Serialize(JMessage.FromValue(signal));
                    writer.Write(data);
                    foreach (KeyValuePair<int, VirtualContainer3> v in frame.vc4.vc3List)
                    {
                        Log1("OUT", virtualIP, virtualPort.ToString(), v.Key, "VC-3", v.Value.POH.ToString(), v.Value.C3);
                    }
               
                sendingTextBox.Clear();
            }
            catch (Exception e)
            {

                Log2("ERR", "Error sending signal");
            }
        }

        private void sendHard(string message, string slot)
        {
            try
            {
                logTextBox.AppendText("DEBISAHFOIS");
                Dictionary<int, VirtualContainer3> vc3List = new Dictionary<int, VirtualContainer3>();
                VirtualContainer3 msg = new VirtualContainer3(adaptation(), message);
                vc3List.Add(int.Parse(slot), msg);
                STM1 frame = new STM1(adaptation(), vc3List);
                List<string> temp = new List<string>();
                temp.Add(this.virtualIP);
                Signal signal = new Signal(virtualPort, frame, temp);
                string data = JMessage.Serialize(JMessage.FromValue(signal));
                writer.Write(data);
                foreach (KeyValuePair<int, VirtualContainer3> v in frame.vc4.vc3List)
                {
                    Log1("OUT", virtualIP, virtualPort.ToString(), v.Key, "VC-3", v.Value.POH.ToString(), v.Value.C3);
                }

                sendingTextBox.Clear();
            }
            catch (Exception e)
            {

                Log2("ERR", "Error sending signal");
            }
        }

        public int adaptation()
        {
            int POH = r.Next(10000, 40000);
            return POH;
        }
       



        private void sendPeriodically(int period, string message)
        {


            Thread myThread = new Thread(async delegate ()
            {
                bool isVc3 = false;
                Signal signal;
                STM1 frame;

                string data;
                Dictionary<int, VirtualContainer3> vc3List = new Dictionary<int, VirtualContainer3>();
                foreach (int slot in slots)
                {
                    VirtualContainer3 vc3 = new VirtualContainer3(adaptation(), "Slot" + slot + " :" + message);
                    vc3List.Add(slot, vc3);
                }
                frame = new STM1(adaptation(), vc3List);
                //SYGNAL
                List<string> temp = new List<string>();
                temp.Add(this.virtualIP);
                signal = new Signal(virtualPort, frame, temp);
                data = JMessage.Serialize(JMessage.FromValue(signal));



                while (cyclic_sending)
                {

                    try
                    {
                        writer.Write(data);
                            foreach (KeyValuePair<int, VirtualContainer3> v in frame.vc4.vc3List)
                            {
                                Log1("OUT", virtualIP, virtualPort.ToString(), v.Key, "VC-3", v.Value.POH.ToString(), v.Value.C3);
                            }

                        await Task.Delay(TimeSpan.FromMilliseconds(period));
                    }
                    catch (Exception e)
                    {


                        Log2("ERR", "Error sending signal: ");
                        break;
                    }

                }


            });
            myThread.Start();
        }

        public void Log1(string type, string clientNodeName, string currentPort, int currentSlot, string containerType, string POH, string message)
        {

            StreamWriter writer = File.AppendText(path);
            writer.WriteLine("\r\n{0} {1} : {2} {3} {4} {5} {6} {7} {8} ", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(),
                type,
                clientNodeName,
                currentPort,
                currentSlot,
                containerType,
                POH,
                message);
            writer.Flush();
            writer.Close();



            if (this.InvokeRequired)
            {
                log1RowCallback d = new log1RowCallback(Log1);
                this.Invoke(d, new object[] { type, clientNodeName, currentPort, currentSlot, containerType, POH, message });
            }
            else
            {
                logTextBox.Paste("\r\n" + DateTime.Now.ToLongTimeString() + " : " + "[" + type + "]"
               + " " + currentPort + " " + currentSlot.ToString() + " " + containerType + " " + POH + " " + message);
                logTextBox.AppendText(Environment.NewLine);
            }
        }

        delegate void log1RowCallback(string type, string clientNodeName, string currentPort, int currentSlot, string containerType, string POH, string message);

        public void Log2(string type, string message)
        {

            StreamWriter writer = File.AppendText(path);
            writer.WriteLine("\r\n{0} {1} : {2} {3}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(),
                type,
                message);
            writer.Flush();
            writer.Close();



            if (this.InvokeRequired)
            {
                log2RowCallback d = new log2RowCallback(Log2);
                this.Invoke(d, new object[] { type, message });
            }
            else
            {
                logTextBox.AppendText("\r\n" + DateTime.Now.ToLongTimeString() + " : " + "[" + type + "]" + " " + message);
                logTextBox.AppendText(Environment.NewLine);
            }
        }

        delegate void log2RowCallback(string type, string message);

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Name.Equals("tabPage1"))
                sendSwiched(sendingTextBox.Text);
            else if (tabControl1.SelectedTab.Name.Equals("tabPage2"))
                sendHard(sendingTextBox.Text, slotTextBox.Text);
            else if (tabControl1.SelectedTab.Name.Equals("tabPage3"))
                sendSoft(sendingTextBox.Text, slotTextBox.Text);
        }

        private void sendSoft(string p1, string p2)
        {
            
        }

        private void sendPeriodicallyBtn_Click(object sender, EventArgs e)
        {
            int time;
            bool res = int.TryParse(timeTextBox.Text, out time);
            if (res)
            {

                cyclic_sending = true;
                sendPeriodically(time, sendingTextBox.Text);
                sendingTextBox.Clear();
                timeTextBox.Clear();
            }
            else
            {
                logTextBox.AppendText(DateTime.Now.ToLongTimeString() + " : " + "Wrong period format");
                logTextBox.AppendText(Environment.NewLine);
                timeTextBox.Clear();
            }


        }

        private void stopSendingBtn_Click(object sender, EventArgs e)
        {
            cyclic_sending = false;
        }

       

        private void ClientWindow_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }



        private void connectButton_Click(object sender, EventArgs e)
        {
            try {
                Address address;
                logTextBox.AppendText("#" + comboBox1.GetItemText(comboBox1.SelectedItem) + "#");
                if (addressTextBox.Text.Equals(""))
                    address = new Address(comboBox1.GetItemText(comboBox1.SelectedItem));
                else
                    address = new Address(addressTextBox.Text);
                controlAgent.connect();
                int speed;
                bool res = int.TryParse(speedComboBox.SelectedItem.ToString(), out speed);
                controlAgent.sendRequest(addressTextBox.Text, speed);
            }
            catch(Exception es)
            {
                Log2("ERR", "Wrong address format");
                addressTextBox.Clear();
            }

            
        }

        private void speedComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentSpeed = Convert.ToInt32(speedComboBox.SelectedItem.ToString());
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            string destinationNode = "";

            int slot;
            bool res = int.TryParse(slotTextBox.Text, out slot);
            if (res)
            {

                if (this.possibleDestinations.ContainsKey(destinationNode))
                {
                    possibleDestinations[destinationNode] = slot;
                }
                else
                {
                    if (destinationNode != "")
                        possibleDestinations.Add(destinationNode, slot);
                }
                List<string> destinations = new List<string>(this.possibleDestinations.Keys);
                addressTextBox.Text = destinationNode;

                //nodeTextBox.Clear();
                slotTextBox.Clear();
            }
            else
            {
                logTextBox.AppendText(DateTime.Now.ToLongTimeString() + " : " + "Wrong slot format");
                logTextBox.AppendText(Environment.NewLine);
                slotTextBox.Clear();
            }

            int virtualPort;
            bool res2 = int.TryParse("", out virtualPort);
            if (res2)
            {
                this.virtualPort = virtualPort;
            }
            else
            {
                logTextBox.AppendText(DateTime.Now.ToLongTimeString() + " : " + "Wrong port format");
                logTextBox.AppendText(Environment.NewLine);

            }
            //portTextBox.Clear();
        }

        
    }
}
