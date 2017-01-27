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
        private TcpClient clientManagement;
        private BinaryWriter writerManagement;
        private BinaryReader readerManagement;
        private TcpListener listenerManagement;
        private Thread threadNCC;

        public AgentNCC(int nccPort)
        {
            Thread.Sleep(100);
            UserInterface.log("Lisening for NCC started at " + nccPort, ConsoleColor.Yellow);
            listenerManagement = new TcpListener(IPAddress.Parse("127.0.0.1"), nccPort);
            threadNCC = new Thread(new ThreadStart(listenForNCC));
            threadNCC.Start();
        }

        private void listenForNCC()
        {
            listenerManagement.Start();
            clientManagement = listenerManagement.AcceptTcpClient();
            writerManagement = new BinaryWriter(clientManagement.GetStream());
            readerManagement = new BinaryReader(clientManagement.GetStream());
            UserInterface.log("Connection successfully established with NCC.", ConsoleColor.Green);
        }

        public void stopRunning()
        {
            threadNCC.Interrupt();
        }
    }
}
