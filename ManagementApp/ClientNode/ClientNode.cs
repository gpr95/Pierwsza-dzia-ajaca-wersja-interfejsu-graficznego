using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientNode
{
    class ClientNode
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
        private Dictionary<String, int> possibleDestinations = new Dictionary<string,int>();
        private int virtualPort;

        public ClientNode(string[] args)
        {
            virtualIP = args[0];
            //int managmentPort = Convert.ToInt32(args[1]); 
            int cloudPort = Convert.ToInt32(args[1]);

            int managementPort = Convert.ToInt32(args[2]);
           
            string fileName = virtualIP + "_" + DateTime.Now.ToLongTimeString().Replace(":", "_") + "_" + DateTime.Now.ToLongDateString().Replace(" ", "_");
            // path = @"D:\TSSTRepo\ManagementApp\ClientNode\logs\"+fileName+".txt";
            path = System.IO.Directory.GetCurrentDirectory() + @"\logs\" + fileName + ".txt";
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), cloudPort);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
            Console.WriteLine(managementPort);
            Thread managementThreadad = new Thread(new ParameterizedThreadStart(initManagmentConnection));
            managementThreadad.Start(managementPort);
            ConsoleInterface();
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


        private static void ListenThread(Object client)
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
                        Console.WriteLine("Message received: " + received_frame.vc4.C4);
                        Log1("IN", virtualIP, received_signal.time.ToString(), "VC-4", received_frame.vc4.POH.ToString(), received_frame.vc4.C4);
                    }

                    else
                    {
                        foreach (int key in received_frame.vc3List.Keys)
                        {
                            Console.WriteLine("Message received: " + received_frame.vc3List[key].C3);
                            Log1("IN", virtualIP, received_signal.time.ToString(), "VC-3", received_frame.vc3List[key].POH.ToString(), received_frame.vc3List[key].C3);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\n Received unknown data type");
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
                managmentClient = new TcpClient("127.0.0.1", (int) managementPort);
                BinaryReader reader = new BinaryReader(managmentClient.GetStream());
                BinaryWriter writer = new BinaryWriter(managmentClient.GetStream());
                while(true)
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

                        }

                    }
                    else
                    {
                        Console.WriteLine("\n Unknown data type");
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect on management interface");
                //debug
                // Console.WriteLine(e.Message);
                Log2("ERR", "Could not connect on management interface");
                Thread.Sleep(2000);
                Environment.Exit(1);
                
            }
        }

        private void ConsoleInterface()
        {
            Boolean quit = false;

            while (!quit)
            {

                Console.WriteLine("\n---- Client Node " + virtualIP + " ----");
                Console.WriteLine("\n MENU: ");
                Console.WriteLine("\n 1) Send message");
                Console.WriteLine("\n 2) Send message periodically");
                Console.WriteLine("\n 3) Stop sending");
                Console.WriteLine("\n 4) Check logs");
                Console.WriteLine("\n 5) Quit");

                int choice;
                bool res = int.TryParse(Console.ReadLine(), out choice);
                if (res)
                {
                    switch (choice)
                    {
                        case 1:
                            prepareDestinations(1);
                            break;
                        case 2:
                            prepareDestinations(2);
                            break;
                        case 3:
                            if (cyclic_sending == true)
                            {
                                cyclic_sending = false;
                                Console.WriteLine("\nSending stopped");
                            }
                            else
                            {
                                Console.WriteLine("\nNothing to stop");
                            }
                            break;
                        case 4:
                            DumpLog();
                            break;
                        case 5:
                            quit = true;
                            break;
                        default:
                            Console.WriteLine("\nWrong option");
                            break;


                    }
                }
                else
                {
                    Console.WriteLine("\nWrong format");
                    ConsoleInterface();
                }

            }
            Environment.Exit(1);
        }

        private void prepareDestinations(int type)
        {
            Console.WriteLine("Choose destination");
            List<string> destinations = new List<string>(this.possibleDestinations.Keys);
            for (int i=0; i < destinations.Count; i++)
            {
                Console.WriteLine("{0}) {1}", i, destinations[i]);
            }
             int choice;
                bool res = int.TryParse(Console.ReadLine(), out choice);
                if (res)
                {
                    if (choice < destinations.Count)
                    {
                        currentSlot = possibleDestinations[destinations[choice]];
                        if (currentSlot == 1)
                        {
                            currentSpeed = 4;
                        }
                        else
                        {
                            currentSpeed = 3;
                        }
                        if (type == 1)
                        {
                            Console.WriteLine("\nEnter message: ");
                            string message = Console.ReadLine();
                            this.send(message);
                        }
                        else
                        {
                            Console.WriteLine("\nEnter period(in seconds): ");
                            string period_tmp = Console.ReadLine();
                            int period = Convert.ToInt32(period_tmp);
                            Console.WriteLine("\nEnter message: ");
                            string message2 = Console.ReadLine();
                            this.sendPeriodically(period, message2);
                            cyclic_sending = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Wrong option");
                        ConsoleInterface();
                    }
                }
                else
                {
                    Console.WriteLine("Wrong format");
                    ConsoleInterface();
                }

        }
        private void send(string message)
        {
            //  TcpClient output = new TcpClient();
            try
            {
                // output.Connect(IPAddress.Parse("127.0.0.1"), outputPort);
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
                    Log1("OUT", virtualIP, signal.time.ToString(), "VC-3", frame.vc3List[currentSlot].POH.ToString(), frame.vc3List[currentSlot].C3);
                    //       output.Close();

                }
                else
                {
                    VirtualContainer4 vc4 = new VirtualContainer4(adaptation(), message);
                    //tutaj wiem ze moge wykorzystac wieksza przepływnosc, wiec pakuje vc4 do stm i wysylam
                    STM1 frame = new STM1(vc4);
                    //port ktory wiem z zarzadzania
                    //int virtualPort = 1;
                    //SYGNAL
                    Signal signal = new Signal(getTime(), virtualPort, frame);
                    string data = JMessage.Serialize(JMessage.FromValue(signal));

                    writer.Write(data);
                    //output.Close();
                    Log1("OUT", virtualIP, signal.time.ToString(), "VC-4", frame.vc4.POH.ToString(), frame.vc4.C4);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("\nError sending signal: " + e.Message);
                Log2("ERR", "\nError sending signal: " + e.Message);
            }


        }
        //to add  POH
        //private byte[] adaptation()
        //{
        //    Random rnd = new Random();
        //    byte[] POH = new Byte[8];
        //    rnd.NextBytes(POH);
        //    //debug
        //    //Console.WriteLine("The Random bytes are: ");
        //    //for (int i = 0; i <= POH.GetUpperBound(0); i++)
        //    //    Console.WriteLine("{0}: {1}", i, POH[i]);
        //    return POH;
        //}
        private int adaptation()
        {
            Random r = new Random();
            int POH = r.Next(0, 50000);
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
                    //port ktory wiem z zarzadzania
                    //int virtualPort = 1;
                    //SYGNAL
                    signal = new Signal(getTime(), virtualPort, frame);
                    data = JMessage.Serialize(JMessage.FromValue(signal));
                    isVc3 = true;

                }
                else
                {
                    VirtualContainer4 vc4 = new VirtualContainer4(adaptation(), message);
                    //tutaj wiem ze moge wykorzystac wieksza przepływnosc, wiec pakuje vc4 do stm i wysylam
                    frame = new STM1(vc4);
                    //port ktory wiem z zarzadzania
                    //int virtualPort = 1;
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
                            Log1("OUT", virtualIP, signal.time.ToString(), "VC-3", frame.vc3List[currentSlot].POH.ToString(), frame.vc3List[currentSlot].C3);
                        else
                            Log1("OUT", virtualIP, signal.time.ToString(), "VC-4", frame.vc4.POH.ToString(), frame.vc4.C4);
                        await Task.Delay(TimeSpan.FromSeconds(period));
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine("\nError sending signal: " + e.Message);
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

        public void DumpLog()
        {

            StreamReader reader = File.OpenText(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
            reader.Close();
        }


    }
}
