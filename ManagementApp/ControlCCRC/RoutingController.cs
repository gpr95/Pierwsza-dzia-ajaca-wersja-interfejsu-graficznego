using ClientWindow;
using ControlCCRC.Protocols;
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
        private Boolean iAmDomain;

        private TcpListener LRMAndRCListener;
        private TcpClient RCClient;

        private Thread threadListenLRMAndRC;
        private Thread threadconnectRC;

        private Dictionary<String, LRMRCThread> threadsMap;
        private Dictionary<String, Dictionary<String, int>> topologyVC31;
        private Dictionary<String, Dictionary<String, int>> topologyVC32;
        private Dictionary<String, Dictionary<String, int>> topologyVC33;

        /**
         * DOMAIN [listen LRM_AND_RC]
         * SUBNETWORK [listen LRM_AND_RC , connect up RC] 
         */
        public RoutingController(string[] args)
        {
            iAmDomain = (args.Length == 1);
            
            if(!iAmDomain)
            {
                consoleWriter("[INIT] SUBNETWORK");
                try
                {
                    RCClient = new TcpClient("localhost", Convert.ToInt32(args[1]));
                }
                catch (SocketException ex)
                {
                    consoleWriter("[ERROR] Cannot connect with upper RC.");
                }
                this.threadconnectRC = new Thread(new ThreadStart(rcConnecting));
                threadconnectRC.Start();
            }
            else
                consoleWriter("[INIT] DOMAIN");


            topologyVC31 = new Dictionary<String, Dictionary<String, int>>();
            topologyVC32 = new Dictionary<String, Dictionary<String, int>>();
            topologyVC33 = new Dictionary<String, Dictionary<String, int>>();

            this.LRMAndRCListener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[0]));
            this.threadListenLRMAndRC = new Thread(new ThreadStart(lrmAndRcListening));
            threadListenLRMAndRC.Start();

            consoleStart();
        }

        private void rcConnecting()
        {
            BinaryReader reader = new BinaryReader(RCClient.GetStream());

            Boolean noError = true;
            while (noError)
            {
                try
                {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type != typeof(RCtoRCSignallingMessage))
                        noError = false;
                    RCtoRCSignallingMessage msg = received_object.Value.ToObject<RCtoRCSignallingMessage>();
                    //@TODO something with Msg
                }
                catch (IOException ex)
                {
                    noError = false;
                }
            }
        }

        private void lrmAndRcListening()
        {
            this.LRMAndRCListener.Start();

            Boolean noError = true;
            while (noError)
            {
                try
                {
                    TcpClient client = LRMAndRCListener.AcceptTcpClient();
                    LRMRCThread thread = new LRMRCThread(client, ref threadsMap, ref topologyVC31, ref topologyVC32, ref topologyVC33);
                }
                catch (SocketException sEx)
                {
                    consoleWriter("[ERROR] Socket failed. Listener.");
                    noError = false;
                }
            }
        }

        public void findPath(String startNode, String endNode, int howMuchVC3)
        {
            consoleWriter("[CC] Sended info to make path between: " + startNode + " and " + endNode + " with:" 
                + howMuchVC3 + "x VC-3");
            switch (howMuchVC3)
            {
                case 1:
                    int whichTopology = 1;
                    List<String> path = shortest_path(startNode, endNode, ref topologyVC31);
                    if (path == null || !path.First().Equals(startNode) || !path.Last().Equals(endNode))
                    { 
                        path = shortest_path(startNode, endNode, ref topologyVC32);
                        whichTopology = 2;
                    }
                    if (path == null || !path.First().Equals(startNode) || !path.Last().Equals(endNode))
                    {
                        path = shortest_path(startNode, endNode, ref topologyVC33);
                        whichTopology = 3;
                    }

                    if (path != null || path.First().Equals(startNode) || path.Last().Equals(endNode))
                    {
                        consoleWriter("[INFO] Shortest path : " + path);
                        switch(whichTopology)
                        {
                            case 1:
                                /** Builded path in 1st layer */
                                for(int i=0;i<path.Count - 1;i++)
                                    topologyVC31[path[i]].Remove(path[i + 1]);
                                break;
                            case 2:
                                /** Builded path in 2nd layer */
                                for (int i = 0; i < path.Count - 1; i++)
                                    topologyVC32[path[i]].Remove(path[i + 1]);
                                break;
                            case 3:
                                /** Builded path in 3th layer */
                                for (int i = 0; i < path.Count - 1; i++)
                                    topologyVC33[path[i]].Remove(path[i + 1]);
                                break;
                        }
                    }
                        break;
                case 2:
                    List<String> path1 = shortest_path(startNode, endNode, ref topologyVC31);
                    List<String> path2 = shortest_path(startNode, endNode, ref topologyVC32);
                    List<String> path3 = shortest_path(startNode, endNode, ref topologyVC33);
                    if(path1 != null && path1.First().Equals(startNode) && path1.Last().Equals(endNode) &&
                       path2 != null && path2.First().Equals(startNode) && path2.Last().Equals(endNode))
                    {
                        /** Builded path in 1st layer */
                        /** Builded path in 2nd layer */
                        for (int i = 0; i < path1.Count - 1; i++)
                            topologyVC31[path1[i]].Remove(path1[i + 1]);
                        for (int i = 0; i < path2.Count - 1; i++)
                            topologyVC32[path2[i]].Remove(path2[i + 1]);
                    }
                    else if (path1 != null && path1.First().Equals(startNode) && path1.Last().Equals(endNode) &&
                             path3 != null && path3.First().Equals(startNode) && path3.Last().Equals(endNode))
                    {
                        /** Builded path in 1st layer */
                        /** Builded path in 3nd layer */
                        for (int i = 0; i < path1.Count - 1; i++)
                            topologyVC31[path1[i]].Remove(path1[i + 1]);
                        for (int i = 0; i < path3.Count - 1; i++)
                            topologyVC33[path3[i]].Remove(path3[i + 1]);
                    }
                    else if (path2 != null && path2.First().Equals(startNode) && path2.Last().Equals(endNode) &&
                             path3 != null && path3.First().Equals(startNode) && path3.Last().Equals(endNode))
                    {
                        /** Builded path in 2nd layer */
                        /** Builded path in 3nd layer */
                        for (int i = 0; i < path2.Count - 1; i++)
                            topologyVC32[path2[i]].Remove(path2[i + 1]);
                        for (int i = 0; i < path3.Count - 1; i++)
                            topologyVC33[path3[i]].Remove(path3[i + 1]);
                    }

                    break;
                case 3:
                    List<String> path31 = shortest_path(startNode, endNode, ref topologyVC31);
                    List<String> path32 = shortest_path(startNode, endNode, ref topologyVC32);
                    List<String> path33 = shortest_path(startNode, endNode, ref topologyVC33);
                    if (path31 != null && path31.First().Equals(startNode) && path31.Last().Equals(endNode) &&
                       path32 != null && path32.First().Equals(startNode) && path32.Last().Equals(endNode) &&
                       path33 != null && path33.First().Equals(startNode) && path33.Last().Equals(endNode))
                    {
                        /** Builded path in 1st layer */
                        /** Builded path in 2nd layer */
                        /** Builded path in 3nd layer */
                    }

                    break;
                default:
                    consoleWriter("[ERROR] Wrong VC-3 number");
                    break;
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


    class LRMRCThread
    {
        private String nodeName;
        private Thread thread;
        private BinaryWriter writer;

        /** Hadnlers */
        private Dictionary<String, LRMRCThread> threadsMap;
        private Dictionary<String, Dictionary<String, int>> topologyVC31;
        private Dictionary<String, Dictionary<String, int>> topologyVC32;
        private Dictionary<String, Dictionary<String, int>> topologyVC33;

        public LRMRCThread(TcpClient connection, ref Dictionary<String, LRMRCThread> threadsMap,ref Dictionary<String, Dictionary<String, int>> topologyVC31,
          ref  Dictionary<String, Dictionary<String, int>> topologyVC32,ref Dictionary<String, Dictionary<String, int>> topologyVC33)
        {
            this.threadsMap = threadsMap;
            this.topologyVC31 = topologyVC31;
            this.topologyVC32 = topologyVC32;
            this.topologyVC33 = topologyVC33;
            thread = new Thread(new ParameterizedThreadStart(lrmThread));
            thread.Start(connection);
        }


        public void lrmThread(Object lrm)
        {
            TcpClient lrmClient = (TcpClient)lrm;
            BinaryReader reader = new BinaryReader(lrmClient.GetStream());
            writer = new BinaryWriter(lrmClient.GetStream());
            Boolean noError = true;
            Boolean lrmConnection = false;
            while (noError)
            {
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(RCtoLRMSignallingMessage))
                    lrmConnection = true;
                else if (received_object.Type == typeof(RCtoRCSignallingMessage))
                    lrmConnection = false;
                else
                {
                    consoleWriter("[ERROR] Received wrong data format.");
                    return;
                }

                if(lrmConnection)
                {
                    RCtoLRMSignallingMessage lrmMsg = received_object.Value.ToObject<RCtoLRMSignallingMessage>();
                    switch(lrmMsg.State)
                    {
                        case RCtoLRMSignallingMessage.LRM_INIT:
                            nodeName = lrmMsg.NodeName;
                            topologyVC31.Add(nodeName, new Dictionary<string, int>());
                            topologyVC32.Add(nodeName, new Dictionary<string, int>());
                            topologyVC33.Add(nodeName, new Dictionary<string, int>());
                            threadsMap.Add(nodeName, this);
                            break;
                        //@TODO!
                        case RCtoLRMSignallingMessage.LRM_TOPOLOGY_ADD:
                            topologyVC31[nodeName].Add(lrmMsg.ConnectedNode, 1);
                            topologyVC32[nodeName].Add(lrmMsg.ConnectedNode, 1);
                            topologyVC33[nodeName].Add(lrmMsg.ConnectedNode, 1);
                            break;
                        case RCtoLRMSignallingMessage.LRM_TOPOLOGY_DELETE:
                            String whoDied = lrmMsg.ConnectedNode;
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
                else
                {
                    RCtoRCSignallingMessage lrmMsg = received_object.Value.ToObject<RCtoRCSignallingMessage>();
                    //@TODO
                }
                ControlSignalingMessage msg = received_object.Value.ToObject<ControlSignalingMessage>();
                switch (msg.HeaderField)
                {
                    case ControlSignalingMessage.Header.INIT:
                        nodeName = msg.ClientAddress;
                        threadsMap.Add(nodeName, this);
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


        private void consoleWriter(String msg)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:" + msg);
            Console.Write(Environment.NewLine);
        }
    }
}
