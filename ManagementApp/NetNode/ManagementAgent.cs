using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientNode;
using ManagementApp;

namespace NetNode
{
    //management agent:
    //  receive commands from management centre
    //  make commands to commutation field and ports
    class ManagementAgent
    {
        private TcpListener listener;
        public int port;
        private string virtualIp;

        public ManagementAgent(int port,string ip)
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
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            BinaryWriter writer = new BinaryWriter(clienttmp.GetStream());

            while (true)
            {
                string received_data = reader.ReadString();
                JSON received_object = JSON.Deserialize(received_data);
                ManagmentProtocol received_Protocol = received_object.Value.ToObject<ManagmentProtocol>();

                if (received_Protocol.State == ManagmentProtocol.WHOIS)
                {
                    Console.WriteLine("Signal from management: receivedWhoIs");
                    //send name to management
                    ManagmentProtocol protocol = new ManagmentProtocol();
                    protocol.Name = this.virtualIp;
                    String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
                    writer.Write(send_object);
                    Console.WriteLine("sending name to management: " + protocol.Name);
                }
                else if (received_Protocol.State == ManagmentProtocol.ROUTINGTABLES)
                {
                    Console.WriteLine("Signal from management: receivedroutingtable");
                    //receiving fibs
                    if (received_Protocol.RoutingTable != null)
                    {
                        foreach (var fib in received_Protocol.RoutingTable)
                        {
                            SwitchingField.addToSwitch(fib);
                        }
                    }
                }
                else if (received_Protocol.State == ManagmentProtocol.ROUTINGENTRY)
                {
                    Console.WriteLine("Signal from management: receivedroutingentry");
                    //receiving fibs
                    if (received_Protocol.RoutingEntry != null)
                    {
                        SwitchingField.addToSwitch(received_Protocol.RoutingEntry);
                    }
                }
                else
                {
                    Console.WriteLine("Signal from management: undefined protocol");
                }
            }
            //reader.Close();
        }

    }
}