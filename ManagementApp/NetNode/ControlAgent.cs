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
using ControlCCRC.Protocols;

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
                    CCtoCCSignallingMessage received_Protocol = received_object.Value.ToObject<CCtoCCSignallingMessage>();

                    if (received_Protocol.State == CCtoCCSignallingMessage.CC_UP_FIB_CHANGE)
                    {
                        //insert FIB
                        Console.WriteLine("Control Signal: insertFib");
                        List<FIB> rec = received_Protocol.Fib_table;

                        //TODO allocate resources
                        foreach (var row in rec)
                        {
                            LRM.allocateResource(row.iport, row.in_cont);
                            LRM.allocateResource(row.oport, row.out_cont);
                            SwitchingField.addToSwitch(row);
                        }
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

        public static void sendCCInit(string ip)
        {
            Console.WriteLine("sending init to CC: " + ip);

            CCtoCCSignallingMessage protocol = new CCtoCCSignallingMessage();
            protocol.State = CCtoCCSignallingMessage.CC_LOW_INIT;
            protocol.NodeName = ip;
            String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            writer.Write(send_object);
        }

        public static void sendTopologyInit(string from)
        {
            Console.WriteLine("sending inittopology to RC: " + from);

            RCtoLRMSignallingMessage protocol = new RCtoLRMSignallingMessage();
            protocol.State = RCtoLRMSignallingMessage.LRM_INIT;
            protocol.NodeName = from;
            String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            writer.Write(send_object);
        }

        public static void sendTopology(string from, int port, string to)
        {
            string toSend = port.ToString() + " " + to;
            Console.WriteLine("sending topology to RC: " + toSend);

            RCtoLRMSignallingMessage protocol = new RCtoLRMSignallingMessage();
            protocol.State = RCtoLRMSignallingMessage.LRM_TOPOLOGY_ADD;
            protocol.ConnectedNodePort = port;
            protocol.ConnectedNode = to;
            String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            writer.Write(send_object);
        }


        public static void sendDeleted(string from, int port, string to)
        {
            string toSend = port.ToString() + " " + to;
            Console.WriteLine("sending to RC info about deletion: " + toSend);

            RCtoLRMSignallingMessage protocol = new RCtoLRMSignallingMessage();
            protocol.State = RCtoLRMSignallingMessage.LRM_TOPOLOGY_DELETE;
            protocol.ConnectedNodePort = port;
            protocol.ConnectedNode = to;
            String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            writer.Write(send_object);
        }

        public static void sendConfirmation(int port, int no_vc3, bool flag)
        {
            CCtoCCSignallingMessage protocol = new CCtoCCSignallingMessage();

            if (flag == true)
            {
                protocol.State = CCtoCCSignallingMessage.CC_LOW_CONFIRM;
                Console.WriteLine("Send CONFIRM");
            }
            else
            {
                protocol.State = CCtoCCSignallingMessage.CC_LOW_REJECT;
                Console.WriteLine("Send REJECT");
            }
            String send_object = JMessage.Serialize(JMessage.FromValue(protocol));
            writer.Write(send_object);
        }
    }
}
