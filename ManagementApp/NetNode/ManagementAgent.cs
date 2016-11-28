﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetNode
{
    //management agent:
    //  receive commands from management centre
    //  make commands to commutation field and ports
    class ManagementAgent
    {
        private TcpListener listener;
        public int port;
        private int p;

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
            Console.WriteLine(received_data);
            //do smth with received data, configure FIB
            reader.Close();
        }
    }
}