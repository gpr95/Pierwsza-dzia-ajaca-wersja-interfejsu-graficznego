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
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(ControlPacket))
                {
                    ControlPacket packet = received_object.Value.ToObject<ControlPacket>();
                    Console.WriteLine(packet.virtualInterface);

                }else
                {
                    Console.WriteLine("Wrong control packet format");
                }
            }
        }

    }
}
