using ClientWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCCRC
{
    class RoutingController
    {
        private TcpListener listener;

        private Thread threadLRMListen;
        private Thread threadConsole;

        private BinaryWriter writer;

        private Dictionary<String, Dictionary<String, int>> topologyVC31;
        private Dictionary<String, Dictionary<String, int>> topologyVC32;
        private Dictionary<String, Dictionary<String, int>> topologyVC33;

        public RoutingController(string[] args)
        {
            topologyVC31 = new Dictionary<String, Dictionary<String, int>>();
            topologyVC32 = new Dictionary<String, Dictionary<String, int>>();
            topologyVC33 = new Dictionary<String, Dictionary<String, int>>();
            this.listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[0]));
            this.threadLRMListen = new Thread(new ThreadStart(lrmListening));
            threadLRMListen.Start();

            consoleStart();
        }

        private void lrmListening()
        {
            this.listener.Start();

            Boolean noError = true;
            while (noError)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(lrmThread));
                    clientThread.Start(client);
                }
                catch(SocketException sEx)
                {
                    consoleWriter("[ERROR] Socket failed. Listener.");
                    noError = false;
                }
            }
        }

        public void lrmThread(Object lrm)
        {
            TcpClient lrmClient = (TcpClient)lrm;
            BinaryReader reader = new BinaryReader(lrmClient.GetStream());
            writer = new BinaryWriter(lrmClient.GetStream());
            String nodeName = null;

            Boolean noError = true;
            while(noError)
            {
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type != typeof(ControlSignalingMessage))
                    noError = false;

                ControlSignalingMessage msg = received_object.Value.ToObject<ControlSignalingMessage>();
                switch(msg.HeaderField)
                {
                    case ControlSignalingMessage.Header.INIT:
                        nodeName = msg.ClientAddress;
                        break;
                    case ControlSignalingMessage.Header.TOPOLOGY:
                        List<String> connectedNodes = msg.ConnectedNodes;
                        Dictionary<String, int> connectionWithWights = new Dictionary<string, int>();
                        foreach (String node in connectedNodes)
                        {
                            if (!connectionWithWights.Keys.Contains(node))
                                connectionWithWights.Add(node, 1);
                        }
                        topologyVC31.Add(nodeName, connectionWithWights);
                        topologyVC32.Add(nodeName, connectionWithWights);
                        topologyVC33.Add(nodeName, connectionWithWights);
                        break;
                    case ControlSignalingMessage.Header.SOMEONE_DIED:
                        String whoDied = msg.WhoDied;
                        topologyVC31.Remove(whoDied);
                        topologyVC32.Remove(whoDied);
                        topologyVC33.Remove(whoDied);
                        foreach (var item in topologyVC31.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                            item.Value.Remove(whoDied);
                        foreach (var item in topologyVC32.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                            item.Value.Remove(whoDied);
                        foreach (var item in topologyVC33.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                            item.Value.Remove(whoDied);
                        break;
                }
            }
        }

        public List<String> shortest_path(String start, String finish, ref Dictionary<String, Dictionary<String, int>> topology)
        {
            Dictionary<String, String> previous = new Dictionary<String, String>();
            Dictionary<String, int> distances = new Dictionary<String, int>();
            List<String> nodes = new List<String>();

            List<String> path = null;

            foreach (var vertex in topology)
            {
                if (vertex.Key == start)
                    distances[vertex.Key] = 0;
                else
                    distances[vertex.Key] = int.MaxValue;

                nodes.Add(vertex.Key);
            }

            while (nodes.Count != 0)
            {
                nodes.Sort((x, y) => distances[x] - distances[y]);

                String smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest == finish)
                {
                    path = new List<String>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }

                    break;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in topology[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }
            }

            return path;
        }

        private void consoleStart()
        {
            consoleWriter("[INIT] RC started.");
        }

      


        private void consoleWriter(String msg)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:" + msg);
            Console.Write(Environment.NewLine);
        }

    }
}
