using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ControlNCC
{
    class NetworkCallControl
    {
        private int controlPort;
        private TcpListener listener;
        private Dictionary<int, CPCCService> services;

        public NetworkCallControl()
        {
            services = new Dictionary<int, CPCCService>();
            string ip = "127.0.0.1";
            readConfig();
            listener = new TcpListener(IPAddress.Parse(ip), controlPort);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();

            Console.WriteLine("[INIT]Start NCC, IP: " + ip + " Port: " + controlPort);
        }

        private void Listen()
        {
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                CPCCService service = new CPCCService(client, this);
            }
        }

        public void addService(int ID, CPCCService handler)
        {
            services.Add(ID, handler);
        }

        //remove

        private void readConfig()
        {
            XDocument doc = XDocument.Load("config.xml");
            string value = doc.XPathSelectElement("//config[1]/controlPort").Value;
            bool res = int.TryParse(value, out controlPort);
        }
    }
}
