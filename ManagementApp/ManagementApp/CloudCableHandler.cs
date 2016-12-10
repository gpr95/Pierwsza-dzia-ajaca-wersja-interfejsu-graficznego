using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementApp
{
    class CloudCableHandler
    {
        //private 
        private List<NodeConnection> connections;
        private TcpClient client;
        private BinaryWriter writer;
        private BinaryReader reader;
        private TcpListener listener;
        Thread thread;

        public CloudCableHandler(List<NodeConnection> connections, int cloudPort)
        {
            this.connections = connections;
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), cloudPort);
            thread = new Thread(new ThreadStart(listenForCloud));
            thread.Start();
            String parameters = "" + cloudPort;
            System.Diagnostics.Process.Start("CableCloud.exe", parameters);
        }

        private void listenForCloud()
        {
            listener.Start();

            client = listener.AcceptTcpClient();
            writer = new BinaryWriter(client.GetStream());
            reader = new BinaryReader(client.GetStream());
        }

        public void updateConnections(List<NodeConnection> connections)
        {
            this.connections.AddRange(connections); 
            for (int i = 0; i < connections.Count; i++)
            {
                System.Threading.Thread.Sleep(5000);
                String data = JSON.Serialize(JSON.FromValue(connections[i].Prop));
                try
                {
                    writer.Write(data);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
        public void updateOneConnection()
        {
            String data = JSON.Serialize(JSON.FromValue(connections.Last().Prop));
            try
            {
                writer.Write(data);
            }catch(SocketException e)
            {
                Console.WriteLine(e.StackTrace);
            }catch(IOException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            
        }

        public void stopRunning()
        {
            thread.Interrupt();
        }
    }
}
