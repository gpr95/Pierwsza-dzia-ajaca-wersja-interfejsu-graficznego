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
using ManagementApp;

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
        private static BinaryWriter writer;

        public NetNode(string[] args)
        {
            this.virtualIp = args[0];
            this.ports = new Ports();
            this.agent = new ManagementAgent(Convert.ToInt32(args[2]), this.virtualIp);
            this.listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[1]));
            this.physicalPort = Convert.ToInt32(args[1]);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
            Thread threadConsole = new Thread(new ThreadStart(ConsoleInterface));
            threadConsole.Start();
            this.commutation();
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
            writer = new BinaryWriter(clienttmp.GetStream());
            while (true)
            {
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(Signal))
                {
                    Signal received_signal = received_object.Value.ToObject<Signal>();
                    STM1 frame = received_signal.stm1;
                    int virtPort = received_signal.port;
                    consoleWriter("received signal time: " + received_signal.time + " on port: " + virtPort);
                    toVirtualPort(virtPort, frame);

                }
            }
        }

        private void toVirtualPort(int virtPort, STM1 received_frame)
        {
            ports.iports[virtPort].addToInQueue(received_frame);
        }

        private void ConsoleInterface()
        {
            Console.WriteLine("NetNode " + this.virtualIp + " " + this.agent.port + " " + this.physicalPort);

            Boolean quit = false;
            while (!quit)
            {
                Console.WriteLine("\n MENU: ");
                Console.WriteLine("\n 1) Manually insert FIB");
                Console.WriteLine("\n 2) Simulate failure");

                int choice;
                bool res = int.TryParse(Console.ReadLine(), out choice);
                if (res)
                {
                    switch (choice)
                    {
                        case 1:
                            insertFib();
                            break;
                        case 2:
                            //TODO
                            break;
                        default:
                            Console.WriteLine("\n Wrong option");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Wrong format");
                    ConsoleInterface();
                }

            }
        }
        private void commutation()
        {
            while (true)
            {
                foreach (IPort iport in this.ports.iports)
                {
                    //check if there is frame in queue and try to process it 
                    if (iport.input.Count > 0)
                    {
                        STM1 frame = iport.input.Dequeue();

                        if (frame.vc4 != null)
                        {
                            int out_pos = -1;
                            VirtualContainer4 vc4 = frame.vc4;
                            out_pos = switchField.commuteContainer(vc4, iport.port);
                            if (out_pos != -1)
                            {
                                Console.WriteLine("ok");
                                this.ports.oports[out_pos].addToOutQueue(vc4);
                            }
                        }
                        else if (frame.vc3List.Count > 0)
                        {
                            foreach (var vc in frame.vc3List)
                            {
                                VirtualContainer3 vc3 = vc.Value;
                                if (vc3 != null)
                                {
                                    int[] out_pos = {-1,-1};
                                    out_pos = switchField.commuteContainer(vc3, iport.port, vc.Key);
                                    if (out_pos[0] != -1)
                                    {
                                        Console.WriteLine("ok");
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
                    //packing STM from tempQueue to outqueue
                    oport.addToOutQueue();
                }

                foreach (OPort oport in this.ports.oports)
                {
                    //check if there is frame in queue and try to send it 
                    if (oport.output.Count > 0)
                    {
                        STM1 frame = oport.output.Dequeue();
                        if (frame.vc4 != null || frame.vc3List.Count > 0)
                        {
                            Signal signal = new Signal(getTime(), oport.port, frame);
                            consoleWriter("sending signal port: " + signal.port);
                            //TcpClient client = new TcpClient();
                            //client.Connect(IPAddress.Parse("127.0.0.1"), this.physicalPort);
                            //BinaryWriter writeOutput = new BinaryWriter(client.GetStream());
                            string data = JMessage.Serialize(JMessage.FromValue(signal));
                            writer.Write(data);
                        }
                    }
                }
                Thread.Sleep(125);
            }
        }

        private void insertFib()
        {
            FIB fib = new FIB(0, 0, 0, 0);
            Console.WriteLine("Insert input port:");
            Int32.TryParse(Console.ReadLine(), out fib.iport);
            Console.WriteLine("Insert input container position:");
            Int32.TryParse(Console.ReadLine(), out fib.in_cont);
            Console.WriteLine("Insert output port:");
            Int32.TryParse(Console.ReadLine(), out fib.oport);
            Console.WriteLine("Insert output container position:");
            Int32.TryParse(Console.ReadLine(), out fib.out_cont);

            SwitchingField.addToSwitch(fib);
        }

        private void consoleWriter(String msg)
        {
            Console.WriteLine("#" + DateTime.Now.ToLongTimeString() + DateTime.Now.ToLongDateString() + "#:" + msg);
        }

        private int getTime()
        {
            Random r = new Random();
            int time = r.Next(10, 125);
            return time;
        }

        static void Main(string[] args)
        {
            //string[] parameters = new string[] { "NN0", "7777", "7776" };
            string[] parameters = new string[] { args[0], args[1], args[2] };

            NetNode netnode = new NetNode(parameters);
        }
    }
}
