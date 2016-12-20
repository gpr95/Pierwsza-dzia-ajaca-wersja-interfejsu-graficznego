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
        private static string virtualIP;
        private TcpListener listener;
        private TcpClient managmentClient;
        private static BinaryWriter writer;
        private static bool cyclic_sending = false;
        string[] args2 = new string[3];
        private int currentSpeed = 3;
        private int currentSlot;
        private static string path;
        private Dictionary<String, int> possibleDestinations = new Dictionary<string, int>();
        private int virtualPort=1;
        private int managementPort;

        public ClientWindow(string[] args)
        {
            virtualIP = args[0];
            int cloudPort = Convert.ToInt32(args[1]);
            managementPort = Convert.ToInt32(args[2]);
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
                    STM1 received_frame = received_signal.stm1;
                    if (received_frame.vc4.C4 != null)
                    {

                        receivedTextBox.AppendText(DateTime.Now.ToLongTimeString() + " : " + received_frame.vc4.C4);
                        receivedTextBox.AppendText(Environment.NewLine);
                        Log1("IN", virtualIP, 1, "VC-4", received_frame.vc4.POH.ToString(), received_frame.vc4.C4);
                    }

                    else
                    {
                        foreach (KeyValuePair<int, VirtualContainer3> v in received_frame.vc4.vc3List)
                        {

                            receivedTextBox.AppendText(DateTime.Now.ToLongTimeString() + " : " + v.Value.C3);
                            receivedTextBox.AppendText(Environment.NewLine);
                            Log1("IN", virtualIP, v.Key, "VC-3", v.Value.POH.ToString(), v.Value.C3);
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
                    if (received_object.Type == typeof(ManagementApp.ManagmentProtocol))
                    {
                        ManagementApp.ManagmentProtocol management_packet = received_object.Value.ToObject<ManagementApp.ManagmentProtocol>();
                        if (management_packet.State == ManagementApp.ManagmentProtocol.WHOIS)
                        {
                            ManagementApp.ManagmentProtocol packet_to_management = new ManagementApp.ManagmentProtocol();
                            packet_to_management.Name = virtualIP;
                            String send_object = JMessage.Serialize(JMessage.FromValue(packet_to_management));
                            writer.Write(send_object);
                        }
                        else if (management_packet.State == ManagementApp.ManagmentProtocol.POSSIBLEDESITATIONS)
                        {

                            this.possibleDestinations = management_packet.possibleDestinations;
                            this.virtualPort = management_packet.Port;
                            //logTextBox.AppendText("Virtual Port: " + virtualPort);
                            List<string> destinations = new List<string>(this.possibleDestinations.Keys);

                            sendComboBox.Items.Clear();
                            sendComboBox.Refresh();
                            for (int i = 0; i < destinations.Count; i++)
                            {
                                //DEBUG
                                //Log2("MAGAGEMENT INFO", destinations[i] + " : " + possibleDestinations[destinations[i]]);
                                sendComboBox.Items.Add(destinations[i]);
                            }
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

        private void send(string message)
        {
           // logTextBox.AppendText("Virtual Port: " + virtualPort);
            try
            {
                if (currentSpeed == 3)
                {

                    VirtualContainer3 vc3 = new VirtualContainer3(adaptation(), message);
                    Dictionary<int, VirtualContainer3> vc3List = new Dictionary<int, VirtualContainer3>();
                    vc3List.Add(currentSlot, vc3);
                    STM1 frame = new STM1(vc3List);
                    //SYGNAL
                    Signal signal = new Signal(virtualPort, frame);
                    string data = JMessage.Serialize(JMessage.FromValue(signal));
                    writer.Write(data);
                    foreach (KeyValuePair<int, VirtualContainer3> v in frame.vc4.vc3List)
                    {
                        Log1("OUT", virtualIP, v.Key, "VC-3", v.Value.POH.ToString(), v.Value.C3);
                    }
                }
                else
                {
                    STM1 frame = new STM1(adaptation(), message);
                    Signal signal = new Signal(virtualPort, frame);
                    string data = JMessage.Serialize(JMessage.FromValue(signal));
                    writer.Write(data);
                    Log1("OUT", virtualIP, 1, "VC-4", frame.vc4.POH.ToString(), frame.vc4.C4);
                }
                sendingTextBox.Clear();
            }
            catch (Exception e)
            {

                Log2("ERR", "Error sending signal");
            }


        }

        private int adaptation()
        {
            Random r = new Random();
            int POH = r.Next(30000, 50000);
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
                if (currentSpeed == 3)
                {

                    VirtualContainer3 vc3 = new VirtualContainer3(adaptation(), message);
                    Dictionary<int, VirtualContainer3> vc3List = new Dictionary<int, VirtualContainer3>();
                    vc3List.Add(currentSlot, vc3);
                    frame = new STM1(vc3List);
                    //SYGNAL
                    signal = new Signal(virtualPort, frame);
                    data = JMessage.Serialize(JMessage.FromValue(signal));
                    isVc3 = true;

                }
                else
                {

                    frame = new STM1(adaptation(), message);
                    //SYGNAL
                    signal = new Signal(virtualPort, frame);
                    data = JMessage.Serialize(JMessage.FromValue(signal));

                }

                while (cyclic_sending)
                {

                    try
                    {

                        writer.Write(data);
                        if (isVc3)
                            foreach (KeyValuePair<int, VirtualContainer3> v in frame.vc4.vc3List)
                            {
                                Log1("OUT", virtualIP, v.Key, "VC-3", v.Value.POH.ToString(), v.Value.C3);
                            }
                        else
                            Log1("OUT", virtualIP, 1, "VC-4", frame.vc4.POH.ToString(), frame.vc4.C4);
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

        public void Log1(string type, string clientNodeName, int currentSlot, string containerType, string POH, string message)
        {

            StreamWriter writer = File.AppendText(path);
            writer.WriteLine("\r\n{0} {1} : {2} {3} {4} {5} {6} {7} ", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(),
                type,
                clientNodeName,
                currentSlot,
                containerType,
                POH,
                message);
            writer.Flush();
            writer.Close();



            if (this.InvokeRequired)
            {
                log1RowCallback d = new log1RowCallback(Log1);
                this.Invoke(d, new object[] { type, clientNodeName, currentSlot, containerType, POH, message });
            }
            else
            {
                logTextBox.Paste("\r\n" + DateTime.Now.ToLongTimeString() + " : " + "[" + type + "]"
               + " " + currentSlot.ToString() + " " + containerType + " " + POH + " " + message);
                logTextBox.AppendText(Environment.NewLine);
            }
        }

        delegate void log1RowCallback(string type, string clientNodeName, int currentSlot, string containerType, string POH, string message);

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
            send(sendingTextBox.Text);

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

        private void sendComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentSlot = possibleDestinations[sendComboBox.SelectedItem.ToString()];


            if (currentSlot == 1)
            {
                currentSpeed = 4;
            }
            else
            {
                currentSpeed = 3;
            }
        }

        private void ClientWindow_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            string destinationNode = nodeTextBox.Text;
            int slot;
            bool res = int.TryParse(slotTextBox.Text, out slot);
            if (res)
            {
                if (this.possibleDestinations.ContainsKey(destinationNode))
                {
                    possibleDestinations[destinationNode] = slot;
                }else
                {
                    possibleDestinations.Add(destinationNode, slot);
                }
                List<string> destinations = new List<string>(this.possibleDestinations.Keys);
                sendComboBox.Items.Clear();
                sendComboBox.Refresh();
                for (int i = 0; i < destinations.Count; i++)
                {
                    //DEBUG
                    //Log2("MAGAGEMENT INFO", destinations[i] + " : " + possibleDestinations[destinations[i]]);
                    sendComboBox.Items.Add(destinations[i]);
                }

                nodeTextBox.Clear();
                slotTextBox.Clear();
            }
            else
            {
                logTextBox.AppendText(DateTime.Now.ToLongTimeString() + " : "+"Wrong slot format");
                logTextBox.AppendText(Environment.NewLine);
                slotTextBox.Clear();
            }

            
        }
    }
}
