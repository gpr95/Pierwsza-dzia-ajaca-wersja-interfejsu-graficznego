using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ControlNCC
{
    class NetworkCallControl
    {
        private int controlPort;
        private TcpListener listener;
        private Dictionary<int, ControlConnectionService> services;
        private string domainNumber;
        private static List<string> directory = new List<string>();

        public NetworkCallControl(string[] domainNumber)
        {
            services = new Dictionary<int, ControlConnectionService>();
            string ip = "127.0.0.1";
            this.domainNumber = domainNumber[0];
            readConfig();
            int.TryParse(domainNumber[1], out this.controlPort);
            listener = new TcpListener(IPAddress.Parse(ip), controlPort);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();

            Console.WriteLine("[INIT]Start NCC, IP: " + ip + " Port: " + controlPort);
            Console.WriteLine("Nodes in my network: ");
            foreach(string node in directory)
            {
                Console.WriteLine(node);
            }
        }

        private void Listen()
        {
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ControlConnectionService service = new ControlConnectionService(client, this);
            }
        }

        public void addService(int ID, ControlConnectionService handler)
        {
            services.Add(ID, handler);
        }

        //remove

        private void readConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("config.xml");
            XmlNode portNode = doc.DocumentElement.SelectSingleNode("/domain"+domainNumber+"/controlPort");
            string controlPort = portNode.InnerText;
            bool res = int.TryParse(controlPort, out this.controlPort);
            XmlNodeList clients = doc.DocumentElement.SelectNodes("/domain" + domainNumber + "/client");
            foreach(XmlNode node in clients)
            {
                directory.Add(node.InnerText);

            }
        }

        public Boolean checkIfInDirectory(string address)
        {
            if (directory.Contains(address))
            {
                return true;
            }
            else
                return false;
        }
    }
}
