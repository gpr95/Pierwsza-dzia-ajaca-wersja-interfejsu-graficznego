using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlNCC
{
    class ManagementHandler
    {
        private int port;
        private Thread thread;
        private TcpClient client;
        private NetworkCallControl control;

        public ManagementHandler(int port, NetworkCallControl control)
        {
            
            this.control = control;
            this.port = port;
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        private void Listen()
        {
            try
            {
                client = new TcpClient("127.0.0.1", this.port);
                BinaryReader reader = new BinaryReader(client.GetStream());
                BinaryWriter writer = new BinaryWriter(client.GetStream());
                Console.WriteLine("BOCZEK!");
                while (true)
                {
                    string received_data = reader.ReadString();
                    //JSON received_object = JSON.Deserialize(received_data);
                    //ApplicationProtocol received_Protocol = received_object.Value.ToObject<Mana>();
                    ///Reciving
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
    }
}
