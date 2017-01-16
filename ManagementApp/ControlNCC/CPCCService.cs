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
    class CPCCService
    {
        private TcpClient client;
        private BinaryWriter writer;
        private NCC handlerNCC;

        public CPCCService(TcpClient clientHandler, NCC handlerNCC)
        {
            this.client = clientHandler;
            this.handlerNCC = handlerNCC;
            init(client);
            
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
                        if(packet.virtualInterface == ControlProtocol.CALL_REQUEST)
                        {
                            Console.WriteLine("Receive call request for "+packet.resourceIdentifier+" on " + ControlProtocol.CALL_REQUEST_ACCEPT + " interface");
                            Console.WriteLine("Send directory request");//sprawdzenie czy w naszej domenie 
                            Console.WriteLine("Receive local name");
                            Console.WriteLine("Send policy out");
                            Console.WriteLine("Call accept");
                            Console.WriteLine("Send call indication or network call coordination out ?");
                            Console.WriteLine("Call accept");
                            Console.WriteLine("Send connection request (to CC) ");
                            Console.WriteLine("receive virtual port + slot ? (from CC) ");
                            //bla bla bla
                            //send(ControlProtocol.CALL_ACCEPT, 1, packet.resourceIdentifier, 1, 3);
                            Console.WriteLine("Send cos tambajsdh");


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
            ControlPacket packet = new ControlPacket(ControlProtocol.CALL_REQUEST, 0, resourceIdentifier, virtualPort, slot);
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);
            
        }

    }
}
