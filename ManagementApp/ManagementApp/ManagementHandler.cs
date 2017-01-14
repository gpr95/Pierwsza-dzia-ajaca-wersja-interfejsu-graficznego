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
    class ManagementHandler
    {
        private TcpClient clientManagement;
        private BinaryWriter writerManagement;
        private BinaryReader readerManagement;
        private TcpListener listenerManagement;
        private Thread threadManagement;

        public ManagementHandler(int port)
        {
            listenerManagement = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            threadManagement = new Thread(new ThreadStart(listenForManagement));
            threadManagement.Start();
            String parameters = "" + port;
            System.Diagnostics.Process.Start("Management.exe", parameters);
        }

        private void listenForManagement()
        {
            listenerManagement.Start();
            clientManagement = listenerManagement.AcceptTcpClient();
            writerManagement = new BinaryWriter(clientManagement.GetStream());
            readerManagement = new BinaryReader(clientManagement.GetStream());
        }

        public void stopRunning()
        {
            threadManagement.Interrupt();
        }
    }
}
