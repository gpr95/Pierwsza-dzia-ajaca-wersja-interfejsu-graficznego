using ClientNode;
using ClientWindow;
using ManagementApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ClientWindow
{
    class CPCC
    { 
        private ClientWindow clientWindowHandler;
        private int controlPort;
        private TcpClient connection;
        string ip;
        BinaryReader reader;
        BinaryWriter writer;
        private Thread thread;

    
        public CPCC(ClientWindow clientWindowHandler, string controlPort)
        {
            this.clientWindowHandler = clientWindowHandler;
            bool res = int.TryParse(controlPort, out this.controlPort);
            ip = "127.0.0.1";
        }

        public void connect()
        {
            connection = new TcpClient(ip, controlPort);
            thread = new Thread(callThread);
            thread.Start();
        }

        private void callThread()
        {
            writer = new BinaryWriter(connection.GetStream());
            reader = new BinaryReader(connection.GetStream());

            while (true)
            {
                try
                {
                    string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(ControlPacket))
                {
                    ControlPacket packet = received_object.Value.ToObject<ControlPacket>();
                    if(packet.virtualInterface == ControlInterface.CALL_ACCEPT)
                        {
                            if (packet.state == ControlPacket.ACCEPT)
                            {
                                clientWindowHandler.addToConnectionCombobox(packet.RequestID, packet.destinationIdentifier);
                                clientWindowHandler.slots.Clear();
                                if(packet.Vc11  != 0)
                                {
                                    clientWindowHandler.slots.Add(11);
                                }
                                if(packet.Vc12 != 0)
                                {
                                    clientWindowHandler.slots.Add(12);
                                }
                                if(packet.Vc13 != 0)
                                {
                                    clientWindowHandler.slots.Add(13);
                                }

                                clientWindowHandler.Log2("CONTROL", "CPCC <- NCC Call Request Accepted");
                            }else
                            {
                                clientWindowHandler.Log2("CONTROL", "CPCC <- NCC Call Request Rejected");
                            }
                        }else if(packet.virtualInterface == ControlInterface.INIT_CPCC_CONNECTION_CONFIRM)
                        {
                            clientWindowHandler.Log2("CONTROL", "CPCC <-> NCC connection established");
                        }else if(packet.virtualInterface == ControlInterface.CALL_INDICATION_CPCC)
                        {
                            clientWindowHandler.Log2("CPCC <- NCC", "Receive Call Indication");
                            ControlPacket packetToNCC = new ControlPacket(ControlInterface.CALL_INDICATION_CPCC_ACCEPT, packet.state, packet.speed, packet.destinationIdentifier, packet.originIdentifier, packet.RequestID);
                            packetToNCC.Vc11 = packet.Vc11;
                            packetToNCC.Vc12 = packet.Vc12;
                            packetToNCC.Vc13 = packet.Vc13;
                            string data = JMessage.Serialize(JMessage.FromValue(packetToNCC));
                            writer.Write(data);
                            clientWindowHandler.Log2("CPCC -> NCC", "Send Call Indication Confirmation");
                        }

                }
                else
                {
                        clientWindowHandler.Log2("CONTROL", "Wrong control packet format");
                }
                }
                catch (IOException e)
                {
                    clientWindowHandler.Log2("CONTROL", "CPCC <-> NCC Connection closed");
                    break;
                }
            }
        }

        public void sendInit()
        {
            Address address = new Address(clientWindowHandler.virtualIP);
            int cpccID = address.type + address.domain + address.subnet + address.space;
            ControlPacket packet = new ControlPacket(ControlInterface.INIT_CPCC_CONNECTION, ControlPacket.IN_PROGRESS, 0, "", clientWindowHandler.virtualIP, cpccID);
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);
            clientWindowHandler.Log2("CONTROL", "Init CPCC <-> NCC connection");
        }

        public void sendRequest(string clientName, int speed)
        {
            ControlPacket packet = new ControlPacket(ControlInterface.CALL_REQUEST,ControlPacket.IN_PROGRESS,speed,clientName,clientWindowHandler.virtualIP, 0);
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);
            clientWindowHandler.Log2("CONTROL", " CPCC -> NCC Send Call Request");
        }


        public void sendRelease(int id, string to)
        {
            ControlPacket packet = new ControlPacket(ControlInterface.CALL_RELEASE_IN, ControlPacket.IN_PROGRESS, 0, to, clientWindowHandler.virtualIP, id);
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);
            clientWindowHandler.Log2("CONTROL", "CPCC -> NCC Send Call Release");
 
        }
    }
}
