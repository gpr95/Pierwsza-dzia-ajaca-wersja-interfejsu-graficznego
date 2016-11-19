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

namespace NetNode
{
    //input port
    class IPort
    {
        public int port;
        public Queue<Packet> input = new Queue<Packet>();
        private TcpListener listener;

        public IPort(int port)
        {
            this.port = port;
            listener = new TcpListener(IPAddress.Parse("192.168.56.1"), 1234+port);
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
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            if (received_object.Type == typeof(Packet))
            {
                Packet packet = received_object.Value.ToObject<Packet>();
                Console.WriteLine("Message received: " + packet.message + " by port: " + port);
               
                addToInQueue(packet);
            }
            else
            {
                Console.WriteLine("\n Odebrano uszkodzony pakiet");
            }
            reader.Close();
        }

        public void addToInQueue(ClientNode.Packet packet)
        {
            input.Enqueue(packet);
        }
    }
}
