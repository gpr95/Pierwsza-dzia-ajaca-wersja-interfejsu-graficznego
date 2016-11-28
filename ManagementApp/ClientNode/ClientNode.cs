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
            if (received_object.Type == typeof(Packet))
            {
                Packet received_packet = received_object.Value.ToObject<Packet>();
                Console.WriteLine("Message received: " + received_packet.message);
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
            if (received_object.Type == typeof(Packet))
            {
                Packet received_packet = received_object.Value.ToObject<Packet>();
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
                            TcpClient output = new TcpClient();
                            try
                            {
                                output.Connect(IPAddress.Parse("127.0.0.1"), outputPort);
                                writeOutput = new BinaryWriter(output.GetStream());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                Console.WriteLine("\nCould not connect to host.");
                                break;
                            }
                            Console.WriteLine("\nEnter message: ");
                            string message = Console.ReadLine();
                            Packet packet = new Packet();
                            packet.sourceAddress = address;
                            packet.message = message;
                            string data = JMessage.Serialize(JMessage.FromValue(packet));
                            writeOutput.Write(data);
                            output.Close();
                            break;
                        case 2:
                            Console.WriteLine("\nEnter node address: ");
                            string address2 = Console.ReadLine();
                            Console.WriteLine("\nEnter period(in seconds): ");
                            string period_tmp = Console.ReadLine();
                            int period = Convert.ToInt32(period_tmp);
                            Console.WriteLine("\nEnter message: ");
                            string message2 = Console.ReadLine();
                            this.SendPeriodically(address2, period, message2);
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


        private void SendPeriodically(string address, int period, string message)
        {


            Thread myThread = new Thread(async delegate()
            {
                Packet packet = new Packet();
                packet.sourceAddress = address;
                packet.message = message;
                string data = JMessage.Serialize(JMessage.FromValue(packet));

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
