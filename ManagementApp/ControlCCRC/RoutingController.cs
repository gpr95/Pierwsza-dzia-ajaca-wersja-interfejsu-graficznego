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

        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer1;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer2;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer3;

        private Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer1;
        private Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer2;
        private Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer3;

        private List<String> latestBuildedPathLayer1;
        private List<String> latestBuildedPathLayer2;
        private List<String> latestBuildedPathLayer3;

        private ConnectionController ccHandler;

     

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


            topologyUnallocatedLayer1 = new Dictionary<String, Dictionary<String, int>>();
            topologyUnallocatedLayer2 = new Dictionary<String, Dictionary<String, int>>();
            topologyUnallocatedLayer3 = new Dictionary<String, Dictionary<String, int>>();
            topologyAllocatedLayer1 = new Dictionary<String, Dictionary<String, int>>();
            topologyAllocatedLayer2 = new Dictionary<String, Dictionary<String, int>>();
            topologyAllocatedLayer3 = new Dictionary<String, Dictionary<String, int>>();

            this.LRMAndRCListener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[0]));
            this.threadListenLRMAndRC = new Thread(new ThreadStart(lrmAndRcListening));
            threadListenLRMAndRC.Start();

            consoleStart();
        }

        public void setCCHandler(ConnectionController cc)
        {
            this.ccHandler = cc;
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
                    LRMRCThread thread = new LRMRCThread(client, ref threadsMap,
                        ref topologyUnallocatedLayer1, ref topologyUnallocatedLayer2, ref topologyUnallocatedLayer3,
                        ref topologyAllocatedLayer1, ref topologyAllocatedLayer2, ref topologyAllocatedLayer3);
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
            latestBuildedPathLayer1 = null;
            latestBuildedPathLayer2 = null;
            latestBuildedPathLayer3 = null;
            switch (howMuchVC3)
            {
                case 1:
                    int whichTopology = 1;
                    List<String> pathRate1 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer1);
                    if (pathRate1 == null || !pathRate1.First().Equals(startNode) || !pathRate1.Last().Equals(endNode))
                    { 
                        pathRate1 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer2);
                        whichTopology = 2;
                    }
                    if (pathRate1 == null || !pathRate1.First().Equals(startNode) || !pathRate1.Last().Equals(endNode))
                    {
                        pathRate1 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer3);
                        whichTopology = 3;
                    }

                    if (pathRate1 != null || pathRate1.First().Equals(startNode) || pathRate1.Last().Equals(endNode))
                    {
                        consoleWriter("[INFO] Shortest path : " + pathRate1);
                        switch(whichTopology)
                        {
                            case 1:
                                /** Builded path in 1st layer */
                                for (int i = 0; i < pathRate1.Count - 1; i++)
                                {
                                    topologyUnallocatedLayer1[pathRate1[i]].Remove(pathRate1[i + 1]);
                                    topologyAllocatedLayer1[pathRate1[i]].Add(pathRate1[i + 1],1);
                                }
                                latestBuildedPathLayer1 = pathRate1;
                                break;
                            case 2:
                                /** Builded path in 2nd layer */
                                for (int i = 0; i < pathRate1.Count - 1; i++)
                                {
                                    topologyUnallocatedLayer2[pathRate1[i]].Remove(pathRate1[i + 1]);
                                    topologyAllocatedLayer2[pathRate1[i]].Add(pathRate1[i + 1], 1);
                                }
                                latestBuildedPathLayer2 = pathRate1;
                                break;
                            case 3:
                                /** Builded path in 3th layer */
                                for (int i = 0; i < pathRate1.Count - 1; i++)
                                {
                                    topologyUnallocatedLayer3[pathRate1[i]].Remove(pathRate1[i + 1]);
                                    topologyAllocatedLayer3[pathRate1[i]].Add(pathRate1[i + 1], 1);
                                }
                                latestBuildedPathLayer3 = pathRate1;
                                break;
                        }
                    }
                    else
                    {
                        consoleWriter("[INFO] NOT able to connect nodes. All paths allocated.");
                    }
                    break;
                case 2:
                    // TODO obsluga
                    List<String> path1 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer1);
                    List<String> path2 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer2);
                    List<String> path3 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer3);
                    if(path1 != null && path1.First().Equals(startNode) && path1.Last().Equals(endNode) &&
                       path2 != null && path2.First().Equals(startNode) && path2.Last().Equals(endNode))
                    {
                        /** Builded path in 1st layer */
                        /** Builded path in 2nd layer */
                        for (int i = 0; i < path1.Count - 1; i++)
                            topologyUnallocatedLayer1[path1[i]].Remove(path1[i + 1]);
                        for (int i = 0; i < path2.Count - 1; i++)
                            topologyUnallocatedLayer2[path2[i]].Remove(path2[i + 1]);
                    }
                    else if (path1 != null && path1.First().Equals(startNode) && path1.Last().Equals(endNode) &&
                             path3 != null && path3.First().Equals(startNode) && path3.Last().Equals(endNode))
                    {
                        /** Builded path in 1st layer */
                        /** Builded path in 3nd layer */
                        for (int i = 0; i < path1.Count - 1; i++)
                            topologyUnallocatedLayer1[path1[i]].Remove(path1[i + 1]);
                        for (int i = 0; i < path3.Count - 1; i++)
                            topologyUnallocatedLayer3[path3[i]].Remove(path3[i + 1]);
                    }
                    else if (path2 != null && path2.First().Equals(startNode) && path2.Last().Equals(endNode) &&
                             path3 != null && path3.First().Equals(startNode) && path3.Last().Equals(endNode))
                    {
                        /** Builded path in 2nd layer */
                        /** Builded path in 3nd layer */
                        for (int i = 0; i < path2.Count - 1; i++)
                            topologyUnallocatedLayer2[path2[i]].Remove(path2[i + 1]);
                        for (int i = 0; i < path3.Count - 1; i++)
                            topologyUnallocatedLayer3[path3[i]].Remove(path3[i + 1]);
                    }

                    break;
                case 3:
                    // TODO obsluga
                    List<String> path31 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer1);
                    List<String> path32 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer2);
                    List<String> path33 = shortest_path(startNode, endNode, ref topologyUnallocatedLayer3);
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

        public List<string> LatestBuildedPathLayer1
        {
            get
            {
                return latestBuildedPathLayer1;
            }

            set
            {
                latestBuildedPathLayer1 = value;
            }
        }

        public List<string> LatestBuildedPathLayer2
        {
            get
            {
                return latestBuildedPathLayer2;
            }

            set
            {
                latestBuildedPathLayer2 = value;
            }
        }

        public List<string> LatestBuildedPathLayer3
        {
            get
            {
                return latestBuildedPathLayer3;
            }

            set
            {
                latestBuildedPathLayer3 = value;
            }
        }

    }


    class LRMRCThread
    {
        private String nodeName;
        private Thread thread;
        private BinaryWriter writer;

        /** Hadnlers */
        private Dictionary<String, LRMRCThread> threadsMap;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer1;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer2;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer3;

        private Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer1;
        private Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer2;
        private Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer3;

        public LRMRCThread(TcpClient connection, 
            ref Dictionary<String, LRMRCThread> threadsMap,
            ref Dictionary<String,Dictionary<String, int>> topologyUnallocatedLayer1,
          ref  Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer2,
          ref Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer3,
            ref Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer1,
          ref Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer2,
          ref Dictionary<String, Dictionary<String, int>> topologyAllocatedLayer3)
        {
            this.threadsMap = threadsMap;
            this.topologyUnallocatedLayer1 = topologyUnallocatedLayer1;
            this.topologyUnallocatedLayer2 = topologyUnallocatedLayer2;
            this.topologyUnallocatedLayer3 = topologyUnallocatedLayer3;
            this.topologyAllocatedLayer1 = topologyAllocatedLayer1;
            this.topologyAllocatedLayer2 = topologyAllocatedLayer2;
            this.topologyAllocatedLayer3 = topologyAllocatedLayer3;
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
                            topologyUnallocatedLayer1.Add(nodeName, new Dictionary<string, int>());
                            topologyUnallocatedLayer2.Add(nodeName, new Dictionary<string, int>());
                            topologyUnallocatedLayer3.Add(nodeName, new Dictionary<string, int>());
                            topologyAllocatedLayer1.Add(nodeName, new Dictionary<string, int>());
                            topologyAllocatedLayer2.Add(nodeName, new Dictionary<string, int>());
                            topologyAllocatedLayer3.Add(nodeName, new Dictionary<string, int>());
                            threadsMap.Add(nodeName, this);
                            break;
                        case RCtoLRMSignallingMessage.LRM_TOPOLOGY_ADD:
                            topologyUnallocatedLayer1[nodeName].Add(lrmMsg.ConnectedNode, 1);
                            topologyUnallocatedLayer2[nodeName].Add(lrmMsg.ConnectedNode, 1);
                            topologyUnallocatedLayer3[nodeName].Add(lrmMsg.ConnectedNode, 1);
                            break;
                        case RCtoLRMSignallingMessage.LRM_TOPOLOGY_DELETE:
                            String whoDied = lrmMsg.ConnectedNode;
                            topologyUnallocatedLayer1.Remove(whoDied);
                            topologyUnallocatedLayer2.Remove(whoDied);
                            topologyUnallocatedLayer3.Remove(whoDied);
                            topologyAllocatedLayer1.Remove(whoDied);
                            topologyAllocatedLayer2.Remove(whoDied);
                            topologyAllocatedLayer3.Remove(whoDied);
                            foreach (var item in topologyUnallocatedLayer1.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                                item.Value.Remove(whoDied);
                            foreach (var item in topologyUnallocatedLayer2.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                                item.Value.Remove(whoDied);
                            foreach (var item in topologyUnallocatedLayer3.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                                item.Value.Remove(whoDied);
                            foreach (var item in topologyAllocatedLayer1.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                                item.Value.Remove(whoDied);
                            foreach (var item in topologyAllocatedLayer2.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                                item.Value.Remove(whoDied);
                            foreach (var item in topologyAllocatedLayer3.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                                item.Value.Remove(whoDied);
                            break;                           
                    }
                }
                else
                {
                    RCtoRCSignallingMessage lrmMsg = received_object.Value.ToObject<RCtoRCSignallingMessage>();
                    //@TODO
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
