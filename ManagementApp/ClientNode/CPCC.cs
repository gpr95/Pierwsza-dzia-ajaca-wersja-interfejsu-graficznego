using ClientNode;
using ClientWindow;
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

                                clientWindowHandler.Log2("CONTROL", "call request accepted");
                            }else
                            {
                                clientWindowHandler.Log2("CONTROL", "call request rejected");
                            }
                        }

                }
                else
                {
                        clientWindowHandler.Log2("CONTROL", "Wrong control packet format");
                }
                }
                catch (IOException e)
                {
                    clientWindowHandler.Log2("CONTROL", "Connection closed");
                    break;
                }
            }
        }

        public void sendRequest(string clientName, int speed)
        {
            ControlPacket packet = new ControlPacket(ControlInterface.CALL_REQUEST,ControlPacket.IN_PROGRESS,speed,clientName,clientWindowHandler.virtualIP, clientWindowHandler.adaptation());
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);
            clientWindowHandler.Log2("CONTROL", "send request on " + ControlInterface.CALL_REQUEST + " interface for"+ clientName);

        }

    }
}
