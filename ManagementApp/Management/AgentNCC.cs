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
        }

        public void sendInfoToOtherNcc(List<int> nccPorts)
        {
            ManagmentProtocol toSend = new ManagmentProtocol();
            toSend.State = ManagmentProtocol.TOOTHERNCC;
            toSend.ConnectionToOtherNcc = nccPorts;
            string data = ManagementApp.JSON.Serialize(ManagementApp.JSON.FromValue(toSend));
            Thread.Sleep(150);
            writerNCC.Write(data);
        }

        public void sendSoftPernament(String start, String end)
        {
            ManagmentProtocol toSend = new ManagmentProtocol();
            toSend.State = ManagmentProtocol.SOFTPERNAMENT;
            toSend.NodeStart = start;
            toSend.NodeEnd = end;
            string data = ManagementApp.JSON.Serialize(ManagementApp.JSON.FromValue(toSend));
            writerNCC.Write(data);
        }

        public void stopRunning()
        {
            threadNCC.Interrupt();
        }
    }
}
