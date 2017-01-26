using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientWindow;
using Management;
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
            try
            {
                while (true)
                {
                    string received_data = reader.ReadString();
                    JSON received_object = JSON.Deserialize(received_data);
                    ManagmentProtocol received_Protocol = received_object.Value.ToObject<ManagmentProtocol>();

                    if (received_Protocol.State == ManagmentProtocol.WHOIS)
                    {
                        NetNode.log("Signal from management: receivedWhoIs", ConsoleColor.Blue);
                        //send name to management
                        ManagmentProtocol protocol = new ManagmentProtocol();
                        protocol.Name = this.virtualIp;
                        String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
                        writer.Write(send_object);
                        NetNode.log("sending name to management: " + protocol.Name, ConsoleColor.Blue);
                    }
                    else if (received_Protocol.State == ManagmentProtocol.ROUTINGTABLES)
                    {
                        NetNode.log("Signal from management: receivedroutingtable", ConsoleColor.Blue);
                        //receiving fibs
                        if (received_Protocol.RoutingTable != null)
                        {
                            foreach (var fib in received_Protocol.RoutingTable)
                            {
                                SwitchingField.addToSwitch(fib);
                                //adding fib for two-way communication
                                SwitchingField.addToSwitch(new FIB(fib.oport, fib.out_cont, fib.iport, fib.in_cont));
                            }
                        }
                    }
                    else if (received_Protocol.State == ManagmentProtocol.ROUTINGENTRY)
                    {
                        NetNode.log("Signal from management: receivedroutingentry", ConsoleColor.Blue);
                        //receiving fibs
                        if (received_Protocol.RoutingEntry != null)
                        {
                            SwitchingField.addToSwitch(received_Protocol.RoutingEntry);
                            //adding fib for two-way communication
                            SwitchingField.addToSwitch(new FIB(received_Protocol.RoutingEntry.oport, received_Protocol.RoutingEntry.out_cont,
                                received_Protocol.RoutingEntry.iport, received_Protocol.RoutingEntry.in_cont));
                        }
                    }
                    else if (received_Protocol.State == ManagmentProtocol.INTERFACEINFORMATION)
                    {
                        NetNode.log("Signal from management: iterfaceinformation", ConsoleColor.Blue);
                        //send dictionary from LRM to management
                        ManagmentProtocol protocol = new ManagmentProtocol();
                        protocol.State = ManagmentProtocol.INTERFACEINFORMATION;
                        protocol.Interfaces = LRM.getConn();
                        String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
                        writer.Write(send_object);
                    }
                    else if (received_Protocol.State == ManagmentProtocol.GETTABLE)
                    {
                        NetNode.log("Signal from management: getTable", ConsoleColor.Blue);
                        //send dictionary from LRM to management
                        ManagmentProtocol protocol = new ManagmentProtocol();
                        protocol.State = ManagmentProtocol.GETTABLE;
                        protocol.RoutingTable = SwitchingField.fib;
                        String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
                        writer.Write(send_object);
                    }
                    else
                    {
                        NetNode.log("Signal from management: undefined protocol", ConsoleColor.Red);
                    }
                }
            }
            catch (Exception e)
            {
                NetNode.log("\nError sending signal: " + e.Message, ConsoleColor.Red);
                Thread.Sleep(2000);
                Environment.Exit(1);
            }
        }

    }
}