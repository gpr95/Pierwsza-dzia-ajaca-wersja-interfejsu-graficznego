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
        private Timer timerForSending;
        private Timer timerForConf;
        private string virtualIp;
        public static Dictionary<int, string> connections = new Dictionary<int,string>();
        private static Dictionary<int, bool> confirmations = new Dictionary<int, bool>();
        //private static Dictionary<int, Dictionary<int, bool>> resources = new Dictionary<int, Dictionary<int,bool>>();
        public static List<Resource> resources = new List<Resource>();

        public LRM(string virtualIp)
        {
            this.virtualIp = virtualIp;
            initResources(resources);

            //Timer
            timerForSending = new Timer();
            timerForSending.Elapsed += new ElapsedEventHandler(sendMessage);
            timerForSending.Interval = 10000; //10 seconds
            timerForSending.Enabled = true;

            //Timer
            timerForConf = new Timer();
            timerForConf.Elapsed += new ElapsedEventHandler(confirmAlive);
            timerForConf.Interval = 10000; //10 seconds
            timerForConf.Enabled = true;
        }

        private void initResources(List<Resource> resources)
        {
            for(int i=0;i<21;i++)
            {
                for(int j=11;j<=13;j++)
                {
                    resources.Add(new Resource(i,j,false));
                }
            }
            ControlAgent.sendTopologyInit(this.virtualIp);
        }

        public void receivedMessage(string lrmProtocol, int port)
        {
            string[] temp = lrmProtocol.Split(' ');
            if (temp[1] != this.virtualIp)
            {
                if (temp[0] == "iam")
                {
                    //Console.WriteLine("received: "+temp[0] + " from " + temp[1]+"on port: "+port);
                    this.saveConnection(port, temp[1]);
                    confirmations[port] = false;
                }
                else if (temp[0] == "whoyouare")
                {
                    this.sendMessageToOne(port,temp[1]);
                }
            }
        }

        private void sendMessageToOne(int port, string from)
        {
            string message = "iam " + this.virtualIp;
            //Console.WriteLine("message " + message + "virt port: " + port);
            Signal signal = new Signal(port, message);
            string data = JMessage.Serialize(JMessage.FromValue(signal));
            writer.Write(data);
        }

        public void sendMessage(object sender, EventArgs e)
        {
            for (int i = 0; i < 21; i++)
            {
                string message = "whoyouare " + this.virtualIp;
                //Console.WriteLine("message " + message + "virt port: " + i);
                Signal signal = new Signal(i, message);
                string data = JMessage.Serialize(JMessage.FromValue(signal));

                if (!confirmations.ContainsKey(i))
                {
                    confirmations.Add(i, true);
                }
                else
                {
                    confirmations[i] = true;
                }

                writer.Write(data);
            }
        }

        private void saveConnection(int port, string virtualIp)
        {
            if(!connections.ContainsKey(port))
            {
                Console.WriteLine("I am connected with " + virtualIp + " on port " + port);
                connections.Add(port, virtualIp);
                //send to RC e.g. NN0 connected on port 2 with NN1
                ControlAgent.sendTopology(this.virtualIp, port, virtualIp);
            }
        }

        private void confirmAlive(object sender, EventArgs e)
        {
            foreach (var i in connections)
            {
                if(confirmations[i.Key] == true)
                {
                    //znaczy ze nie ma polaczenia
                    Console.WriteLine("connection lost no keepalive from neighbour on port " + i.Key);
                    connections.Remove(i.Key);
                    //inform RC that row is deleted
                    ControlAgent.sendDeleted(this.virtualIp, i.Key, i.Value);
                    clearResources(i.Key);
                }
            }
        }

        public static int allocateResourceAmount(int port, int amount)
        {
            int no_vc3 = 0;
            int count = 0;
            if(checkResources(port, amount))
            {
                foreach (var res in resources)
                {
                    if (res.port == port && res.status == false && count < amount)
                    {
                        Console.WriteLine("Allocating on port: " + res.port + " vc3: " + res.no_vc3);
                        resources.Where(d => d.port == res.port && d.no_vc3 == res.no_vc3).First().status = true;
                        no_vc3 = res.no_vc3;
                        count++;
                    }
                }
                return no_vc3;
            }
            else
            {
                return 0;
            }
        }

        public static bool allocateResource(int port, int no_vc3)
        {
                if (resources[port].no_vc3 == no_vc3 && resources[port].status == false )
                {
                    //empty so allocating
                    Console.WriteLine("Allocating on port: " + port + "vc3: " + no_vc3);
                    resources[port].status = true;
                    return true;
                }
            return false;
        }

        private static bool checkResources(int port, int amount)
        {
            int counter=0;
            foreach (var res in resources)
            {
                if (res.port == port && res.status == false)
                {
                    counter++;
                }
            }
            if (counter >= amount)
                return true;
            else
                return false;
        }

        private void clearResources(int port)
        {
            foreach (var res in resources)
            {
                if(res.port == port)
                {
                    res.status = false;
                }
            }
            Console.WriteLine("resources cleared");
        }

        public static Dictionary<int,string> getConn()
        {
            return connections;
        }

        public static void printConn()
        {
            foreach (var temp in connections)
            {
                NetNode.log("port: " + temp.Key + " node: " + temp.Value, ConsoleColor.Yellow);
            }
        }

        public static void printResources()
        {
            foreach (var res in resources)
            {
                NetNode.log(res.toString(), ConsoleColor.DarkYellow);
            }
        }
    }
}
