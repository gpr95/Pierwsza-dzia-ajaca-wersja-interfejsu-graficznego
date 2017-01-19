using ClientNode;
using ClientWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlNCC
{
    class ControlConnectionService
    {
        private TcpClient client;
        private BinaryWriter writer;
        private NetworkCallControl handlerNCC;
        private TcpClient connection;
        private Thread thread;
        private string ip;
        private int connectionControlPort;   //do dodania
        BinaryWriter writerToCC;

        public ControlConnectionService(TcpClient clientHandler, NetworkCallControl handlerNCC)
        {
            this.client = clientHandler;
            this.handlerNCC = handlerNCC;
            init(client);
            ip = "127.0.0.1";
            
        }

        private void init(TcpClient client)
        {
            Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
            clientThread.Start(client);
        }

        private void ListenThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            writer = new BinaryWriter(clienttmp.GetStream());
            while (true)
            {
                try {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type == typeof(ControlPacket))
                    {
                        ControlPacket packet = received_object.Value.ToObject<ControlPacket>();
                        if(packet.virtualInterface == ControlInterface.CALL_REQUEST)
                        {
                            Console.WriteLine("[CPCC]Receive call request for "+packet.resourceIdentifier+" on " + ControlInterface.CALL_REQUEST_ACCEPT + " interface");
                            Console.WriteLine("[DIRECTORY]Send directory request");//sprawdzenie czy w naszej domenie 
                            if (handlerNCC.checkIfInDirectory(packet.resourceIdentifier))
                            {
                                Console.WriteLine("[DIRECTORY]Receive local name");
                                Console.WriteLine("[POLICY]Send policy out");
                                Console.WriteLine("[POLICY]Call accept");
                                Console.WriteLine("[CPCC]Send call indication");
                                Console.WriteLine("[CPCC]Call confirmed");
                                //wysylanie do innego cpcc
                                Console.WriteLine("[CC]Send connection request");
                                //connection = new TcpClient(ip, connectionControlPort);
                               // thread = new Thread(new ParameterizedThreadStart(connectionControlThread));
                               // thread.Start(packet.resourceIdentifier);
                            }
                            else
                            {
                                Console.WriteLine("[DIRECTORY]This client is not in my network");
                                Console.WriteLine("[NCC]Send call request to next NCC");
                            }
                            
                           
                            
                            Console.WriteLine("[CPCC] Call confirmed or not");


                        }

                    } else
                    {
                        Console.WriteLine("Wrong control packet format");
                    }

                }catch(IOException e)
                {
                    Console.WriteLine("Connection closed");
                    break;
                }
             }
        }

        public void send(string virtualInterface, int FLAG, string resourceIdentifier, int virtualPort, int slot)
        {
            ControlPacket packet = new ControlPacket(ControlInterface.CALL_REQUEST, 0, resourceIdentifier);
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);
            
        }

        private void connectionControlThread(object resourceIdentifier)
        {
            string address = (string)resourceIdentifier;
            writer = new BinaryWriter(connection.GetStream());
            BinaryReader reader = new BinaryReader(connection.GetStream());
            ControlPacket packet = new ControlPacket(ControlInterface.CONNECTION_REQUEST_OUT, 0, address);
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            while (true)
                try
                {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type == typeof(ControlPacket))
                    {
                        ControlPacket packet_received = received_object.Value.ToObject<ControlPacket>();
                        if (packet_received.virtualInterface == ControlInterface.CONNECTION_REQUEST_OUT)
                        {
                            Console.WriteLine("[CC]Connection confirmed or not");
                            // albo ok i szczeliny albo nie i lipton
                        }

                    }
                    else
                    {
                        Console.WriteLine("[ERR]Wrong control packet format");
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("[ERR]Connection closed");
                    break;
                }
        }

    }

    }

