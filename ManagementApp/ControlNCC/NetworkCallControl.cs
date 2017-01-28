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
using ManagementApp;


namespace ControlNCC
{
    class NetworkCallControl
    {
        private int controlPort;
        private TcpListener listener;
        private Dictionary<int, ControlConnectionService> services;
        private int domainNumber;
        private static List<string> directory = new List<string>();
        private ControlConnectionService CCService;
        private ManagementHandler management;
        private int managementPort;
        public NetworkCallControl(string[] domainParams)
        {
            
            services = new Dictionary<int, ControlConnectionService>();
            string ip = "127.0.0.1";
            int.TryParse(domainParams[0], out domainNumber);
            Console.WriteLine("Domain: " + domainNumber + " Listener: " + domainParams[1] + " Management: " + domainParams[2]);
            //readConfig();
            int.TryParse(domainParams[1], out this.controlPort);
            listener = new TcpListener(IPAddress.Parse(ip), controlPort);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
            Console.WriteLine("Nodes in my network: ");
            Console.WriteLine("[INIT]Start NCC, IP: " + ip + " Port: " + controlPort);

            int.TryParse(domainParams[2], out this.managementPort);
            management = new ManagementHandler(this.managementPort, this);
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

        public ControlConnectionService getService(int ID)
        {
            return services[ID];
        }

        public void setCCService(ControlConnectionService handler)
        {
            this.CCService = handler;
        }

        public ControlConnectionService getCCService()
        {
            return this.CCService;
        }

        //remove

        //private void readConfig()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load("config.xml");
        //    XmlNode portNode = doc.DocumentElement.SelectSingleNode("/domain"+domainNumber+"/controlPort");
        //    string controlPort = portNode.InnerText;
        //    bool res = int.TryParse(controlPort, out this.controlPort);
        //    XmlNodeList clients = doc.DocumentElement.SelectNodes("/domain" + domainNumber + "/client");
        //    foreach(XmlNode node in clients)
        //    {
        //        directory.Add(node.InnerText);

        //    }
        //}

        public Boolean checkIfInDirectory(string address)
        {
            Address addres = new Address(address);
            String[] addressArray = address.Split('.');
            int.TryParse(addressArray[0], out addres.type);
            int.TryParse(addressArray[1], out addres.domain);
            int.TryParse(addressArray[2], out addres.subnet);
            int.TryParse(addressArray[3], out addres.space);
            if(addres.domain == domainNumber)
            {
                return true;
            }else
            {
                return false;
            }

            
        }
    }
}
