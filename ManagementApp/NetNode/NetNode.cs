using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientNode;

namespace NetNode
{
    class NetNode
    {
        private string virtualIp;
        public SwitchingField switchField;
        public Ports ports;
        public ManagementAgent agent;

        //tcp dla komunikacji z chmura kablowa client i listener
        private TcpListener listenerFromCloud;

        //tcp dla polaczen miedzywezlowych
        TcpClient clientToNodes;
        private TcpListener listenerFromNodes;

        public NetNode(string[] args)
        {
            this.virtualIp = args[0];
            //TODO readConfig()
            this.ports = new Ports();
            this.switchField = new SwitchingField();
            this.agent = new ManagementAgent(Convert.ToInt32(args[1]));
            this.listenerFromCloud = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[2]));
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
            ConsoleInterface();
        }
        private void Listen()
        {
            this.listenerFromCloud.Start();
            while (true)
            {
                TcpClient clientToCloud = listenerFromCloud.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                clientThread.Start(clientToCloud);
            }
        }

        private static void ListenThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            string received_data = reader.ReadString();
            Console.WriteLine(received_data);
            reader.Close();
        }

        private void ConsoleInterface()
        {
            //main for testing netNode
            Console.WriteLine("NetNode");

            //zestawiane tylko na czas odebrania
            //TODO
            int inputPortFromCloud = 2222;
            this.setupListenerFromNodes(inputPortFromCloud);
            //zestawiane tylko na czas wyslania
            this.clientToNodes = new TcpClient();
            int destinationPortFromCloud = 4444;

            //sprawdza czy sa jakies pakiety w kolejkach w portach wejsciowych
            while(true)
            {
                foreach(IPort iport in this.ports.iports)
                {
                    //check if there is packet in queue and try to process it 
                    if(iport.input.Count > 0)
                    {
                        //zabranie z kolejki pakietu
                        ClientNode.Packet pack = iport.input.Dequeue();
                        //wyliczenie wyjscia na ktore ma przejsc pakiet zgodnie z tablica forwardowania
                        int oport = this.switchField.commutePacket(pack, iport.port, pack.sourceAddress);
                        //dopisanie do odpowiedniego portu wyjsciowego
                        this.ports.oports[oport].addToOutQueue(pack);
                    }
                }
                foreach (OPort oport in this.ports.oports)
                {
                    //check if there is packet in queue and try to send it 
                    if (oport.output.Count > 0)
                    {
                        Packet packet = oport.output.Dequeue();
                        clientToNodes.Connect(IPAddress.Parse("127.0.0.1"), destinationPortFromCloud);
                        BinaryWriter writeOutput = new BinaryWriter(this.clientToNodes.GetStream());
                        string data = JMessage.Serialize(JMessage.FromValue(packet));
                        writeOutput.Write(data);
                    }
                }
            }
        }

        private void setupListenerFromNodes(int port)
        {
            //TODO
            this.listenerFromNodes = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            Thread thread2 = new Thread(new ThreadStart(ListenNodes));
            thread2.Start();
        }
        private void ListenNodes()
        {
            this.listenerFromNodes.Start();
            while (true)
            {
                clientToNodes = listenerFromNodes.AcceptTcpClient();
                Thread clientThreadNodes = new Thread(new ParameterizedThreadStart(ListenThreadNodes));
                clientThreadNodes.Start(clientToNodes);
            }
        }
        private static void ListenThreadNodes(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            string received_data = reader.ReadString();
            Console.WriteLine(received_data);
            reader.Close();
        }
        static void Main(string[] args)
        {
            string[] parameters = new string[] { "192.168.56.55", "111", "112" };
            NetNode netnode = new NetNode(parameters);
        }
    }
}
