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

namespace ClientNode
{
    public partial class ClientWindow : Form
    {
        private static string virtualIP;
        private TcpListener listener;
        private TcpClient managmentClient;
        private static BinaryWriter writer;
        private static bool cyclic_sending = false;
        string[] args2 = new string[3];
        //obecna przeplywnosc, mozna potem zmienic jak dostanie na VC-4 (4) całe mozliwosc
        private int currentSpeed = 3;
        private int currentSlot;
        private static string path;
        private Dictionary<String, int> possibleDestinations = new Dictionary<string, int>();
        private int virtualPort;
        private int managementPort;

        public ClientWindow(string[] args)
        {
            virtualIP = args[0];
            //int managmentPort = Convert.ToInt32(args[1]); 
            int cloudPort = Convert.ToInt32(args[1]);

            managementPort = Convert.ToInt32(args[2]);

            string fileName = virtualIP + "_" + DateTime.Now.ToLongTimeString().Replace(":", "_") + "_" + DateTime.Now.ToLongDateString().Replace(" ", "_");
            // path = @"D:\TSSTRepo\ManagementApp\ClientNode\logs\"+fileName+".txt";
            path = System.IO.Directory.GetCurrentDirectory() + @"\logs\" + fileName + ".txt";
            Log2("", "START LOG");
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), cloudPort);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
           // Console.WriteLine(managementPort);
            Thread managementThreadad = new Thread(new ParameterizedThreadStart(initManagmentConnection));
            managementThreadad.Start(managementPort);
            InitializeComponent();
            this.Text=virtualIP;

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


        private  void ListenThread(Object client)
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
                    if (received_frame.vc4 != null)
                    {
           
                        receivedTextBox.AppendText(received_frame.vc4.C4);
                        receivedTextBox.AppendText(Environment.NewLine);
                        Log1("IN", virtualIP, received_signal.time.ToString(), "VC-4", received_frame.vc4.POH.ToString(), received_frame.vc4.C4);
                    }

