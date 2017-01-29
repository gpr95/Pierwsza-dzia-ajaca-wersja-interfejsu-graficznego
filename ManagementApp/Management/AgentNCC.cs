using ManagementApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Management
{
    class AgentNCC
    {
        private TcpClient clientNCC;
        private BinaryWriter writerNCC;
        private BinaryReader readerNCC;
        private TcpListener listenerNCC;
        private Thread threadNCC;

        public AgentNCC(int nccPort)
        {
            Thread.Sleep(100);
            UserInterface.log("Lisening for NCC started at " + nccPort, ConsoleColor.Yellow);
            listenerNCC = new TcpListener(IPAddress.Parse("127.0.0.1"), nccPort);
            threadNCC = new Thread(new ThreadStart(listenForNCC));
            threadNCC.Start();
        }

        private void listenForNCC()
        {
            listenerNCC.Start();
            clientNCC = listenerNCC.AcceptTcpClient();
            writerNCC = new BinaryWriter(clientNCC.GetStream());
            readerNCC = new BinaryReader(clientNCC.GetStream());
            UserInterface.log("Connection successfully established with NCC.", ConsoleColor.Green);

            try
            {
                while (true)
                {
                    string received_data = readerNCC.ReadString();
                    JSON received_object = JSON.Deserialize(received_data);
                    ManagmentProtocol received_Protocol = received_object.Value.ToObject<ManagmentProtocol>();

                    if (received_Protocol.State == ManagmentProtocol.SOFTPERNAMENT)
                    {
                        Console.WriteLine("Received signal from NCC connection: " + received_Protocol.Name + " " + received_Protocol.NodeEnd);
                        ManagementPlane.conn.Add(received_Protocol.Name, received_Protocol.NodeEnd);
                    }
                    else
                    {
                        Console.WriteLine("Signal from management: undefined protocol", ConsoleColor.Red);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError sending signal: " + e.Message, ConsoleColor.Red);
                Thread.Sleep(2000);
                Environment.Exit(1);
            }

        }

        public void sendInfoToOtherNcc(List<int> nccPorts)
        {
            ManagmentProtocol toSend = new ManagmentProtocol();
            toSend.State = ManagmentProtocol.TOOTHERNCC;
            toSend.ConnectionToOtherNcc = nccPorts;
            string data = ManagementApp.JSON.Serialize(ManagementApp.JSON.FromValue(toSend));
            Thread threadManagement = new Thread(new ParameterizedThreadStart(tryToSendData));
            threadManagement.Start(data);
        }

        private void tryToSendData(Object data)
        {
            int numberOfAttempts = 0;
            while (numberOfAttempts < 10)
            {
                if (writerNCC == null)
                    Thread.Sleep(100);
                else
                {
                    writerNCC.Write((string)data);
                    break;
                }

                numberOfAttempts++;
            }
        }

        public void sendSoftPernament(String start, String end, int speed)
        {
            ManagmentProtocol toSend = new ManagmentProtocol();
            toSend.State = ManagmentProtocol.SOFTPERNAMENT;
            toSend.NodeStart = start;
            toSend.NodeEnd = end;
            toSend.Speed = speed;
            string data = ManagementApp.JSON.Serialize(ManagementApp.JSON.FromValue(toSend));
            writerNCC.Write(data);
        }

        public void stopRunning()
        {
            threadNCC.Interrupt();
        }
    }
}
