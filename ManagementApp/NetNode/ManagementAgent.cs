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
    //management agent:
    //  receive commands from management centre
    //  make commands to commutation field and ports
    class ManagementAgent
    {
        private TcpListener listener;
        public int port;

        public ManagementAgent(int port)
        {
            this.port = port;
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
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            //TODO receive FIB from management
            if (received_object.Type == typeof(FIB))
            {
                FIB received_fib = received_object.Value.ToObject<FIB>();
                //add to switching field list
                NetNode.addToFib(received_fib);
            }
            else
            {
                //TODO received management signal
            }
            reader.Close();
        }
    }
}