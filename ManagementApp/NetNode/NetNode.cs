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

        public int physicalPort;
        private TcpListener listener;

        public NetNode(string[] args)
        {
            this.virtualIp = args[0];
            //TODO readConfig()
            this.ports = new Ports();
            this.switchField = new SwitchingField();
            this.agent = new ManagementAgent(Convert.ToInt32(args[1]));
            this.listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[2]));
            this.physicalPort = Convert.ToInt32(args[2]);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
            ConsoleInterface();
        }
        private void Listen()
        {
            this.listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                clientThread.Start(client);
            }
        }

        private void ListenThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            if (received_object.Type == typeof(Frame))
            {
                Frame received_frame = received_object.Value.ToObject<Frame>();
                toVirtualPort(received_frame);
                
            }
            Console.WriteLine(received_data);
            reader.Close();
        }

        private void toVirtualPort(Frame received_frame)
        {
            //TODO evaluate frame header
            int iport=0;//temporary
            ports.iports[iport].addToInQueue(received_frame);
        }

        private void ConsoleInterface()
        {
            //main for testing netNode
            Console.WriteLine("NetNode");

            //sprawdza czy sa jakies pakiety w kolejkach w portach wejsciowych
            while(true)
            {
                foreach(IPort iport in this.ports.iports)
                {
                    //check if there is frame in queue and try to process it 
                    if(iport.input.Count > 0)
                    {
                        //zabranie z kolejki pakietu
                        ClientNode.Frame frame = iport.input.Dequeue();
                        //wyliczenie wyjscia na ktore ma przejsc pakiet zgodnie z tablica forwardowania
                        int oport = this.switchField.commuteFrame(frame, frame.sourceAddress);
                        //TODO zmiana stm-1 dodanie portu wyjsciowego

                        //dopisanie do odpowiedniego portu wyjsciowego
                        this.ports.oports[oport].addToOutQueue(frame);
                    }
                }
                foreach (OPort oport in this.ports.oports)
                {
                    //check if there is frame in queue and try to send it 
                    if (oport.output.Count > 0)
                    {
                        Frame frame = oport.output.Dequeue();
                        TcpClient client = new TcpClient();
                        client.Connect(IPAddress.Parse("127.0.0.1"), this.physicalPort);
                        BinaryWriter writeOutput = new BinaryWriter(client.GetStream());
                        string data = JMessage.Serialize(JMessage.FromValue(frame));
                        writeOutput.Write(data);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            string[] parameters = new string[] { "192.168.56.55", "111", "112" };
            NetNode netnode = new NetNode(parameters);
        }
    }
}
