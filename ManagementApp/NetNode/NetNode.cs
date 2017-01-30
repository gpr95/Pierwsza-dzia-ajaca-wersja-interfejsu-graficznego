using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientWindow;
using Management;

namespace NetNode
{
    class NetNode
    {
        private string virtualIp;
        private static SwitchingField switchField = new SwitchingField();
        private LRM lrm;
        public Ports ports;
        public ManagementAgent managementAgent;
        public ControlAgent controlAgent;

        public static Boolean flag;
        public int physicalPort;
        private TcpListener listener;
        private static BinaryWriter writer;
        private Thread threadListen;
        private Thread threadConsole;
        private Thread threadComutation;

        private List<string> pathList = new List<string>();

        public NetNode(string[] args)
        {
            flag = true;
            this.virtualIp = args[0];
            Console.Title = args[0];
            this.ports = new Ports();
            this.physicalPort = Convert.ToInt32(args[1]);
            this.managementAgent = new ManagementAgent(Convert.ToInt32(args[2]), this.virtualIp);
            this.controlAgent = new ControlAgent(Convert.ToInt32(args[3]), this.virtualIp);
            this.listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[1]));

            this.threadListen = new Thread(new ThreadStart(Listen));
            threadListen.Start();
            this.threadConsole = new Thread(new ThreadStart(ConsoleInterface));
            threadConsole.Start();
            this.threadComutation = new Thread(new ThreadStart(commutation));
            threadComutation.Start();

            if (args[3] != "0")
            {
                this.lrm = new LRM(args[0]);
            }

            //this.commutation();
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
            LRM.writer = new BinaryWriter(clienttmp.GetStream());

            try
            {
                while (true)
                {
                    string received_data = reader.ReadString();
                    //Console.WriteLine(received_data);
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type == typeof(Signal))
                    {
                        Signal received_signal = received_object.Value.ToObject<Signal>();
                        if(received_signal.stm1 != null)
                        {
                            this.pathList = received_signal.path;
                            STM1 frame = received_signal.stm1;
                            int virtPort = received_signal.port;
                            consoleWriter("received signal on port: " + virtPort);
                            toVirtualPort(virtPort, frame);
                            //Console.WriteLine(received_data);
                        }
                        else if(received_signal.lrmProtocol != null)
                        {
                            string lrmProtocol = received_signal.lrmProtocol;
                            int port = received_signal.port;
                            this.lrm.receivedMessage(lrmProtocol, port);
                            //Console.WriteLine(received_data);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log("\nError sending signal: " + e.Message, ConsoleColor.Red);
                Thread.Sleep(2000);
                Environment.Exit(1);
            }
        }

        private void toVirtualPort(int virtPort, STM1 received_frame)
        {
            ports.iports[virtPort].addToInQueue(received_frame);
        }

        private void freeze()
        {
            flag = false;
        }

        private void unfreeze()
        {
            flag = true;
            this.threadComutation = new Thread(new ThreadStart(commutation));
            this.threadComutation.Start();
        }

        private void ConsoleInterface()
        {
            log("NetNode " + this.virtualIp + " " + this.managementAgent.port + " " + this.controlAgent.port + " " + this.physicalPort, ConsoleColor.Yellow);

            Boolean quit = false;
            while (!quit)
            {
                Console.WriteLine("\n MENU: ");
                Console.WriteLine("\n 1) Manually insert entry in connection table");
                Console.WriteLine("\n 2) Show connection table");
                Console.WriteLine("\n 3) Clear connection table");
                Console.WriteLine("\n 4) Show node connections");
                Console.WriteLine("\n 5) Show node resource status");
                Console.WriteLine("\n");

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
                            SwitchingField.printFibTable();
                            break;
                        case 3:
                            SwitchingField.clearFibTable();
                            break;
                        case 4:
                            LRM.printConn();
                            break;
                        case 5:
                            LRM.printResources();
                            break;
                        default:
                            log("\n Wrong option", ConsoleColor.Red);
                            break;
                    }
                }
                else
                {
                    log("Wrong format", ConsoleColor.Red);
                    ConsoleInterface();
                }

            }
        }
        private void commutation()
        {
            while (flag)
            {
                int opt = commOption();

                foreach (IPort iport in this.ports.iports)
                {
                    //check if there is frame in queue and try to process it 
                    if (iport.input.Count > 0)
                    {
                        STM1 frame = iport.input.Dequeue();

                        //if (frame.vc4 != null)
                        if(opt != 1)
                        {
                            Console.WriteLine("vc4");
                            int out_pos = -1;
                            VirtualContainer4 vc4 = frame.vc4;
                            out_pos = switchField.commutateContainer(vc4, iport.port);
                            if (out_pos != -1)
                            {
                                log("ok", ConsoleColor.Green);
                                this.ports.oports[out_pos].addToOutQueue(vc4);
                            }
                        }
                        //else if (frame.vc4.vc3List.Count > 0)
                        else
                        {
                            Console.WriteLine("vc3->vc4");
                            Console.WriteLine("unpacking container");
                            foreach (var vc in frame.vc4.vc3List)
                            {
                                VirtualContainer3 vc3 = vc.Value;
                                if (vc3 != null)
                                {
                                    int[] out_pos = { -1, -1 };
                                    out_pos = switchField.commutateContainer(vc3, iport.port, vc.Key);
                                    if (out_pos[0] != -1)
                                    {
                                        log("ok", ConsoleColor.Green);
                                        this.ports.oports[out_pos[0]].addToTempQueue(vc3, out_pos[1]);
                                    }
                                }
                            }
                        }
                        //else
                        //{
                            //Console.WriteLine("smth wrong with stm1");
                        //}
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
                        if (frame.vc4 != null || frame.vc4.vc3List.Count > 0)
                        {
                            try
                            {
                                pathList.Add(this.virtualIp);
                                Signal signal = new Signal(oport.port, frame, pathList);
                                consoleWriter("sending signal port: " + signal.port);
                                string data = JMessage.Serialize(JMessage.FromValue(signal));
                                Console.WriteLine(data);
                                writer.Write(data);
                            }
                            catch (Exception e)
                            {
                                log("\nError sending signal: " + e.Message, ConsoleColor.Red);
                                Thread.Sleep(2000);
                                Environment.Exit(1);
                            }
                        }
                    }
                }
                Thread.Sleep(125);
            }
        }

        private int commOption()
        {
            int counter = 1;
            for (int j = 0; j < SwitchingField.fib.Count; j++)
            {
                int temp = SwitchingField.fib[j].iport;
                int temp2 = SwitchingField.fib[j].oport;
                for (int i = 1; i < SwitchingField.fib.Count; i++)
                {
                    if (SwitchingField.fib[i].oport == temp2)
                    {
                        if (SwitchingField.fib[i].iport != temp && SwitchingField.fib[i].in_cont != 1)
                            counter++;
                    }
                }
                if (counter == 2 || counter == 3)
                    return 1;
            }
            return 0;
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
            //adding fib for two-way communication
            SwitchingField.addToSwitch(new FIB(fib.oport, fib.out_cont, fib.iport, fib.in_cont));
        }

        private void consoleWriter(String msg)
        {
            log("#" + DateTime.Now.ToLongTimeString() + DateTime.Now.ToLongDateString() + "#:" + msg, ConsoleColor.Magenta);
        }

        public static void log(String msg, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void Main(string[] args)
        {
            //string[] parameters = new string[] { args[0], args[1], args[2] };

            NetNode netnode = new NetNode(args);
        }
    }
}
