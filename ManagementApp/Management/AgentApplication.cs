using ManagementApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Management
{
    class AgentApplication
    {
        private int port;
        private string virtualIp;

        public AgentApplication(int port, string ip)
        {
            this.port = port;
            this.virtualIp = ip;
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
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError sending signal: " + e.Message);
                Thread.Sleep(100);
                Environment.Exit(1);
            }
        }
    }
}
