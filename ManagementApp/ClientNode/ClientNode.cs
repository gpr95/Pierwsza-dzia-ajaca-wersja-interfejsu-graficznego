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
        private TcpListener listener;
        private TcpClient managmentClient;
        private static BinaryWriter writeOutput;
        private static int outputPort;
        private static bool cyclic_sending = false;
        string[] args2 = new string[3];
        //obecna przeplywnosc, mozna potem zmienic jak dostanie na VC-4 (4) całe mozliwosc
        private int currentSpeed = 1;

        public ClientNode(string[] args)
        {
            virtualIP = args[0];
            int managmentPort = Convert.ToInt32(args[1]); 
            int inputPort = Convert.ToInt32(args[2]);
            outputPort = Convert.ToInt32(args[2]);
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), inputPort);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
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
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            if (received_object.Type == typeof(STM1))
            {
                STM1 received_frame = received_object.Value.ToObject<STM1>();
                if (received_frame.VC4.POH == 0)
                {
                    Console.WriteLine("Message received: " + received_frame.VC4.message);
                }else
                {
                    foreach(VirtualContainer3 con in received_frame.VC4.C4)
                    {
                        Console.WriteLine("Message received: " + con.message);
                    }
                }
            } 
            else
            {
                Console.WriteLine("\n Odebrano uszkodzony pakiet");
            }

            reader.Close();
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
                Console.WriteLine("\n Odebrano uszkodzony pakiet");
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
                Console.WriteLine("\n 4) Quit");

                //int choice = Convert.ToInt32(Console.ReadLine());
                int choice;
                bool res = int.TryParse(Console.ReadLine(), out choice);
                if (res)
                {
                    switch (choice)
                    {
                        case 1:
                            Console.WriteLine("\nEnter node address: ");
                            string address = Console.ReadLine();                     
                            Console.WriteLine("\nEnter message: ");
                            string message = Console.ReadLine();
                            this.send(address, message);
                            break;
                        case 2:
                            Console.WriteLine("\nEnter node address: ");
                            string address2 = Console.ReadLine();
                            Console.WriteLine("\nEnter period(in seconds): ");
                            string period_tmp = Console.ReadLine();
                            int period = Convert.ToInt32(period_tmp);
                            Console.WriteLine("\nEnter message: ");
                            string message2 = Console.ReadLine();
                            this.sendPeriodically(address2, period, message2);
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
                            Environment.Exit(1);
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
                    quit = true;
                }

            }
        }

        private void send(string address,string message)
        {
            TcpClient output = new TcpClient();
            try
            {
                output.Connect(IPAddress.Parse("127.0.0.1"), outputPort);
                writeOutput = new BinaryWriter(output.GetStream());
                if (currentSpeed == 3)
                {
                    STM1 frame = new STM1();
                    VirtualContainer3 vc3 = new VirtualContainer3();
                    vc3.message = message;
                    vc3.sourceAddress = address;
                    //tu dopasowac port dla adresu, narazie domyslny jakis
                    vc3.port = 43333;
                    frame.VC4.C4.Add(vc3);
                   
                    string data = JMessage.Serialize(JMessage.FromValue(frame));
                    writeOutput.Write(data);
                    output.Close();

                }
                else
                {
                    STM1 frame = new STM1();
                    frame.VC4 = new VirtualContainer4(0);//POH
                    frame.VC4.message = message;
                    frame.VC4.sourceAddress = address;
                    //tu dopasowac port dla adresu, narazie domyslny jakis
                    frame.VC4.port = 43333;
                    string data = JMessage.Serialize(JMessage.FromValue(frame));
                    writeOutput.Write(data);
                    output.Close();
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("\nCould not connect to host.");
                
            }
           

        }
        private void sendPeriodically(string address, int period, string message)
        {


            Thread myThread = new Thread(async delegate()
            {
                string data;
                if (currentSpeed == 1)
                {
                    STM1 frame = new STM1();
                    VirtualContainer3 vc3 = new VirtualContainer3();
                    vc3.message = message;
                    vc3.sourceAddress = address;
                    //tu dopasowac port dla adresu, narazie domyslny jakis
                    vc3.port = 43333;
                    frame.VC4.C4.Add(vc3);

                    data = JMessage.Serialize(JMessage.FromValue(frame));


                }
                else
                {
                    STM1 frame = new STM1();
                    frame.VC4 = new VirtualContainer4(0);//POH
                    frame.VC4.message = message;
                    frame.VC4.sourceAddress = address;
                    //tu dopasowac port dla adresu, narazie domyslny jakis
                    frame.VC4.port = 43333;
                    data = JMessage.Serialize(JMessage.FromValue(frame));
                   
                }

                while (cyclic_sending)
                {
                    TcpClient output = new TcpClient();
                    try
                    {
                        output.Connect(IPAddress.Parse("127.0.0.1"), outputPort);
                        writeOutput = new BinaryWriter(output.GetStream());
                        writeOutput.Write(data);
                        await Task.Delay(TimeSpan.FromSeconds(period));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine("Could not connect to host.");
                        break;
                    }

                }


            });
            myThread.Start();
        }


    }
}
