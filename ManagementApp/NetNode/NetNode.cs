using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetNode
{
    class NetNode
    {
        public IPAddress ip;
        public SwitchingField switchField;
        public Ports ports;

        public NetNode(IPAddress ip)
        {
            this.ip = ip;
            //TODO readConfig()
            this.ports = new Ports();
            this.switchField = new SwitchingField();
        }

        static void Main(string[] args)
        {
            //main for testing netNode
            Console.WriteLine("NetNode");
            NetNode netnode = new NetNode(IPAddress.Parse("192.168.56.1"));
            
            //symuluje odebranie do portu wiadomosci
            TcpClient temp = new TcpClient();
            temp.Connect(netnode.ip, 1234);
            BinaryWriter writeOutput = new BinaryWriter(temp.GetStream());
            Packet packet = new Packet();
            packet.sourceAddress = "192.168.1.12";
            packet.message = "tralalala";
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writeOutput.Write(data);
            temp.Close();

            //sprawdza czy sa jakies pakiety w kolejkach w portach wejsciowych
            while(true)
            {
                foreach(IPort iport in netnode.ports.iports)
                {
                    //check if there is packet in queue and try to process it 
                    if(iport.input.Count > 0)
                    {
                        Packet pack = iport.input.Dequeue();
                        int oport = netnode.switchField.commutePacket(pack, iport.port, pack.sourceAddress);
                        netnode.ports.oports[oport].addToOutQueue(packet);
                        Console.ReadLine();
                    }
                }
            }
        }
    }
}
