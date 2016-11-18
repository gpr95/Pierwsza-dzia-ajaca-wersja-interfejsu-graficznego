using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetNode
{
    //output port
    class OPort
    {
        private int port;
        public Queue<Packet> output = new Queue<Packet>();

        public OPort(int port)
        {
            this.port = port;
        }

        public void addToOutQueue(Packet packet)
        {
            this.output.Enqueue(packet);
            sendData(packet);
        }

        public void sendData(Packet packet)
        {
            TcpClient temp = new TcpClient();
            temp.Connect(IPAddress.Parse("192.168.56.1"), 1234+port);
            BinaryWriter writeOutput = new BinaryWriter(temp.GetStream());
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writeOutput.Write(data);
            this.output.Dequeue();
        }
    }
}
