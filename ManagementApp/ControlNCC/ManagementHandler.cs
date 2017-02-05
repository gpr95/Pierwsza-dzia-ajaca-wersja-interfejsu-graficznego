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
                          
                            control.consoleWriter("[NCC <- Management] Soft pernament from "+ management_packet.NodeStart+" to "+management_packet.NodeEnd);
                            int RequestID = control.generateRequestID();
                            Address address = new Address(management_packet.NodeStart);
                            int cpccID = address.type + address.domain + address.subnet + address.space;
                            control.addCpccRequest(RequestID, cpccID);

                            ControlPacket packetToCPCC = new ControlPacket(ControlInterface.CALL_INDICATION_CPCC, ControlPacket.IN_PROGRESS, management_packet.Speed, management_packet.NodeEnd, management_packet.NodeStart, RequestID);
                            packetToCPCC.Vc11 = 1;
                            packetToCPCC.Vc12 = 1;
                            packetToCPCC.Vc13 = 1;
                            ControlConnectionService cpccService = control.getCpccServiceByAddr(management_packet.NodeEnd);
                            cpccService.send(packetToCPCC);
                            control.consoleWriter("[NCC -> CPCC] Send Call Indication");

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
        public void send(int reqid, string address)
        {
            ManagmentProtocol protocol = new ManagmentProtocol();
            protocol.State = ManagmentProtocol.SOFTPERNAMENT;
            string id = reqid.ToString();
            protocol.Name = id;
            protocol.NodeEnd = address; 
            String send_object = JSON.Serialize(JSON.FromValue(protocol));
            writer.Write(send_object);
        }
    }
}
