using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementApp
{
    public class ManagementHandler
    {
        private TcpClient clientManagement;
        private BinaryWriter writerManagement;
        private BinaryReader readerManagement;
        private TcpListener listenerManagement;
        private Thread threadManagement;

        public ManagementHandler(int applicationPort, int nodeConnectionPort, int nccPort = 0)
        {
            listenerManagement = new TcpListener(IPAddress.Parse("127.0.0.1"), applicationPort);
            threadManagement = new Thread(new ThreadStart(listenForManagement));
            threadManagement.Start();
            ProcessStartInfo startInfo = new ProcessStartInfo("Management.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = applicationPort + " " + nodeConnectionPort + " " + nccPort;
            Process.Start(startInfo);
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

        public void sendConnectClientNcc(List<String> nodeNames)
        {
            if (clientManagement != null)
            {
                ApplicationProtocol toSend = new ApplicationProtocol();
                toSend.State = ApplicationProtocol.CONNECTIONTONCC;
                toSend.ConnectionToNcc = nodeNames;
                string data = JSON.Serialize(JSON.FromValue(toSend));
                writerManagement.Write(data);
            }
        }

        public void sandInfoToOtherNcc(List<int> nccPorts)
        {
            ApplicationProtocol toSend = new ApplicationProtocol();
            toSend.State = ApplicationProtocol.TOOTHERNCC;
            toSend.ConnectionToOtherNcc = nccPorts;
            string data = JSON.Serialize(JSON.FromValue(toSend));
            threadManagement = new Thread(new ParameterizedThreadStart(tryToSendData));
            threadManagement.Start(data);
        }

        private void tryToSendData(Object data)
        {
            int numberOfAttempts = 0;
            while (numberOfAttempts < 10)
            {
                if (writerManagement == null)
                    Thread.Sleep(100);
                else
                {
                    writerManagement.Write((string)data);
                    break;
                }
                    
                numberOfAttempts++;
            }
        }

        public void killManagement()
        {
            if (clientManagement != null)
            {
                ApplicationProtocol toSend = new ApplicationProtocol();
                toSend.State = ApplicationProtocol.KILL;
                string data = JSON.Serialize(JSON.FromValue(toSend));
                writerManagement.Write(data);
            }
        }

        internal void sendAllClients(List<string> clientList)
        {
            ApplicationProtocol toSend = new ApplicationProtocol();
            toSend.State = ApplicationProtocol.ALLCLIENTS;
            toSend.AllClients = clientList;
            string data = JSON.Serialize(JSON.FromValue(toSend));
            threadManagement = new Thread(new ParameterizedThreadStart(tryToSendData));
            threadManagement.Start(data);
        }
    }
}
