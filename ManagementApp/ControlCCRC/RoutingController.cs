using ClientWindow;
using ControlCCRC.Protocols;
using Management;
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
        private String identifier;

        private TcpClient RCClient;
        private Thread threadconnectRC;

        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer1;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer2;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer3;
        private Dictionary<String, Dictionary<String, int>> wholeTopologyNodesAndConnectedNodesWithPorts;

        private List<String> pathNeededToBeCount;
        private String upperRc;

        private ConnectionController ccHandler;
        private Dictionary<String, BinaryWriter> socketHandler;



        /**
        * DOMAIN [RC_ID]
        * SUBNETWORK [RC_ID, connect up RC] 
        */
        public RoutingController(string[] args)
        {
            iAmDomain = (args.Length == 1);
            identifier = args[0];

            if (!iAmDomain)
            {
                consoleWriter("[INIT] SUBNETWORK");
                try
                {
                    int rccId;
                    int.TryParse(args[0], out rccId);
                    RCClient = new TcpClient("localhost", rccId);
                }
                catch (SocketException ex)
                {
                    consoleWriter("[ERROR] Cannot connect with upper RC.");
                }
                this.threadconnectRC = new Thread(new ThreadStart(rcConnecting));
                threadconnectRC.Start();
            }
            else
            {
                consoleWriter("[INIT] DOMAIN");
                identifier = "DOMAIN_" + identifier;
            }


            topologyUnallocatedLayer1 = new Dictionary<String, Dictionary<String, int>>();
            topologyUnallocatedLayer2 = new Dictionary<String, Dictionary<String, int>>();
            topologyUnallocatedLayer3 = new Dictionary<String, Dictionary<String, int>>();
            wholeTopologyNodesAndConnectedNodesWithPorts = new Dictionary<string, Dictionary<string, int>>();

            consoleStart();
        }

        public void setCCHandler(ConnectionController cc)
        {
            this.ccHandler = cc;
        }

        public void setSocketHandler(Dictionary<String, BinaryWriter> socketHandler)
        {
            this.socketHandler = socketHandler;
        }

        private void rcConnecting()
        {
            BinaryReader reader = new BinaryReader(RCClient.GetStream());
            BinaryWriter writer = new BinaryWriter(RCClient.GetStream());



            RCtoRCSignallingMessage initMsg = new RCtoRCSignallingMessage();
            initMsg.Identifier = identifier;
            String send_object = JMessage.Serialize(JMessage.FromValue(initMsg));
            writer.Write(send_object);

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
                    switch(msg.State)
                    {
                        case RCtoRCSignallingMessage.COUNT_ALL_PATHS:
                            pathNeededToBeCount = msg.AllUpperNodesToCountWeights;
                            upperRc = msg.Identifier;
                            if(socketHandler.Keys.Where(id => id.StartsWith("RC")).Count() > 0)
                                foreach (String id in socketHandler.Keys.Where(id => id.StartsWith("RC")))
                                {
                                    RCtoRCSignallingMessage countPathsMsg = new RCtoRCSignallingMessage();
                                    countPathsMsg.State = RCtoRCSignallingMessage.COUNT_ALL_PATHS;
                                    countPathsMsg.AllUpperNodesToCountWeights = wholeTopologyNodesAndConnectedNodesWithPorts.Keys.ToList();
                                    String dataToSend = JMessage.Serialize(JMessage.FromValue(countPathsMsg));
                                    socketHandler[id].Write(dataToSend);
                                }
                            else
                            {
                                RCtoRCSignallingMessage countedPathValue = new RCtoRCSignallingMessage();

                                Dictionary<String, String> pairs;
                                Dictionary<String, Dictionary<String, int>> nodeConnectionsAndWeights = new Dictionary<string, Dictionary<string, int>>();
                                for (int i=0; i< pathNeededToBeCount.Count(); i++)
                                {
                                    List<String> others = pathNeededToBeCount
                                        .Where(node => !node.Equals(pathNeededToBeCount[i])).ToList();

                                    Dictionary<String, int> connections = new Dictionary<string, int>();
                                    foreach (String s in others)
                                    {
                                        if (findWeightBetweenTwoNodes(pathNeededToBeCount[i], s, msg.RateToCountWeights) != 0)
                                            connections.Add(s, findWeightBetweenTwoNodes(pathNeededToBeCount[i], s, msg.RateToCountWeights));
                                    }

                                    nodeConnectionsAndWeights.Add(pathNeededToBeCount[i], connections);
                                }

                                if (nodeConnectionsAndWeights.Count != 0)
                                {
                                    countedPathValue.NodeConnectionsAndWeights = nodeConnectionsAndWeights;
                                    countedPathValue.State = RCtoRCSignallingMessage.COUNTED_ALL_PATHS_CONFIRM;
                                }
                                else
                                {
                                    countedPathValue.State = RCtoRCSignallingMessage.COUNTED_ALL_PATHS_REFUSE;
                                }

                                String dataToSend = JMessage.Serialize(JMessage.FromValue(countedPathValue));
                                writer.Write(dataToSend);
                            }
                            break;
                    }
                }
                catch (IOException ex)
                {
                    noError = false;
                }
            }
        }

    

        public int findWeightBetweenTwoNodes(String startNode, String endNode, int howMuchVC3)
        {
            int result = 0;
            consoleWriter("[RC] Sended info to count weights between: " + startNode + " and " + endNode + " with:"
                + howMuchVC3 + "x VC-3");

            if (wholeTopologyNodesAndConnectedNodesWithPorts
               .Where(node => node.Value.ContainsKey(startNode)) == null)
            {
                return 0;
            }
            if (wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)) == null)
            {
                return 0;
            }

            String firstInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(startNode)).First().Key;

            String lastInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)).First().Key;


            switch (howMuchVC3)
            {
                case 1:
                    if(shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1) != null)
                        result = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1).Count + 2;
                    if(result == 0 && shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer2) != null)
                        result = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer2).Count + 2;
                    if (result == 0 && shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer3) != null)
                        result = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer3).Count + 2;
                    break;
                case 2:
                    int shortest1 = 0;
                    int shortest2 = 0;
                    int shortest3 = 0;

                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer2) != null)
                    {
                        shortest1 = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1).Count + 2;
                    }
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer3) != null)
                    {
                        shortest2 = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1).Count + 2;
                    }
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer2) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer3) != null)
                    {
                        shortest3 = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1).Count + 2;
                    }
                    int[] anArray = { shortest1, shortest2, shortest3};
                    result = anArray.Max();
                    break;
                case 3:
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer2) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer3) != null)
                        result = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1).Count + 2;
                    break;
            }

            return result;
        }

        public Dictionary<String,List<FIB>> findPath(String startNode, String endNode, int howMuchVC3)
        {
            
            consoleWriter("[CC] Sended info to make path between: " + startNode + " and " + endNode + " with:" 
                + howMuchVC3 + "x VC-3");

            Dictionary<String, List<FIB>> result = new Dictionary<string, List<FIB>>();
            String firstInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(startNode)).First().Key;

            String lastInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)).First().Key;


            switch (howMuchVC3)
            {
                case 1:
                    int whichTopology = 1;
                    List<String> pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer1);
                    if (pathRate1 == null || !pathRate1.First().Equals(firstInMyNetwork) || !pathRate1.Last().Equals(lastInMyNetwork))
                    { 
                        pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer2);
                        whichTopology = 2;
                    }
                    if (pathRate1 == null || !pathRate1.First().Equals(firstInMyNetwork) || !pathRate1.Last().Equals(lastInMyNetwork))
                    {
                        pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, ref topologyUnallocatedLayer3);
                        whichTopology = 3;
                    }

                    if (pathRate1 != null || pathRate1.First().Equals(firstInMyNetwork) || pathRate1.Last().Equals(lastInMyNetwork))
                    {
                        consoleWriter("[INFO] Shortest path : " + pathRate1);
                        foreach (String node in pathRate1)
                            result.Add(node, new List<FIB>());
                        result.First().Value.Add(new FIB(
                                            wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][startNode],
                                            whichTopology,
                                            wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][pathRate1[1]],
                                            whichTopology
                                            ));
                        result.Last().Value.Add(new FIB(
                                            wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1.Last()][pathRate1[pathRate1.Count-2]],
                                            whichTopology,
                                            wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1.Last()][endNode],
                                            whichTopology
                                            ));
                        for(int i =0; i< pathRate1.Count; i++)
                        {
                            if (i != 0 && i != pathRate1.Count - 1)
                                result[pathRate1[i]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i - 1]],
                                    1,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i + 1]],
                                    1
                                    ));
                        }

                        switch (whichTopology)
                        {
                            case 1:
                                /** Builded path in 1st layer */
                                for (int i = 0; i < pathRate1.Count - 1; i++)
                                {
                                    topologyUnallocatedLayer1[pathRate1[i]].Remove(pathRate1[i + 1]);
                                }                               
                                break;
                            case 2:
                                /** Builded path in 2nd layer */
                                for (int i = 0; i < pathRate1.Count - 1; i++)
                                {
                                    topologyUnallocatedLayer2[pathRate1[i]].Remove(pathRate1[i + 1]);
                                    if (i != 0 && i != pathRate1.Count - 1)
                                        result[pathRate1[i]].Add(new FIB(
                                            wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i - 1]],
                                            1,
                                            wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i + 1]],
                                            1
                                            ));
                                }
                                break;
                            case 3:
                                /** Builded path in 3th layer */
                                for (int i = 0; i < pathRate1.Count - 1; i++)
                                {
                                    topologyUnallocatedLayer3[pathRate1[i]].Remove(pathRate1[i + 1]);
                                }
                                break;
                        }
                        return result;
                    }
                    else
                    {
                        consoleWriter("[INFO] NOT able to connect nodes. All paths allocated.");
                        return null;
                    }
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

            return null;
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

       

      
        public void initLRMNode(String nodeName)
        {
            topologyUnallocatedLayer1.Add(nodeName, new Dictionary<string, int>());
            topologyUnallocatedLayer2.Add(nodeName, new Dictionary<string, int>());
            topologyUnallocatedLayer3.Add(nodeName, new Dictionary<string, int>());
            wholeTopologyNodesAndConnectedNodesWithPorts.Add(nodeName, new Dictionary<string, int>());
        }

        public void addTopologyElementFromLRM(String nodeName, String connectedNode, int connectedNodePort)
        {
            topologyUnallocatedLayer1[nodeName].Add(connectedNode, 1);
            topologyUnallocatedLayer2[nodeName].Add(connectedNode, 1);
            topologyUnallocatedLayer3[nodeName].Add(connectedNode, 1);
            wholeTopologyNodesAndConnectedNodesWithPorts[nodeName]
                .Add(connectedNode, connectedNodePort);
        }

        public void deleteTopologyElementFromLRM(String whoDied)
        {
            topologyUnallocatedLayer1.Remove(whoDied);
            topologyUnallocatedLayer2.Remove(whoDied);
            topologyUnallocatedLayer3.Remove(whoDied);
            foreach (var item in topologyUnallocatedLayer1.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                item.Value.Remove(whoDied);
            foreach (var item in topologyUnallocatedLayer2.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                item.Value.Remove(whoDied);
            foreach (var item in topologyUnallocatedLayer3.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                item.Value.Remove(whoDied);
        }

        public void initConnectionRequestFromCC(String nodeFrom, String nodeTo, int rate)
        {
            foreach(String id in socketHandler.Keys.Where(id => id.StartsWith("RC")))
            {
                RCtoRCSignallingMessage countPathsMsg = new RCtoRCSignallingMessage();
                countPathsMsg.State = RCtoRCSignallingMessage.COUNT_ALL_PATHS;
                countPathsMsg.Identifier = identifier;
                countPathsMsg.AllUpperNodesToCountWeights = wholeTopologyNodesAndConnectedNodesWithPorts.Keys.ToList();
                countPathsMsg.RateToCountWeights = rate;
                String dataToSend = JMessage.Serialize(JMessage.FromValue(countPathsMsg));
                socketHandler[id].Write(dataToSend);
            }
        }

        internal void lowerRcSendedConnectionsAction(Dictionary<string, Dictionary<string, int>> nodeConnectionsAndWeights)
        {
            foreach (String node in nodeConnectionsAndWeights.Keys)
            {
                for (int i = 0; i < nodeConnectionsAndWeights[node].Count; i++)
                {
                    if (!wholeTopologyNodesAndConnectedNodesWithPorts[node].ContainsKey(nodeConnectionsAndWeights[node].Keys.ElementAt(i)))
                    {
                        wholeTopologyNodesAndConnectedNodesWithPorts[node]
                            .Add(nodeConnectionsAndWeights[node].Keys.ElementAt(i),
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                    }
                }
            }
        }

        private void consoleWriter(String msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:[RC]" + msg);
            Console.Write(Environment.NewLine);
        }

        private void consoleStart()
        {
            consoleWriter("[INIT] Started.");
        }
    }
}
