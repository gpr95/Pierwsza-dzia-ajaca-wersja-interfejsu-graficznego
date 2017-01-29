using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Management;
using ManagementApp;
using ClientNode;
using ControlCCRC.Protocols;

namespace ControlNCC
{
    class ManagementHandler
    {
        private int port;
        private Thread thread;
        private TcpClient client;
        private NetworkCallControl control;
        private BinaryWriter writer;
        private Random r;

        public ManagementHandler(int port, NetworkCallControl control)
        {
            
            this.control = control;
            this.port = port;
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
            r = new Random();
        }

        private void Listen()
        {
            try
            {
                client = new TcpClient("127.0.0.1", this.port);
                BinaryReader reader = new BinaryReader(client.GetStream());
                writer = new BinaryWriter(client.GetStream());
                while (true)
                {
                    string received_data = reader.ReadString();
                    JSON received_object = JSON.Deserialize(received_data);
                    Management.ManagmentProtocol received_Protocol = received_object.Value.ToObject<Management.ManagmentProtocol>();
                    if (received_object.Type == typeof(Management.ManagmentProtocol))
                    {
                        Management.ManagmentProtocol management_packet = received_object.Value.ToObject<Management.ManagmentProtocol>();
                        if (management_packet.State == Management.ManagmentProtocol.TOOTHERNCC)
                        {
                            foreach (int port in management_packet.ConnectionToOtherNcc)
                            {
                                TcpClient connection = new TcpClient("127.0.0.1", port);
                                ControlConnectionService service = new ControlConnectionService(connection, control);
                                Thread.Sleep(500);
                                ControlPacket packetToNCC = new ControlPacket(ControlInterface.NETWORK_CALL_COORDINATION_IN, ControlPacket.IN_PROGRESS, 0, "", "", control.domainNumber);
                                service.send(packetToNCC);
                            }
                        }
                        else if (management_packet.State == Management.ManagmentProtocol.SOFTPERNAMENT)
                        {
                            CCtoNCCSingallingMessage packet = new CCtoNCCSingallingMessage();
                            packet.State = CCtoNCCSingallingMessage.NCC_SET_CONNECTION;
                            packet.NodeFrom = management_packet.NodeStart;
                            packet.NodeTo = management_packet.NodeEnd;
                            packet.Vc11 = 1;
                            packet.Vc12 = 1;
                            packet.Vc13 = 1;
                            packet.Rate = management_packet.Speed;
                            packet.RequestID = r.Next(10000, 40000);
                            control.addService(packet.RequestID, control.getCCService());
                            control.getCCService().sendCCRequest(packet);
                        }
                    }
                }
            }
            catch (SocketException e)
            {

            }
            catch (IOException e)
            {
                Thread.Sleep(1000);
                Environment.Exit(1);
            }
        }
        public void send(string address, List<int> slots)
        {

        }
    }
}
