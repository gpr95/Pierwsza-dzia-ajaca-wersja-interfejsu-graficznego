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
        private string virtualIP;
        private TcpListener listenerInput;
        private TcpListener listenerOutput;
        private TcpClient managmentClient;
        private static BinaryWriter writeOutput;
        private static int outputPort;
        private static bool cyclic_sending = false;
        string[] args2 = new string[3];
        //obecna przeplywnosc, mozna potem zmienic jak dostanie na VC-4 (4) całe mozliwosc
        private int currentSpeed = 4;
        private static string name;
        private static string path;

        public ClientNode(string[] args)
        {
            virtualIP = args[0];
            //int managmentPort = Convert.ToInt32(args[1]); 
            int inputPort = Convert.ToInt32(args[1]);
            //DEBUG normlanie args[2]
            outputPort = Convert.ToInt32(args[2]);
            name = args[3];
            string fileName = args[3] + "_" + DateTime.Now.ToLongTimeString().Replace(":", "_") + "_" + DateTime.Now.ToLongDateString().Replace(" ", "_");
            // path = @"D:\TSSTRepo\ManagementApp\ClientNode\logs\"+fileName+".txt";
            path = Path.Combine(Environment.CurrentDirectory, @"logs\", fileName + ".txt");
            System.IO.Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, @"logs\"));
            listenerInput = new TcpListener(IPAddress.Parse("127.0.0.1"), inputPort);
            Thread threadIn = new Thread(new ThreadStart(ListenIn));
            threadIn.Start();


            listenerOutput = new TcpListener(IPAddress.Parse("127.0.0.1"), outputPort);
            Thread threadOut = new Thread(new ThreadStart(ListenOut));
            threadOut.Start();

            ConsoleInterface();
        }
        private void ListenIn()
        {
            listenerInput.Start();

            while (true)
            {
                TcpClient client = listenerInput.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenInThread));
                clientThread.Start(client);
            }
        }

        private void ListenOut()
        {
            listenerOutput.Start();

            while (true)
            {
                TcpClient client = listenerOutput.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenOutThread));
                clientThread.Start(client);
            }
        }

        private static void ListenInThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            if (received_object.Type == typeof(Signal))
            {
                Signal received_signal = received_object.Value.ToObject<Signal>();
                STM1 received_frame = received_signal.stm1;
                if (received_frame.vc4 != null)
                {
                    Console.WriteLine("Message received: " + received_frame.vc4.C4);
                    Log1("IN", name, received_signal.time.ToString(), "VC-4", received_frame.vc4.POH.ToString(), received_frame.vc4.C4);
                }

                else
                {
                    foreach (VirtualContainer3 vc3 in received_frame.vc3List)
                    {
                        Console.WriteLine("Message received: " + vc3.C3);
                        Log1("IN", name, received_signal.time.ToString(), "VC-3", vc3.POH.ToString(), vc3.C3);
                    }
                }
            }
            else
            {
                Console.WriteLine("\n Received unknown data type");
                Log2("ERR", "Received unknown data type");
            }

            reader.Close();
        }

        private static void ListenOutThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            writeOutput = new BinaryWriter(clienttmp.GetStream());
            Console.WriteLine("\n MAKING CLOUD OUTLIST CONNECTION: PORT:" + ((IPEndPoint)clienttmp.Client.RemoteEndPoint).Port);
        }

        private void initManagmentConnection(int sessionPort)
        {
            managmentClient.Connect("127.0.0.1", sessionPort);
            BinaryReader reader = new BinaryReader(managmentClient.GetStream());
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            if (received_object.Type == typeof(STM1))
            {
                STM1 received_frame = received_object.Value.ToObject<STM1>();
                //TO DO odbierz od marka tablice adresow i ich portow i wpisz u siebie lokalnie
            }
            else
            {
                Console.WriteLine("\n Unknown data type");
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

                            Console.WriteLine("\nEnter message: ");
                            string message = Console.ReadLine();
                            this.send(message);
                            break;
                        case 2:
                            Console.WriteLine("\nEnter period(in seconds): ");
                            string period_tmp = Console.ReadLine();
                            int period = Convert.ToInt32(period_tmp);
                            Console.WriteLine("\nEnter message: ");
                            string message2 = Console.ReadLine();
                            this.sendPeriodically(period, message2);
                            cyclic_sending = true;

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
                            Console.WriteLine("\n Wrong option");
                            break;


                    }
                }
                else
                {
                    Console.WriteLine("Wrong format");
                    ConsoleInterface();
                }

            }
            Environment.Exit(1);
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
                    //od zarządzania znam pozycje gdzie wpisac kontener jesli chce go wyslać do klienta jakiegoś tam
                    //po stronie klienta moja tablica ma jeden element, ale net node moze juz wywolac z 2 lub 3 na raz
                    VirtualContainer3[] vc3List = new VirtualContainer3[0];
                    vc3List[0] = vc3;
                    int[] pos = new int[0];
                    // z zarzadania wstawiam w pozycje 1 w stm
                    pos[0] = 1;
                    STM1 frame = new STM1(vc3List, pos);
                    //port ktory wiem z zarzadzania
                    int virtualPort = 1;
                    //SYGNAL
                    Signal signal = new Signal(getTime(), virtualPort, frame);
                    string data = JMessage.Serialize(JMessage.FromValue(signal));


                    writeOutput.Write(data);
                    Log1("OUT", name, signal.time.ToString(), "VC-3", frame.vc3List[0].POH.ToString(), frame.vc3List[0].C3);
                    //       output.Close();

                }
                else
                {
                    VirtualContainer4 vc4 = new VirtualContainer4(adaptation(), message);
                    //tutaj wiem ze moge wykorzystac wieksza przepływnosc, wiec pakuje vc4 do stm i wysylam
                    STM1 frame = new STM1(vc4);
                    //port ktory wiem z zarzadzania
                    int virtualPort = 1;
                    //SYGNAL
                    Signal signal = new Signal(getTime(), virtualPort, frame);
                    string data = JMessage.Serialize(JMessage.FromValue(signal));

                    writeOutput.Write(data);
                    //output.Close();
                    Log1("OUT", name, signal.time.ToString(), "VC-4", frame.vc4.POH.ToString(), frame.vc4.C4);
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
                    //od zarządzania znam pozycje gdzie wpisac kontener jesli chce go wyslać do klienta jakiegoś tam
                    //po stronie klienta moja tablica ma jeden element, ale net node moze juz wywolac z 2 lub 3 na raz
                    VirtualContainer3[] vc3List = new VirtualContainer3[0];
                    vc3List[0] = vc3;
                    int[] pos = new int[0];
                    // z zarzadania wstawiam w pozycje 1 w stm
                    pos[0] = 1;
                    frame = new STM1(vc3List, pos);
                    //port ktory wiem z zarzadzania
                    int virtualPort = 4000;
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
                    int virtualPort = 4000;
                    //SYGNAL
                    signal = new Signal(getTime(), virtualPort, frame);
                    data = JMessage.Serialize(JMessage.FromValue(signal));

                }

                while (cyclic_sending)
                {
                    TcpClient output = new TcpClient();
                    try
                    {
                        output.Connect(IPAddress.Parse("127.0.0.1"), outputPort);
                        writeOutput = new BinaryWriter(output.GetStream());
                        writeOutput.Write(data);
                        if (isVc3)
                            Log1("OUT", name, signal.time.ToString(), "VC-3", frame.vc3List[0].POH.ToString(), frame.vc3List[0].C3);
                        else
                            Log1("OUT", name, signal.time.ToString(), "VC-4", frame.vc4.POH.ToString(), frame.vc4.C4);
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
