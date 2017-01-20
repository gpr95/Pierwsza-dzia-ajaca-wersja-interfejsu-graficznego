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
using ManagementApp;
using Management;
using ControlCCRC;

namespace NetNode
{
    class ControlAgent
    {
        //TODO receiving from upper CC information to LRM to reserve resources
        //TODO receive from upper CC information to store fib to fibtable
        //TODO send confirmation to upper CC
        //TODO send info from LRM to RC
        private TcpListener listener;
        public int port;
        private string virtualIp;

        private static BinaryReader reader;
        private static BinaryWriter writer;

        public ControlAgent(int port, string ip)
        {
            this.port = port;
            this.virtualIp = ip;
            //listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        private void Listen()
        {
            TcpClient clienttmp = new TcpClient("127.0.0.1", this.port);
            reader = new BinaryReader(clienttmp.GetStream());
            writer = new BinaryWriter(clienttmp.GetStream());
            try
            {
                while (true)
                {
                    string received_data = reader.ReadString();
                    JSON received_object = JSON.Deserialize(received_data);
                    ControlProtocol received_Protocol = received_object.Value.ToObject<ControlProtocol>();

                    if (received_Protocol.State == ControlProtocol.ALLOCATERES)
                    {
                        Console.WriteLine("Control Signal: allocateRes");
                        //allocate resource and send confirmation of error
                        string rec = received_Protocol.allocateNo;
                        
                        //TODO allocate and return value
                        int port = 1;
                        int amount = 1;
                        int res = LRM.allocateResource(port,amount);
                        if(res != 0)
                        {
                            //send ok
                            sendConfirmation(port, amount, true);
                        }
                        else
                        {
                            //send err
                            sendConfirmation(port, amount, true);
                        }
                    }
                    else if (received_Protocol.State == ControlProtocol.INSERTFIB)
                    {
                        //insert FIB
                        Console.WriteLine("Control Signal: insertFib");
                        FIB rec = received_Protocol.fib;
                        SwitchingField.addToSwitch(rec);
                    }
                    else
                    {
                        Console.WriteLine("Control Signal: undefined protocol");
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

        public static void sendTopology(string from, int port, string to)
        {
            //TODO send to RC e.g. NN0 connected on port 2 with NN1
            string toSend = from + "/" + port.ToString() + "/" + to;
            Console.WriteLine("sending topology to RC: " + toSend);

            ControlProtocol protocol = new ControlProtocol();
            protocol.State = ControlProtocol.SENDTOPOLOGY;
            protocol.topology = toSend;
            String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            writer.Write(send_object);
        }


        public static void sendDeleted(string from, int port, string to)
        {
            //TODO send to RC that row e.g. NN0 connected on port 2 with NN1 is deleted
            string toSend = from + "/" + port.ToString() + "/" + to;
            Console.WriteLine("sending to RC info about deletion: " + toSend);

            ControlProtocol protocol = new ControlProtocol();
            protocol.State = ControlProtocol.SENDDELETED;
            protocol.topologyDeleted = toSend;
            String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            writer.Write(send_object);
        }

        public static void sendConfirmation(int port, int no_vc3, bool flag)
        {
            //TODO send to CC confirmation of resource reservation and vc3 number
            string status = "ERR";
            if (flag == true)
                status = "OK";
            string toSend = status+"/"+port.ToString() + "/" + no_vc3.ToString();
            Console.WriteLine("sending to CC allocated id" + toSend);

            ControlProtocol protocol = new ControlProtocol();
            protocol.State = ControlProtocol.SENDCONFIRMATION;
            protocol.allocationConf = toSend;
            String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            writer.Write(send_object);
        }
    }
}
