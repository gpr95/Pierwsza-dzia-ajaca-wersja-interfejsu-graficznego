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
        private string domainNumber;
    
        public CPCC(ClientWindow clientWindowHandler, string domainNumber)
        {
            this.clientWindowHandler = clientWindowHandler;
            this.domainNumber = domainNumber;
            this.readConfig();
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
                            clientWindowHandler.Log2("CONTROL", "uuu mam slot i port moge slac");
                            
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

        public void sendRequest(string clientName)
        {
            ControlPacket packet = new ControlPacket(ControlInterface.CALL_REQUEST, 0, clientName);
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);
            clientWindowHandler.Log2("CONTROL", "send request on " + ControlInterface.CALL_REQUEST + " interface");

        }

        private void readConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("config.xml");
            XmlNode portNode = doc.DocumentElement.SelectSingleNode("/domain"+ domainNumber +"/controlPort");
            string port = portNode.InnerText;
            bool res = int.TryParse(port, out this.controlPort);
        }
    }
}
