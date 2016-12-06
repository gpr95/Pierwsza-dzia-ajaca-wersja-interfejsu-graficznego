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
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        private void Listen()
        {
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                clientThread.Start(client);
            }
        }
        private void ListenThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            BinaryWriter writer = new BinaryWriter(clienttmp.GetStream());
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            ManagmentProtocol received_Protocol = received_object.Value.ToObject<ManagmentProtocol>();
            
            if (received_Protocol.State == ManagmentProtocol.WHOIS)
            {
                //send name to management
                ManagmentProtocol protocol = new ManagmentProtocol();
                protocol.Name = this.virtualIp;
                String send_object = JSON.Serialize(JSON.FromValue(protocol));
                writer.Write(send_object);
            }
            else if(received_Protocol.State == ManagmentProtocol.ROUTINGTABLES)
            {
                //receiving fibs
                if(received_Protocol.RoutingTable != null)
                {
                    foreach(var fib in received_Protocol.RoutingTable)
                    {
                        SwitchingField.addToSwitch(fib);
                    }
                }
            }
            else
            {
                Console.WriteLine("undefined protocol");
            }
            reader.Close();
        }

    }
}