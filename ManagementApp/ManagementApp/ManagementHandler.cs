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

        public ManagementHandler(int applicationPort, int nodeConnectionPort)
        {
            listenerManagement = new TcpListener(IPAddress.Parse("127.0.0.1"), applicationPort);
            threadManagement = new Thread(new ThreadStart(listenForManagement));
            threadManagement.Start();
            String parameters = "" + applicationPort + " " + nodeConnectionPort;
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

        public void killManagement()
        {
            if(clientManagement != null)
            {
                ApplicationProtocol toSend = new ApplicationProtocol();
                toSend.State = ApplicationProtocol.KILL;
                string data = JSON.Serialize(JSON.FromValue(toSend));
                writerManagement.Write(data);
            }
        }
    }
}
