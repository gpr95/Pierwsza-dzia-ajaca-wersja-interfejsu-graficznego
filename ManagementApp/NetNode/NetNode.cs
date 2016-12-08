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
        private static SwitchingField switchField = new SwitchingField();
        public Ports ports;
        public ManagementAgent agent;

        public int physicalPort;
        private TcpListener listener;

        public NetNode(string[] args)
        {
            this.virtualIp = args[0];
            //TODO readConfig()
            this.ports = new Ports();
            //this.switchField = new SwitchingField();
            this.agent = new ManagementAgent(Convert.ToInt32(args[2]),this.virtualIp);
            this.listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[1]));
            this.physicalPort = Convert.ToInt32(args[1]);
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
            if (received_object.Type == typeof(Signal))
            {
                Signal received_signal = received_object.Value.ToObject<Signal>();
                STM1 frame = received_signal.stm1;
                int virtPort = received_signal.port;
                Console.WriteLine("received signal time: " + received_signal.time + " on port: "+virtPort);
                toVirtualPort(virtPort, frame);
                
            }
            Console.WriteLine(received_data);
            reader.Close();
        }

        private void toVirtualPort(int virtPort, STM1 received_frame)
        {
            ports.iports[virtPort].addToInQueue(received_frame);
        }

        private void ConsoleInterface()
        {
            Console.WriteLine("NetNode " + this.virtualIp);

            while(true)
            {
                foreach(IPort iport in this.ports.iports)
                {
                    //check if there is frame in queue and try to process it 
                    if(iport.input.Count > 0)
                    {
                        STM1 frame = iport.input.Dequeue();

                        if(frame.vc4 != null)
                        {
                            int out_pos;
                            VirtualContainer4 vc4 = frame.vc4;
                            out_pos = switchField.commuteContainer(vc4, iport.port);
                            if(out_pos != -1)
                            {
                                this.ports.oports[out_pos].addToOutQueue(vc4);
                            }
                        }
                        else if (frame.vc3List.Count != 0)
                        {
                            foreach(var vc in frame.vc3List)
                            {
                                VirtualContainer3 vc3 = vc.Value;
                                if (vc3 != null)
                                {
                                    int [] out_pos;
                                    out_pos = switchField.commuteContainer(vc3, iport.port, vc.Key);
                                    if (out_pos[0] != -1)
                                    {
                                        this.ports.oports[out_pos[0]].addToTempQueue(vc3, out_pos[1]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("smth wrong with stm1");
                        }
                    }
                }
                foreach (OPort oport in this.ports.oports)
                {
                    //pakowanie w STM to co jest w tempQueue
                    oport.addToOutQueue();
                }
                foreach (OPort oport in this.ports.oports)
                {
                    //check if there is frame in queue and try to send it 
                    if (oport.output.Count > 0)
                    {
                        STM1 frame = oport.output.Dequeue();
                        if (frame.vc4 != null || frame.vc3List != null)
                        {   
                            //TODO from management
                            int virtualPort = 3;
                            Signal signal = new Signal(DateTimeToInt(), virtualPort, frame);
                            Console.WriteLine(signal);
                            TcpClient client = new TcpClient();
                            client.Connect(IPAddress.Parse("127.0.0.1"), this.physicalPort);
                            BinaryWriter writeOutput = new BinaryWriter(client.GetStream());
                            string data = JMessage.Serialize(JMessage.FromValue(signal));
                            writeOutput.Write(data);
                        }
                    }
                }
            }
        }

        public static int DateTimeToInt()
        {
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            return unixTime;
        }

        static void Main(string[] args)
        {
            //string[] parameters = new string[] { "NN0", "7777", "7776" };
            string[] parameters = new string[] { args[0], args[1], args[2] };

            NetNode netnode = new NetNode(parameters);
        }
    }
}
