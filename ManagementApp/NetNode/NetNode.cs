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
        public Ports ports;
        public ManagementAgent agent;

        public static Boolean flag;
        public int physicalPort;
        private TcpListener listener;
        private static BinaryWriter writer;
        private Thread threadListen;
        private Thread threadConsole;
        private Thread threadComutation;

        public NetNode(string[] args)
        {
            flag = true;
            this.virtualIp = args[0];
            Console.Title = args[0];
            this.ports = new Ports();
            this.agent = new ManagementAgent(Convert.ToInt32(args[2]), this.virtualIp);
            this.listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[1]));
            this.physicalPort = Convert.ToInt32(args[1]);
            this.threadListen = new Thread(new ThreadStart(Listen));
            threadListen.Start();
            this.threadConsole = new Thread(new ThreadStart(ConsoleInterface));
            threadConsole.Start();
            this.threadComutation = new Thread(new ThreadStart(commutation));
            threadComutation.Start();
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
            try
            {
                while (true)
                {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type == typeof(Signal))
                    {
                        Signal received_signal = received_object.Value.ToObject<Signal>();
                        STM1 frame = received_signal.stm1;
                        int virtPort = received_signal.port;
                        consoleWriter("received signal on port: " + virtPort);
                        toVirtualPort(virtPort, frame);
                        Console.WriteLine(received_data);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError sending signal: " + e.Message);
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
            Console.WriteLine("NetNode " + this.virtualIp + " " + this.agent.port + " " + this.physicalPort);

            Boolean quit = false;
            while (!quit)
            {
                Console.WriteLine("\n MENU: ");
                Console.WriteLine("\n 1) Manually insert entry in connection table");
                Console.WriteLine("\n 2) Show connection table");
                Console.WriteLine("\n 3) Clear connection table");
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
                                Console.WriteLine("ok");
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
                                        Console.WriteLine("ok");
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
                                Signal signal = new Signal(oport.port, frame);
                                consoleWriter("sending signal port: " + signal.port);
                                string data = JMessage.Serialize(JMessage.FromValue(signal));
                                Console.WriteLine(data);
                                writer.Write(data);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("\nError sending signal: " + e.Message);
                                Thread.Sleep(2000);
                                Environment.Exit(1);
                            }
                        }
                    }
                }
                Thread.Sleep(1250);
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
        }

        private void consoleWriter(String msg)
        {
            Console.WriteLine("#" + DateTime.Now.ToLongTimeString() + DateTime.Now.ToLongDateString() + "#:" + msg);
        }

        static void Main(string[] args)
        {
            string[] parameters = new string[] { args[0], args[1], args[2] };

            NetNode netnode = new NetNode(parameters);
        }
    }
}