                    else
                    {
                        foreach (KeyValuePair<int, VirtualContainer3> v in received_frame.vc3List)
                        {
                            
                            receivedTextBox.AppendText(v.Value.C3);
                            receivedTextBox.AppendText(Environment.NewLine);
                            Log1("IN", virtualIP, received_signal.time.ToString(), "VC-3", v.Value.POH.ToString(), v.Value.C3);
                        }
                    }
                }
                else
                {

                    Log2("ERR", "Received unknown data type");
                }
            }

            // reader.Close();
        }

        private void initManagmentConnection(Object managementPort)
        {
            try
            {
                //managmentClient.Connect("127.0.0.1", managementPort);
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
                            logTextBox.AppendText("Virtual Port: " + virtualPort);
                            List<string> destinations = new List<string>(this.possibleDestinations.Keys);
                            sendComboBox.Items.Clear();
                            for (int i = 0; i < destinations.Count; i++)
                            {
                                sendComboBox.Items.Add(destinations[i]);
                            }
                        }

                    }
                    else
                    {
                        //Console.WriteLine("\n Unknown data type");
                    }
                }

            }
            catch (Exception e)
            {
                logTextBox.AppendText("Could not connect on management interface");
                logTextBox.AppendText(Environment.NewLine);
                //debug
                // Console.WriteLine(e.Message);
                Log2("ERR", "Could not connect on management interface");
                Thread.Sleep(4000);
                Environment.Exit(1);

            }
        }

        private void send(string message)
        {
            try
            {
                if (currentSpeed == 3)
                {

                    VirtualContainer3 vc3 = new VirtualContainer3(adaptation(), message);
                    Dictionary<int, VirtualContainer3> vc3List = new Dictionary<int, VirtualContainer3>();
                    vc3List.Add(currentSlot, vc3);
                    STM1 frame = new STM1(vc3List);
                    //SYGNAL
                    Signal signal = new Signal(getTime(), virtualPort, frame);
                    string data = JMessage.Serialize(JMessage.FromValue(signal));
                    writer.Write(data);
                    foreach (KeyValuePair<int, VirtualContainer3> v in frame.vc3List)
                    {
                        Log1("OUT", virtualIP, signal.time.ToString(), "VC-3", v.Value.POH.ToString(), v.Value.C3);
                    }
                }
                else
                {
                    VirtualContainer4 vc4 = new VirtualContainer4(adaptation(), message);
                    STM1 frame = new STM1(vc4);
                    Signal signal = new Signal(getTime(), virtualPort, frame);
                    string data = JMessage.Serialize(JMessage.FromValue(signal));
                    writer.Write(data);
                    Log1("OUT", virtualIP, signal.time.ToString(), "VC-4", frame.vc4.POH.ToString(), frame.vc4.C4);
                }
                sendingTextBox.Clear();
            }
            catch (Exception e)
            {
                logTextBox.AppendText("Error sending signal");
                logTextBox.AppendText(Environment.NewLine);
                Log2("ERR", "\nError sending signal: " + e.Message);
            }


        }

        private int adaptation()
        {
            Random r = new Random();
            int POH = r.Next(30000, 50000);
            return POH;
        }

        //losowy czas sygnalu z przedzialu od 0 do 125 mikro sekund
        private int getTime()
        {
            Random r = new Random();
            int time = r.Next(10, 125);
            return time;
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
                    signal = new Signal(getTime(), virtualPort, frame);
                    data = JMessage.Serialize(JMessage.FromValue(signal));
                    isVc3 = true;

                }
                else
                {
                    VirtualContainer4 vc4 = new VirtualContainer4(adaptation(), message);
                    frame = new STM1(vc4);
                    //SYGNAL
                    signal = new Signal(getTime(), virtualPort, frame);
                    data = JMessage.Serialize(JMessage.FromValue(signal));

                }

                while (cyclic_sending)
                {

                    try
                    {

                        writer.Write(data);
                        if (isVc3)
                            foreach (KeyValuePair<int, VirtualContainer3> v in frame.vc3List)
                            {
                                Log1("OUT", virtualIP, signal.time.ToString(), "VC-3", v.Value.POH.ToString(), v.Value.C3);
                            }
                        else
                            Log1("OUT", virtualIP, signal.time.ToString(), "VC-4", frame.vc4.POH.ToString(), frame.vc4.C4);
                        await Task.Delay(TimeSpan.FromSeconds(period));
                    }
                    catch (Exception e)
                    {

                        logTextBox.AppendText("Error sending signal");
                        logTextBox.AppendText(Environment.NewLine);
                        Log2("ERR", "\nError sending signal: " + e.Message);
                        break;
                    }

                }


            });
            myThread.Start();
        }

        public static void Log1(string type, string clientNodeName, string signalDuration, string containerType, string POH, string message)
        {

            StreamWriter writer = File.AppendText(path);
            writer.WriteLine("\r\n{0} {1} : {2} {3} {4} {5} {6} {7}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(),
                type,
                clientNodeName,
                signalDuration,
                containerType,
                POH,
                message);
            writer.Flush();
            writer.Close();
        }

        public static void Log2(string type, string message)
        {

            StreamWriter writer = File.AppendText(path);
            writer.WriteLine("\r\n{0} {1} : {2} {3}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(),
                type,
                message);
            writer.Flush();
            writer.Close();
        }

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
                logTextBox.AppendText("Wrong period format");
                logTextBox.AppendText(Environment.NewLine);
                timeTextBox.Clear();
            }


        }

        private void stopSendingBtn_Click(object sender, EventArgs e)
        {

        }

        private void sendComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentSlot = possibleDestinations[sendComboBox.SelectedItem.ToString()];
            logTextBox.AppendText("Current slot: " + currentSlot);
            receivedTextBox.AppendText(Environment.NewLine);

            if (currentSlot == 1)
            {
                currentSpeed = 4;
            }
            else
            {
                currentSpeed = 3;
            }
        }

      
    }
}
