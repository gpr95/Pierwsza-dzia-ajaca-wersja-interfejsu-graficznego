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
        private static String Id = null;
        private TcpListener listener;
        private Thread thread;
        private BinaryWriter writeOutput;
        public ClientNode(string[] args)
        {
            Id = args[0];
          
                int inputPort = Convert.ToInt32(args[1]);
                int outputPort = Convert.ToInt32(args[2]);

                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), inputPort);
                thread = new Thread(new ThreadStart(Listen));
                thread.Start();

            TcpClient output = new TcpClient();
            output.Connect(IPAddress.Parse("127.0.0.1"), outputPort);
            writeOutput = new BinaryWriter(output.GetStream());

            ConsoleInterface();
        }
        private void Listen()
        {
            listener.Start();
          
            while (true)
            {
             
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                clientThread.Start();
            }
        }

         private static void ListenThread(Object client)
        {
                BinaryReader reader = new BinaryReader(((TcpClient)client).GetStream());
                Console.WriteLine("Message received: " + reader.ReadString());
                reader.Close();
        }

        private void ConsoleInterface()
        {
            Boolean quit = false;

            while (!quit)
            {

                Console.WriteLine("\n---- Client Node " + Id + " ----");
                Console.WriteLine("\n MENU: ");
                Console.WriteLine("\n 1) Send message");
                Console.WriteLine("\n 2) Quit");

                //int choice = Convert.ToInt32(Console.ReadLine());
                int choice;
                bool res = int.TryParse(Console.ReadLine(), out choice);
                if (res)
                {
                    switch (choice)
                    {
                        case 1:
                            Console.WriteLine("\n Enter node address: ");
                            String address = Console.ReadLine();
                            Console.WriteLine("\n Enter message: ");
                            String message = Console.ReadLine();
                            writeOutput.Write(message);
                            break;

                        case 2:
                            Environment.Exit(0);
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
    }
}
