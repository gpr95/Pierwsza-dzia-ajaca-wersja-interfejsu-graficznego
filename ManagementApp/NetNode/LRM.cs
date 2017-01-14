using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using ClientWindow;
using NetNode;

namespace NetNode
{
    class LRM
    {
        //TODO send from all ports signal with protocol whoyouare?
        //receive and interpret ip of neighbours and for now print it then send to RC
        
        public static BinaryWriter writer;
        private Timer timerForsending;
        private string virtualIp;
        private Dictionary<int, string> connections;

        public LRM(string virtualIp)
        {
            this.virtualIp = virtualIp;
            this.connections = new Dictionary<int, string>();

            //Timer
            timerForsending = new Timer();
            timerForsending.Elapsed += new ElapsedEventHandler(sendMessage);
            timerForsending.Interval = 10000; //10 seconds
            timerForsending.Enabled = true;
        }

        public void receivedMessage(string lrmProtocol, int port)
        {
            string[] temp = lrmProtocol.Split(' ');
            if (temp[1] != this.virtualIp)
            {
                if (temp[0] == "whoyouare")
                {
                    //Console.WriteLine("received: "+temp[0] + " from " + temp[1]);
                    this.saveConnection(port, temp[1]);
                }
            }
        }

        public void sendMessage(object sender, EventArgs e)
        {
            for (int i = 0; i < 21; i++)
            {
                string message = "whoyouare " + this.virtualIp;
                int port = i;
                Signal signal = new Signal(port, message);
                string data = JMessage.Serialize(JMessage.FromValue(signal));
                writer.Write(data);
            }
        }

        private void saveConnection(int port, string virtualIp)
        {
            if(!this.connections.ContainsKey(port))
            {
                Console.WriteLine("I am connected with " + virtualIp + " on port " + port);
                this.connections.Add(port, virtualIp);
                //TODO send to RC
            }
            else
            {
                //Console.WriteLine("connections already stored");
            }
        }
    }
}
