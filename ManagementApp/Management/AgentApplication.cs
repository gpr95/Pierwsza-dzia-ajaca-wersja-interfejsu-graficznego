using ManagementApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Management
{
    class AgentApplication
    {
        private int port;
        private Thread thread;
        private TcpClient client;
        private ManagementPlane management;

        public AgentApplication(int port, ManagementPlane management)
        {
            this.management = management;
            this.port = port;
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        private void Listen()
        {
            try
            {
                client = new TcpClient("127.0.0.1", this.port);
                BinaryReader reader = new BinaryReader(client.GetStream());
                BinaryWriter writer = new BinaryWriter(client.GetStream());
                management.log("Connection successfully established with Window application.", ConsoleColor.Green);
                while (true)
                {
                    string received_data = reader.ReadString();
                    JSON received_object = JSON.Deserialize(received_data);
                    ApplicationProtocol received_Protocol = received_object.Value.ToObject<ApplicationProtocol>();

                    switch(received_Protocol.State)
                    {
                        case ApplicationProtocol.CONNECTIONTONCC:
                            Console.WriteLine("Option A");
                            break;
                        case ApplicationProtocol.KILL:
                            Environment.Exit(1);
                            break;
                        case ApplicationProtocol.TOOTHERNCC:
                            UserInterface.log("Recived ConnectToOtherDomains", ConsoleColor.Yellow);
                            Thread.Sleep(50);
                            management.connectToOtherNcc(received_Protocol.ConnectionToOtherNcc);
                            break;
                        default:
                            break;
                    }
                    //if (received_Protocol.State == ApplicationProtocol.CONNECTIONTONCC)
                    //    Console.WriteLine("Option A");
                    //else if (received_Protocol.State == ApplicationProtocol.KILL)
                        
                }
            }
            catch (SocketException e)
            {
                management.log("\nError: " + e.Message, ConsoleColor.Red);
                //Thread.Sleep(2000);
                //Environment.Exit(1);
            }
            catch (IOException e)
            {
                management.log("\nError: " + e.Message, ConsoleColor.Red);
                Thread.Sleep(1000);
                Environment.Exit(1);
            }
        }
    }
}
