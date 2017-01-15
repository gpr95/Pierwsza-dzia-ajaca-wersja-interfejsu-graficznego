using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ClientWindow;
using ManagementApp;
using System.Net;

namespace NetNode
{
    class ControlAgent
    {
        //TODO receiving from upper CC information to LRM to reserve resources
        //TODO receive from upper CC information to store fib to fibtable
        //TODO send confirmation to upper CC
        //TODO send info from LRM to RC
        private TcpListener listener;
        public int port;
        private string virtualIp;

        BinaryReader reader;
        BinaryWriter writer;

        public ControlAgent(int port, string ip)
        {
            this.port = port;
            this.virtualIp = ip;
            //listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        private void Listen()
        {
            TcpClient clienttmp = new TcpClient("127.0.0.1", this.port);
            reader = new BinaryReader(clienttmp.GetStream());
            writer = new BinaryWriter(clienttmp.GetStream());
            try
            {
                while (true)
                {
                    string received_data = reader.ReadString();
                    JSON received_object = JSON.Deserialize(received_data);
                    //ControlProtocol received_Protocol = received_object.Value.ToObject<ControlProtocol>();

                    //if (received_Protocol.State == ControlProtocol.WHOIS)
                    //{
                        //Console.WriteLine("Control Signal: receivedWhoIs");
                        //send name to management
                       //ControlProtocol protocol = new ControlProtocol();
                        //protocol.Name = this.virtualIp;
                        //String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
                        //writer.Write(send_object);
                        //Console.WriteLine("sending name to management: " + protocol.Name);
                    //}
                    //else
                    //{
                       // Console.WriteLine("Control Signal: undefined protocol");
                    //}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError sending signal: " + e.Message);
                Thread.Sleep(2000);
                Environment.Exit(1);
            }
        }

        public static void sendTopology(string from, int port, string to)
        {
            //TODO send to RC e.g. NN0 connected on port 2 with NN1
            Console.WriteLine("sending topology to RC: " + from+" "+port+" "+to);

            //ControlProtocol protocol = new ControlProtocol();
            //protocol.topology = this.virtualIp;
            //String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            //writer.Write(send_object);
        }


        public static void sendDeleted(string from, int port, string to)
        {
            //TODO send to RC that row e.g. NN0 connected on port 2 with NN1 is deleted
            Console.WriteLine("sending to RC info about deletion: " + from + " " + port + " " + to);
        }

        public static void sendConfirmation(int port, int no_vc3)
        {
            //TODO send to CC confirmation of resource reservation and vc3 number
        }
    }
}
