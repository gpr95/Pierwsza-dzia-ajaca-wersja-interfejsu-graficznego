using ClientWindow;
using ControlCCRC.Protocols;
using Management;
using ManagementApp;
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
        public Boolean iAmDomain;
        public int domainNumber;

        private String identifier;

        private int lowerRcRequestedInAction;

        private TcpClient RCClient;
        private Thread threadconnectRC;

        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer1;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer2;
        private Dictionary<String, Dictionary<String, int>> topologyUnallocatedLayer3;
        public Dictionary<String, Dictionary<String, int>> wholeTopologyNodesAndConnectedNodesWithPorts;
        Dictionary<String, String> mapNodeConnectedNodeAndAssociatedRCSubnetwork;
        private List<String> pathNeededToBeCount;
        private String upperRc;
        private BinaryWriter upperWriter;

        public String requestNodeFrom;
        public String requestNodeTo;
        public int requestRate;
        public int requestId;
        private Dictionary<int, Dictionary<String, List<FIB>>> requestForFibs;
        private Dictionary<int, Dictionary<String, String>> requestIdAndRcNames;
        private ConnectionController ccHandler;
        private Dictionary<String, BinaryWriter> socketHandler;

        private int usingTopology1 = 0;
        private int usingTopology2 = 0;
        private int usingTopology3 = 0;

        private String associatedNodeStart;
        private String associatedNodeStop;
        public Dictionary<string, string> myBorderNodeAndConnectedOtherBorderNodeMap;

        public bool vc1Forbidden { get; private set; }
        public bool vc2Forbidden { get; private set; }
        public bool vc3Forbidden { get; private set; }



        /**
        * DOMAIN [RC_ID]
        * SUBNETWORK [RC_ID, connect up RC] 
        */
        public RoutingController(string[] args)
        {
            vc1Forbidden = false;
            vc2Forbidden = false;
            vc3Forbidden = false;
            requestForFibs = new Dictionary<int, Dictionary<string, List<FIB>>>();
            requestIdAndRcNames = new Dictionary<int, Dictionary<string, string>>();
            iAmDomain = (args.Length == 1);
            identifier = args[0];
            domainNumber = 0;
            if (!iAmDomain)
            {
                consoleWriter("[INIT] SUBNETWORK - " + identifier);
                try
                {
                    int rccId;
                    int.TryParse(args[1], out rccId);
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
                consoleWriter("[INIT] DOMAIN - " + identifier);
               int.TryParse(identifier.Substring(identifier.IndexOf("_")+1), out domainNumber);
                myBorderNodeAndConnectedOtherBorderNodeMap = new Dictionary<String, String>();
            }


            topologyUnallocatedLayer1 = new Dictionary<String, Dictionary<String, int>>();
            topologyUnallocatedLayer2 = new Dictionary<String, Dictionary<String, int>>();
            topologyUnallocatedLayer3 = new Dictionary<String, Dictionary<String, int>>();
            wholeTopologyNodesAndConnectedNodesWithPorts = new Dictionary<string, Dictionary<string, int>>();
            mapNodeConnectedNodeAndAssociatedRCSubnetwork = new Dictionary<String, String>();

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

        public void allocatedTopologyConnection(string nodeName, string connectedNode, int slotVC3)
        {
            consoleWriter("[LRM] Topology status: " + nodeName + " : " + connectedNode +  " allocated");
            switch (slotVC3)
            {
                case 11:
                    consoleWriter("ALLOCATION: removing slot " + slotVC3 + " from " + nodeName + " to " + connectedNode);
                    topologyUnallocatedLayer1[nodeName].Remove(connectedNode);
                    break;
                case 12:
                    consoleWriter("ALLOCATION: removing slot " + slotVC3 + " from " + nodeName + " to " + connectedNode);
                    topologyUnallocatedLayer2[nodeName].Remove(connectedNode);
                    break;
                case 13:
                    consoleWriter("ALLOCATION: removing slot " + slotVC3 + " from " + nodeName + " to " + connectedNode);
                    topologyUnallocatedLayer3[nodeName].Remove(connectedNode);
                    break;
            }
        }

        public void deallocatedTopologyConnection(string nodeName, string connectedNode, int slotVC3)
        {
            consoleWriter("[LRM] Topology status: " + nodeName + " : " + connectedNode + " resource free again ");
            switch (slotVC3)
            {
                case 11:
                    topologyUnallocatedLayer1[nodeName].Add(connectedNode,1);
                    break;
                case 12:
                    topologyUnallocatedLayer2[nodeName].Add(connectedNode, 1);
                    break;
                case 13:
                    topologyUnallocatedLayer3[nodeName].Add(connectedNode, 1);
                    break;
            }
        }

        private void rcConnecting()
        {
            BinaryReader reader = new BinaryReader(RCClient.GetStream());
            upperWriter = new BinaryWriter(RCClient.GetStream());



            RCtoRCSignallingMessage initMsg = new RCtoRCSignallingMessage();
            initMsg.State = RCtoRCSignallingMessage.RC_FROM_SUBNETWORK_INIT;
            initMsg.Identifier = identifier;
            String send_object = JSON.Serialize(JSON.FromValue(initMsg));
            upperWriter.Write(send_object);

            Boolean noError = true;
            while (noError)
            {
                try
                {
                    string received_data = reader.ReadString();
                    JSON received_object = JSON.Deserialize(received_data);
                    if (received_object.Type != typeof(RCtoRCSignallingMessage))
                        noError = false;
                    RCtoRCSignallingMessage msg = received_object.Value.ToObject<RCtoRCSignallingMessage>();
                    switch(msg.State)
                    {
                        case RCtoRCSignallingMessage.COUNT_ALL_PATHS_REQUEST:
                            pathNeededToBeCount = msg.AllUpperNodesToCountWeights;
                            upperRc = msg.Identifier;
                            requestId = msg.RequestId;
                            if (socketHandler.Keys.Where(id => id.StartsWith("RC_")).Count() > 0)
                            {
                                lowerRcRequestedInAction = socketHandler.Keys.Where(id => id.StartsWith("RC_")).Count();
                                Console.WriteLine("DEBUG###1: " + lowerRcRequestedInAction);
                                foreach (String id in socketHandler.Keys.Where(id => id.StartsWith("RC")))
                                {
                                    RCtoRCSignallingMessage countPathsMsg = new RCtoRCSignallingMessage();
                                    countPathsMsg.State = RCtoRCSignallingMessage.COUNT_ALL_PATHS_REQUEST;
                                    countPathsMsg.Identifier = identifier;
                                    countPathsMsg.RequestId = requestId;
                                    countPathsMsg.AllUpperNodesToCountWeights = wholeTopologyNodesAndConnectedNodesWithPorts.Keys.ToList();
                                    countPathsMsg.RateToCountWeights = msg.RateToCountWeights;
                                    String dataToSend = JSON.Serialize(JSON.FromValue(countPathsMsg));
                                    socketHandler[id].Write(dataToSend);
                                }
                            }
                            else
                            {
                                sendCountedWeightsToUpperNode(msg.RateToCountWeights);
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

        private void sendCountedWeightsToUpperNode( int rate)
        {
            RCtoRCSignallingMessage countedPathValue = new RCtoRCSignallingMessage();
            Dictionary<String, Dictionary<String, int>> nodeConnectionsAndWeights =
                new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, string> nodeConnectionsIntoSubnet =
                new Dictionary<string, string>();
            Dictionary<String, List<String>> computedPaths = new Dictionary<string, List<string>>();

            for (int i = 0; i < pathNeededToBeCount.Count(); i++)
            {
                List<String> others = pathNeededToBeCount
                    .Where(node => !node.Equals(pathNeededToBeCount[i])).ToList();
               
                Dictionary<String, int> connections = new Dictionary<string, int>();
                foreach (String s in others)
                {
                    if (computedPaths.ContainsKey(s) && computedPaths[s].Contains(pathNeededToBeCount[i]))
                        continue;
                    if (findWeightBetweenTwoNodes(pathNeededToBeCount[i], s, rate) != 0)
                    {
                        consoleWriter("[RC] Computing weights between: " + pathNeededToBeCount[i] + " and " + s + " with:"
                        + rate + "x VC-3");
                        connections.Add(s, findWeightBetweenTwoNodes(pathNeededToBeCount[i], s, rate));
                        if(!nodeConnectionsIntoSubnet.ContainsKey(pathNeededToBeCount[i]))
                            nodeConnectionsIntoSubnet.Add(pathNeededToBeCount[i], associatedNodeStart + "#" + associatedNodeStop);
                        consoleWriter("Computed weight  between:" + pathNeededToBeCount[i] + " and " + s +
                            "with w=" + connections[s]);
                    }
                }

                foreach (String other in others)
                {
                    if (!computedPaths.ContainsKey(pathNeededToBeCount[i]))
                    {
                        computedPaths.Add(pathNeededToBeCount[i], new List<String>());
                        computedPaths[pathNeededToBeCount[i]].Add(other);
                    }
                    else
                    {
                        computedPaths[pathNeededToBeCount[i]].Add(other);
                    }
                }
                if(connections.Count > 0)
                    nodeConnectionsAndWeights.Add(pathNeededToBeCount[i], connections);
            }

            if (nodeConnectionsAndWeights.Count != 0)
            {
                countedPathValue.NodeConnectionsAndWeights = nodeConnectionsAndWeights;
                countedPathValue.AssociatedNodesInSubnetwork = nodeConnectionsIntoSubnet;
                countedPathValue.State = RCtoRCSignallingMessage.COUNTED_ALL_PATHS_CONFIRM;
                foreach(String node in nodeConnectionsAndWeights.Keys)
                {
                    foreach(String connection in nodeConnectionsAndWeights[node].Keys)
                    {
                        consoleWriter("Sending calculated weights: " + node + " -> " + connection +
                            " weight:" + nodeConnectionsAndWeights[node][connection]);
                    }
                }
            }
            else
            {
                countedPathValue.State = RCtoRCSignallingMessage.COUNTED_ALL_PATHS_REFUSE;
            }
            countedPathValue.RateToCountWeights = rate;
            countedPathValue.Identifier = identifier;
            String dataToSend = JSON.Serialize(JSON.FromValue(countedPathValue));
            upperWriter.Write(dataToSend);
        }


        public int findWeightBetweenTwoNodes(String startNode, String endNode, int howMuchVC3)
        {
            int result = 0;

            bool ableToConnect1 = false;
            bool ableToConnect2 = false;
            foreach (String myNode in wholeTopologyNodesAndConnectedNodesWithPorts.Keys)
            {
                foreach(String connectedNode in wholeTopologyNodesAndConnectedNodesWithPorts[myNode].Keys)
                {
                    if (connectedNode.Equals(startNode))
                        ableToConnect1 = true;
                    if (connectedNode.Equals(endNode))
                        ableToConnect2 = true;
                }
            }
            if (!ableToConnect1 || !ableToConnect2)
                return 0;

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
               .Where(node => node.Value.ContainsKey(startNode)).FirstOrDefault().Key;
            if (firstInMyNetwork.Equals(default(String)))
                return 0;


            String lastInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)).FirstOrDefault().Key;
            if (lastInMyNetwork.Equals(default(String)))
                return 0;
       


            switch (howMuchVC3)
            {
                case 1:
                    if(shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1) != null)
                    {
                        int weight = 0;
                        List<String> path = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        for(int i = 0;i < path.Count-1; i++)
                        {
                            weight += topologyUnallocatedLayer1[path[i]][path[i + 1]];
                            consoleWriter("Weight" + i + " :" + topologyUnallocatedLayer1[path[i]][path[i + 1]]);
                        }
                        result = weight + 2;
                        associatedNodeStart = path.First();
                        associatedNodeStop = path.Last();
                        consoleWriter("Total: " + result);
                    }
                    if(result == 0 && shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2) != null)
                    {
                        int weight = 0;
                        List<String> path = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            weight += topologyUnallocatedLayer2[path[i]][path[i + 1]];
                            consoleWriter("Weight" + i + " :" + topologyUnallocatedLayer1[path[i]][path[i + 1]]);
                        }
                        result = weight + 2;
                        associatedNodeStart = path.First();
                        associatedNodeStop = path.Last();
                    }
                    if (result == 0 && shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3) != null)
                    {
                        int weight = 0;
                        List<String> path = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            weight += topologyUnallocatedLayer3[path[i]][path[i + 1]];
                            consoleWriter("Weight" + i + " :" + topologyUnallocatedLayer1[path[i]][path[i + 1]]);
                        }
                        result = weight + 2;
                        associatedNodeStart = path.First();
                        associatedNodeStop = path.Last();
                    }
                    break;
                case 2:
                    int shortest1 = 0;
                    int shortest2 = 0;
                    int shortest3 = 0;

                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2) != null)
                    {  
                        int weight = 0;
                        List<String> path = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            weight += topologyUnallocatedLayer1[path[i]][path[i + 1]];
                        }
                        shortest1 = weight + 2;
                        associatedNodeStart = path.First();
                        associatedNodeStop = path.Last();
                    }
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3) != null)
                    {
                        int weight = 0;
                        List<String> path = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            weight += topologyUnallocatedLayer1[path[i]][path[i + 1]];
                        }
                        shortest2 = weight + 2;
                    }
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3) != null)
                    {
                        int weight = 0;
                        List<String> path = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            weight += topologyUnallocatedLayer2[path[i]][path[i + 1]];
                        }
                        shortest3 = weight + 2;
                    }
                    int[] anArray = { shortest1, shortest2, shortest3};
                    result = anArray.Max();
                    break;
                case 3:
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3) != null)
                    {
                        int weight = 0;
                        List<String> path = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            weight += topologyUnallocatedLayer1[path[i]][path[i + 1]];
                        }
                        result = weight + 2;
                        associatedNodeStart = path.First();
                        associatedNodeStop = path.Last();
                    }
                    break;
            }
            return result;
        }


        public List<String> findChepestNodesBetweenTwoNodes(String startNode, String endNode, int howMuchVC3)
        {
            List<String> result = new List<string>();
            consoleWriter("[RC] calculating shortest path (only calculating): " + startNode + " and " + endNode + " with:"
                + howMuchVC3 + "x VC-3");

            if (wholeTopologyNodesAndConnectedNodesWithPorts
               .Where(node => node.Value.ContainsKey(startNode)) == null)
            {
                return null;
            }
            if (wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)) == null)
            {
                return null;
            }

            String firstInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(startNode)).First().Key;

            String lastInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)).First().Key;


            switch (howMuchVC3)
            {
                case 1:
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1) != null)
                        result = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                    if ((result == null || result.Count == 0) 
                        && shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2) != null)
                        result = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2);
                    if ((result == null || result.Count == 0)
                        && shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3) != null)
                        result = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3);
                    break;
                case 2:
                    List<String> shortest1 = null;
                    List<String> shortest2 = null;
                    List<String> shortest3 = null;

                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2) != null)
                    {
                        shortest1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                    }
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3) != null)
                    {
                        shortest2 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                    }
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3) != null)
                    {
                        shortest3 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                    }

                    if (shortest1 != null)
                        result = shortest1;
                    else if (shortest2 != null)
                        result = shortest2;
                    else if (shortest3 != null)
                        result = shortest3;
                    break;
                case 3:
                    if (shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2) != null &&
                        shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3) != null)
                        result = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                    break;
            }

            return result;
        }

        public Dictionary<String,List<FIB>> findPathWithSubnetworks(String startNode, String endNode, int howMuchVC3)
        {
            consoleWriter("[CC] Sended info to make path between: " + startNode + " and " + endNode + " with:"
                + howMuchVC3 + "x VC-3");
            Dictionary<String, List<FIB>> result = new Dictionary<string, List<FIB>>();
            String firstInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
               .Where(node => node.Value.ContainsKey(startNode)).FirstOrDefault().Key;
            if (firstInMyNetwork == null || firstInMyNetwork.Equals(default(String)))
                return null;
            String lastInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)).FirstOrDefault().Key;
            if (lastInMyNetwork == null || lastInMyNetwork.Equals(default(String)))
                return null;

            switch (howMuchVC3)
            {
                case 1:
                    int whichTopology = 1;
                    usingTopology1 = 1;
                    usingTopology2 = 0;
                    usingTopology3 = 0;
                    List<String> pathRate1 = null;
                        pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                    if (pathRate1 == null || !pathRate1.First().Equals(firstInMyNetwork) || !pathRate1.Last().Equals(lastInMyNetwork))
                    {
                            pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2);
                        whichTopology = 2;
                        usingTopology1 = 0;
                        usingTopology2 = 1;
                        usingTopology3 = 0;
                    }
                    if (pathRate1 == null || !pathRate1.First().Equals(firstInMyNetwork) || !pathRate1.Last().Equals(lastInMyNetwork))
                    {
                            pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3);
                        whichTopology = 3;
                        usingTopology1 = 0;
                        usingTopology2 = 0;
                        usingTopology3 = 1;
                    }

                    if (pathRate1 != null && pathRate1.First().Equals(firstInMyNetwork) && pathRate1.Last().Equals(lastInMyNetwork))
                    {
                        foreach (var temp in pathRate1)
                        {
                            consoleWriter("[INFO] Shortest path : " + temp);
                        }
                        foreach (String node in pathRate1)
                        {
                            result.Add(node, new List<FIB>());
                        }

                        result = setFibsForRateAndCallSubnetworks(pathRate1,
                            startNode, endNode, usingTopology1, usingTopology2, usingTopology3);
                        return result;
                    }
                    else
                    {
                        consoleWriter("[INFO] NOT able to connect nodes. All paths allocated.");
                        usingTopology1 = 0;
                        usingTopology2 = 0;
                        usingTopology3 = 0;
                        return null;
                    }
                case 2:
                    List<String> path1 = null;
                    List<String> path2 = null;
                    List<String> path3 = null;
                        path1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        path2 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2);
                        path3 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3);
                    if (path1 != null && path1.Count > 0 && path1.First().Equals(firstInMyNetwork) && path1.Last().Equals(lastInMyNetwork) &&
                       path2 != null && path2.Count > 0 && path2.First().Equals(firstInMyNetwork) && path2.Last().Equals(lastInMyNetwork))
                    {
                        usingTopology1 = 1;
                        usingTopology2 = 1;
                        usingTopology3 = 0;
                        /** Builded path in 1st layer */
                        /** Builded path in 2nd layer */

                    }
                    else if (path1 != null && path1.Count > 0 && path1.First().Equals(firstInMyNetwork) && path1.Last().Equals(lastInMyNetwork) &&
                             path3 != null && path3.Count > 0 && path3.First().Equals(firstInMyNetwork) && path3.Last().Equals(lastInMyNetwork))
                    {
                        usingTopology1 = 1;
                        usingTopology2 = 0;
                        usingTopology3 = 1;
                        /** Builded path in 1st layer */
                        /** Builded path in 3nd layer */

                    }
                    else if (path2 != null && path2.Count > 0 && path2.First().Equals(firstInMyNetwork) && path2.Last().Equals(lastInMyNetwork) &&
                             path3 != null && path3.Count > 0 && path3.First().Equals(firstInMyNetwork) && path3.Last().Equals(lastInMyNetwork))
                    {
                        usingTopology1 = 0;
                        usingTopology2 = 1;
                        usingTopology3 = 1;
                        /** Builded path in 2nd layer */
                        /** Builded path in 3nd layer */
                    }
                    else
                    {
                        consoleWriter("[INFO] NOT able to connect nodes. All paths allocated.");
                        usingTopology1 = 0;
                        usingTopology2 = 0;
                        usingTopology3 = 0;
                        return null;
                    }

                    List<String> properPath = null;
                    if (usingTopology1 == 1)
                        properPath = path1;
                    if (usingTopology2 == 1)
                        properPath = path2;
                    if (usingTopology3 == 1)
                        properPath = path3;
                    result = setFibsForRateAndCallSubnetworks(properPath,
                           startNode, endNode, usingTopology1, usingTopology2, usingTopology3);
                    return result;
                case 3:
                    List<String> path31 = null;
                    List<String> path32 = null;
                    List<String> path33 = null;
                        path31 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        path32 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2);
                        path33 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3);
                    if (path31 != null && path31.Count > 0 && path31.First().Equals(firstInMyNetwork) && path31.Last().Equals(lastInMyNetwork) &&
                       path32 != null && path32.Count > 0 && path32.First().Equals(firstInMyNetwork) && path32.Last().Equals(lastInMyNetwork) &&
                       path33 != null && path33.Count > 0 && path33.First().Equals(firstInMyNetwork) && path33.Last().Equals(lastInMyNetwork))
                    {

                        usingTopology1 = 1;
                        usingTopology2 = 1;
                        usingTopology3 = 1;
                    }
                    else
                    {
                        consoleWriter("[INFO] NOT able to connect nodes. All paths allocated.");
                        usingTopology1 = 0;
                        usingTopology2 = 0;
                        usingTopology3 = 0;
                        return null;
                    }
                    result = setFibsForRateAndCallSubnetworks(path31,
                           startNode, endNode, usingTopology1, usingTopology2, usingTopology3);
                    return result;
            }





            return result;
        }

        public Dictionary<string,List<FIB>> setFibsForRateAndCallSubnetworks(List<String> path,String startNode,
            String endNode, int vc31,int vc32, int vc33)
        {
            Dictionary<string, List<FIB>> result = new Dictionary<string, List<FIB>>();
            List<String> additionalPath = new List<string>();
            additionalPath.Add(startNode);
            foreach(String node in path)
            {
                additionalPath.Add(node);
            }
            additionalPath.Add(endNode);

            Dictionary<String,String> nodeFromListAndRcName = new Dictionary<String, String>();
            Dictionary<String, String> nodeToListAndRcName = new Dictionary<String, String>();

            foreach (String node in additionalPath)
            {
                if(!node.Equals(startNode) && !node.Equals(endNode))
                    result.Add(node, new List<FIB>());
            }
            List<int> layers = new List<int>();
            if (vc31 != 0)
                layers.Add(1);
            if (vc32 != 0)
                layers.Add(2);
            if (vc33 != 0)
                layers.Add(3);
            List<int> lastFibs = new List<int>();
            if (!vc1Forbidden)
                lastFibs.Add(1);
            if (!vc2Forbidden)
                lastFibs.Add(2);
            if (!vc3Forbidden)
                lastFibs.Add(3);
            int counter = 0;
            foreach (int layer in layers)
            {

                for (int i = 1; i < additionalPath.Count - 1; i++)
                {
                    bool isSubnetwork = false;
                    if (layer == 1 && topologyUnallocatedLayer1[additionalPath.ElementAt(i)][additionalPath.ElementAt(i + 1)] > 1)
                        isSubnetwork = true;
                    else if (layer == 2 && topologyUnallocatedLayer2[additionalPath.ElementAt(i)][additionalPath.ElementAt(i + 1)] > 1)
                        isSubnetwork = true;
                    else if (layer == 3 && topologyUnallocatedLayer3[additionalPath.ElementAt(i)][additionalPath.ElementAt(i + 1)] > 1)
                        isSubnetwork = true;

                    if (isSubnetwork)
                    {
                        String virtualNodeFrom = additionalPath.ElementAt(i);
                        String virtualNodeTo = additionalPath.ElementAt(i + 1);
                        String rcNeededToBeSet =
                            mapNodeConnectedNodeAndAssociatedRCSubnetwork[virtualNodeFrom + "#" + virtualNodeTo].Substring(0,
                             mapNodeConnectedNodeAndAssociatedRCSubnetwork[virtualNodeFrom + "#" + virtualNodeTo].IndexOf("#"));
                        String internalNodeFrom = mapNodeConnectedNodeAndAssociatedRCSubnetwork[virtualNodeFrom + "#" + virtualNodeTo].Split('#')[1];
                        String internalNodeTo = mapNodeConnectedNodeAndAssociatedRCSubnetwork[virtualNodeTo + "#" + virtualNodeFrom].Split('#')[1];
                        consoleWriter("While setting FIB founded subnetwork!!!\n " +
                            "VirtualNodeFrom:" + virtualNodeFrom + "\n" +
                            "internalNodeFrom:" + internalNodeFrom + "\n" +
                            "internalNodeTo:" + internalNodeTo + "\n" +
                            "VirtualNodeTo:" + virtualNodeTo + "\n" +
                            "RC: " + rcNeededToBeSet);

                        if(!nodeFromListAndRcName.Keys.Contains(virtualNodeFrom))
                            nodeFromListAndRcName.Add(virtualNodeFrom, rcNeededToBeSet);
                        if (!nodeToListAndRcName.Keys.Contains(virtualNodeTo))
                            nodeToListAndRcName.Add(virtualNodeTo, rcNeededToBeSet);

                        result[additionalPath[i]].Add(new FIB(
                        wholeTopologyNodesAndConnectedNodesWithPorts[additionalPath[i]][additionalPath[i - 1]],
                        layer + 10,
                        wholeTopologyNodesAndConnectedNodesWithPorts[additionalPath[i]][internalNodeFrom],
                        layer + 10
                        ));
                        result[additionalPath[i + 1]].Add(new FIB(
                       wholeTopologyNodesAndConnectedNodesWithPorts[additionalPath[i + 1]][internalNodeTo],
                       layer + 10,
                       wholeTopologyNodesAndConnectedNodesWithPorts[additionalPath[i + 1]][additionalPath[i + 2]],
                       lastFibs[counter] + 10
                       ));

                        i++;
                        continue;
                    }
                    else
                    {
                        if (i == additionalPath.Count - 2)
                        {
                            result[additionalPath[i]].Add(new FIB(
                               wholeTopologyNodesAndConnectedNodesWithPorts[additionalPath[i]][additionalPath[i - 1]],
                               layer + 10,
                               wholeTopologyNodesAndConnectedNodesWithPorts[additionalPath[i]][additionalPath[i + 1]],
                               lastFibs[counter] + 10
                               ));
                        }
                        else
                        {
                            result[additionalPath[i]].Add(new FIB(
                                wholeTopologyNodesAndConnectedNodesWithPorts[additionalPath[i]][additionalPath[i - 1]],
                                layer + 10,
                                wholeTopologyNodesAndConnectedNodesWithPorts[additionalPath[i]][additionalPath[i + 1]],
                                layer + 10
                                ));
                        }
                    }
                    
                    
                }
                counter++;
            }
            if (!requestForFibs.ContainsKey(requestId))
                requestForFibs.Add(requestId, result);
            if (!requestIdAndRcNames.ContainsKey(requestId))
                requestIdAndRcNames.Add(requestId, nodeFromListAndRcName);

            for (int i = 0; i < nodeFromListAndRcName.Count; i++)
                ccHandler.sendFIBSettingRequestForSubnetwork(nodeFromListAndRcName.Keys.ElementAt(i), 
                    nodeToListAndRcName.Keys.ElementAt(i), nodeFromListAndRcName.Values.ElementAt(i), vc31 + vc32 + vc33);

           
            return result;
        }


        public Dictionary<String,List<FIB>> findPath(String startNode, String endNode, int howMuchVC3)
        {
            
            consoleWriter("[CC] Sended info to make path between: " + startNode + " and " + endNode + " with:" 
                + howMuchVC3 + "x VC-3");

            Dictionary<String, List<FIB>> result = new Dictionary<string, List<FIB>>();
            String firstInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(startNode)).FirstOrDefault().Key;
            if (firstInMyNetwork == null || firstInMyNetwork.Equals(default(String)))
                return null;


            String lastInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)).FirstOrDefault().Key;
            if (lastInMyNetwork == null || lastInMyNetwork.Equals(default(String)))
                return null;

            switch (howMuchVC3)
            {
                case 1:
                    int whichTopology = 1;
                    usingTopology1 = 1;
                    usingTopology2 = 0;
                    usingTopology3 = 0;
                    List<String> pathRate1 = null;
                        pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                    if (pathRate1 == null || pathRate1.Count == 0 || !pathRate1.First().Equals(firstInMyNetwork) || !pathRate1.Last().Equals(lastInMyNetwork))
                    {
                            pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2);
                        whichTopology = 2;
                        usingTopology1 = 0;
                        usingTopology2 = 1;
                        usingTopology3 = 0;
                    }
                    if (pathRate1 == null || pathRate1.Count == 0 || !pathRate1.First().Equals(firstInMyNetwork) || !pathRate1.Last().Equals(lastInMyNetwork))
                    {
                            pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3);
                        whichTopology = 3;
                        usingTopology1 = 0;
                        usingTopology2 = 0;
                        usingTopology3 = 1;
                    }

                    if (pathRate1 != null && pathRate1.Count != 0 && pathRate1.First().Equals(firstInMyNetwork) && pathRate1.Last().Equals(lastInMyNetwork))
                    {
                        foreach(var temp in pathRate1)
                        {
                            consoleWriter("[INFO] Shortest path : " + temp);
                        }
                        foreach (String node in pathRate1)
                        {
                            result.Add(node, new List<FIB>());
                        }
                            result.First().Value.Add(new FIB(
                                                wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][startNode],
                                                whichTopology + 10,
                                                wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][pathRate1[1]],
                                                whichTopology + 10
                                                ));
                        int lastSlot = whichTopology;
                        switch(whichTopology)
                        {
                            case 1:
                                if (vc1Forbidden)
                                {
                                    if (!vc2Forbidden)
                                        lastSlot = 2;
                                    else
                                        lastSlot = 3;
                                }
                                break;
                            case 2:
                                if (vc2Forbidden)
                                {
                                    if (!vc1Forbidden)
                                        lastSlot = 1;
                                    else
                                        lastSlot = 3;
                                }
                                break;
                            case 3:
                                if (vc3Forbidden)
                                {
                                    if (!vc1Forbidden)
                                        lastSlot = 1;
                                    else
                                        lastSlot = 2;
                                }
                                break;
                        }
                            result.Last().Value.Add(new FIB(
                                                wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1.Last()][pathRate1[pathRate1.Count - 2]],
                                                whichTopology+10,
                                                wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1.Last()][endNode],
                                                lastSlot + 10
                                                ));

                        for(int i =1; i< pathRate1.Count-1; i++)
                        {
                                result[pathRate1[i]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i - 1]],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i + 1]],
                                    whichTopology + 10
                                    ));
                        }

                        foreach (var temp in result)
                        {
                            foreach (var fib in temp.Value)
                            {
                                Console.WriteLine("debug: " + temp.Key + " " + fib.toString());
                            }
                        }
                        if (!requestForFibs.ContainsKey(requestId))
                            requestForFibs.Add(requestId, result);
                        return result;
                    }
                    else
                    {
                        consoleWriter("[INFO] NOT able to connect nodes. All paths allocated.");
                        usingTopology1 = 0;
                        usingTopology2 = 0;
                        usingTopology3 = 0;
                        return null;
                    }
                case 2:
                    List<String> path1 =null;
                    List<String> path2 = null;
                    List<String> path3 = null;
                        path1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        path2 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2);
                        path3 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3);
                    if (path1 != null && path1.Count > 0 && path1.First().Equals(firstInMyNetwork) && path1.Last().Equals(lastInMyNetwork) &&
                       path2 != null && path2.Count > 0 && path2.First().Equals(firstInMyNetwork) && path2.Last().Equals(lastInMyNetwork))
                    {
                        usingTopology1 = 1;
                        usingTopology2 = 1;
                        usingTopology3 = 0;
                        /** Builded path in 1st layer */
                        /** Builded path in 2nd layer */
                        
                    }
                    else if (path1 != null && path1.Count > 0 && path1.First().Equals(firstInMyNetwork) && path1.Last().Equals(lastInMyNetwork) &&
                             path3 != null && path3.Count > 0 && path3.First().Equals(firstInMyNetwork) && path3.Last().Equals(lastInMyNetwork))
                    {
                        usingTopology1 = 1;
                        usingTopology2 = 0;
                        usingTopology3 = 1;
                        /** Builded path in 1st layer */
                        /** Builded path in 3nd layer */

                    }
                    else if (path2 != null && path2.Count > 0 && path2.First().Equals(firstInMyNetwork) && path2.Last().Equals(lastInMyNetwork) &&
                             path3 != null && path3.Count > 0 && path3.First().Equals(firstInMyNetwork) && path3.Last().Equals(lastInMyNetwork))
                    {
                        usingTopology1 = 0;
                        usingTopology2 = 1;
                        usingTopology3 = 1;
                        /** Builded path in 2nd layer */
                        /** Builded path in 3nd layer */
                    }
                    else
                    {
                        consoleWriter("[INFO] NOT able to connect nodes. All paths allocated.");
                        usingTopology1 = 0;
                        usingTopology2 = 0;
                        usingTopology3 = 0;
                        return null;
                    }
                    List<int> makePaths = new List<int>();
                    if (usingTopology1 == 1)
                        makePaths.Add(1);
                    if (usingTopology2 == 1)
                        makePaths.Add(2);
                    if (usingTopology3 == 1)
                        makePaths.Add(3);
                    List<int> lastSlots = new List<int>();
                    if (usingTopology1 == 1 && !vc1Forbidden)
                        lastSlots.Add(1);
                    if (usingTopology2 == 1 && !vc2Forbidden)
                        lastSlots.Add(2);
                    if (usingTopology3 == 1 && !vc2Forbidden)
                        lastSlots.Add(3);
                    if (!vc1Forbidden && !lastSlots.Contains(1) && lastSlots.Count != 2)
                        lastSlots.Add(1);
                    if (!vc2Forbidden && !lastSlots.Contains(2) && lastSlots.Count != 2)
                        lastSlots.Add(2);
                    if (!vc3Forbidden && !lastSlots.Contains(3) && lastSlots.Count != 2)
                        lastSlots.Add(3);
                    List<String> properPath = null;
                    if (usingTopology1 == 1)
                        properPath = path1;
                    if (usingTopology2 == 1)
                        properPath = path2;
                    if (usingTopology3 == 1)
                        properPath = path3;

                    foreach (String node in properPath)
                    {
                        result.Add(node, new List<FIB>());
                    }

                    int counter = 0;
                    foreach (int layers in makePaths)
                    {
                        result.First().Value.Add(new FIB(
                                            wholeTopologyNodesAndConnectedNodesWithPorts[properPath[0]][startNode],
                                            layers + 10,
                                            wholeTopologyNodesAndConnectedNodesWithPorts[properPath[0]][properPath[1]],
                                            layers + 10
                                            ));
                        result.Last().Value.Add(new FIB(
                                            wholeTopologyNodesAndConnectedNodesWithPorts[properPath.Last()][properPath[properPath.Count - 2]],
                                            layers + 10,
                                            wholeTopologyNodesAndConnectedNodesWithPorts[properPath.Last()][endNode],
                                            lastSlots.ElementAt(counter++) + 10
                                            ));

                        for (int i = 1; i < properPath.Count - 1; i++)
                        {
                            result[properPath[i]].Add(new FIB(
                                wholeTopologyNodesAndConnectedNodesWithPorts[properPath[i]][properPath[i - 1]],
                                layers + 10,
                                wholeTopologyNodesAndConnectedNodesWithPorts[properPath[i]][properPath[i + 1]],
                                layers + 10
                                ));
                        }
                    }
                    if (!requestForFibs.ContainsKey(requestId))
                        requestForFibs.Add(requestId, result);
                    return result;
                case 3:
                    List<String> path31 = null;
                    List<String> path32 = null;
                    List<String> path33 = null;
                        path31 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
                        path32 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer2);
                        path33 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer3);
                    if (path31 != null && path31.Count > 0 && path31.First().Equals(firstInMyNetwork) && path31.Last().Equals(lastInMyNetwork) &&
                       path32 != null && path32.Count > 0 && path32.First().Equals(firstInMyNetwork) && path32.Last().Equals(lastInMyNetwork) &&
                       path33 != null && path33.Count > 0 && path33.First().Equals(firstInMyNetwork) && path33.Last().Equals(lastInMyNetwork))
                    {

                        usingTopology1 = 1;
                        usingTopology2 = 1;
                        usingTopology3 = 1;
                        /** Builded path in 1st layer */
                        /** Builded path in 2nd layer */
                        /** Builded path in 3nd layer */
                        foreach (String node in path31)
                        {
                            result.Add(node, new List<FIB>());
                        }
                        for (int trippleRate = 1; trippleRate <= 3; trippleRate++)
                        {
                            consoleWriter("[INFO] Shortest path : ");
                            foreach (var temp in path31)
                            {
                                consoleWriter(temp + " ### ");
                            }
                            result.First().Value.Add(new FIB(
                                                wholeTopologyNodesAndConnectedNodesWithPorts[path31[0]][startNode],
                                                trippleRate + 10,
                                                wholeTopologyNodesAndConnectedNodesWithPorts[path31[0]][path31[1]],
                                                trippleRate + 10
                                                ));
                            result.Last().Value.Add(new FIB(
                                                wholeTopologyNodesAndConnectedNodesWithPorts[path31.Last()][path31[path31.Count - 2]],
                                                trippleRate + 10,
                                                wholeTopologyNodesAndConnectedNodesWithPorts[path31.Last()][endNode],
                                                trippleRate + 10
                                                ));

                            for (int i = 1; i < path31.Count - 1; i++)
                            {
                                result[path31[i]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[path31[i]][path31[i - 1]],
                                    trippleRate + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[path31[i]][path31[i + 1]],
                                    trippleRate + 10
                                    ));
                            }
                        }
                        if(!requestForFibs.ContainsKey(requestId))
                        requestForFibs.Add(requestId, result);
                        return result;
                    }
                    else
                    {
                        consoleWriter("[ERROR] NOT able to connect nodes. All paths allocated.");
                        usingTopology1 = 0;
                        usingTopology2 = 0;
                        usingTopology3 = 0;
                        return null;
                    }
                default:
                    consoleWriter("[ERROR] Wrong VC-3 number");
                    break;
            }

            return null;
        }

     

 
            

        public List<String> shortest_path(String start, String finish, Dictionary<String, Dictionary<String, int>> topology)
        {
            List<String> nodesUnderControl = wholeTopologyNodesAndConnectedNodesWithPorts.Keys.ToList();
            List<String> nodes = new List<String>();
            Dictionary<String, int> distances = new Dictionary<String, int>();
            Dictionary<String, String> previous = new Dictionary<String, String>();

            foreach (KeyValuePair<String, Dictionary<String, int>> nodeAndConnected in topology)
            {
                if (nodeAndConnected.Key.Equals(start))
                    distances[nodeAndConnected.Key] = 0;
                else
                    distances[nodeAndConnected.Key] = int.MaxValue;

                nodes.Add(nodeAndConnected.Key);
            }

            while (nodes.Count != 0)
            {
                nodes.Sort((x, y) => distances[x] - distances[y]);

                String smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest.Equals(finish))
                {
                    List<String> reversePath = new List<String>();
                    while (previous.ContainsKey(smallest))
                    {
                        reversePath.Add(smallest);
                        smallest = previous[smallest];
                    }
                    List<String> result = new List<string>();
                    result.Add(start);
                    for (int i = reversePath.Count - 1; i >= 0; i--)
                        result.Add(reversePath.ElementAt(i));
                    return result;
                }

                if (distances[smallest] == int.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in topology[smallest].Where(node => nodesUnderControl.Contains(node.Key)))
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }

            }

            return null;
        }
        
        public void initLRMNode(String nodeName)
        {
            consoleWriter("[LRM] INIT FROM: " + nodeName);
            topologyUnallocatedLayer1.Add(nodeName, new Dictionary<string, int>());
            topologyUnallocatedLayer2.Add(nodeName, new Dictionary<string, int>());
            topologyUnallocatedLayer3.Add(nodeName, new Dictionary<string, int>());
            wholeTopologyNodesAndConnectedNodesWithPorts.Add(nodeName, new Dictionary<string, int>());
        }

        public void addTopologyElementFromLRM(String nodeName, String connectedNode, int connectedNodePort)
        {
            consoleWriter("[LRM] New topology:" + nodeName + " to " + connectedNode);
            topologyUnallocatedLayer1[nodeName].Add(connectedNode, 1);
            topologyUnallocatedLayer2[nodeName].Add(connectedNode, 1);
            topologyUnallocatedLayer3[nodeName].Add(connectedNode, 1);
            wholeTopologyNodesAndConnectedNodesWithPorts[nodeName]
                .Add(connectedNode, connectedNodePort);

            Address adr = new Address(connectedNode);
            if (iAmDomain && adr.domain != domainNumber)
            {

                Address adr2 = new Address(nodeName);
                ccHandler.sendBorderNodesToNCC(adr2, adr.domain);
                myBorderNodeAndConnectedOtherBorderNodeMap.Add(nodeName, connectedNode);
            }
        }

        public void deleteTopologyElementFromLRM(String whoDied)
        {
            consoleWriter("[LRM]" + whoDied + "died ! (removing from topology)");
            foreach (var item in topologyUnallocatedLayer1.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                item.Value.Remove(whoDied);
            foreach (var item in topologyUnallocatedLayer2.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                item.Value.Remove(whoDied);
            foreach (var item in topologyUnallocatedLayer3.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                item.Value.Remove(whoDied);
            foreach (var item in wholeTopologyNodesAndConnectedNodesWithPorts.Where(node => node.Value.ContainsKey(whoDied)).ToList())
                item.Value.Remove(whoDied);
            topologyUnallocatedLayer1.Remove(whoDied);
            topologyUnallocatedLayer2.Remove(whoDied);
            topologyUnallocatedLayer3.Remove(whoDied);
            wholeTopologyNodesAndConnectedNodesWithPorts.Remove(whoDied);
            socketHandler.Remove(whoDied);
            if(requestForFibs.ContainsKey(requestId) && requestForFibs[requestId].ContainsKey(whoDied))
                requestForFibs[requestId].Remove(whoDied);

            if (iAmDomain)
            {
                releaseConnection(requestId);
                initConnectionRequestFromCC(requestNodeFrom, requestNodeTo, requestRate, requestId, 1, 1, 1,true);
            }

        }

        public void initConnectionRequestFromCC(String nodeFrom, String nodeTo, int rate, int requestId, int vc1, int vc2, int vc3, bool routed)
        {
            consoleWriter("[CC] Received request for path between:" + nodeFrom
                                + " and " + nodeTo + " with " + rate + "x VC-3 , requestId=" + requestId);
            this.requestNodeFrom = nodeFrom;
            this.requestNodeTo = nodeTo;
            this.requestRate = rate;
            this.requestId = requestId;
            if (vc1 == 0)
                this.vc1Forbidden = true;
            if (vc2 == 0)
                this.vc2Forbidden = true;
            if (vc3 == 0)
                this.vc3Forbidden = true;
            lowerRcRequestedInAction = socketHandler.Keys.Where(id => id.StartsWith("RC_")).ToList().Count();

            if(lowerRcRequestedInAction == 0)
            {
                ccHandler.sendFibs(findPath(nodeFrom, nodeTo, rate), usingTopology1, usingTopology2, usingTopology3,requestId, rate, routed);
            }

            foreach (String id in socketHandler.Keys.Where(id => id.StartsWith("RC_")))
            {
                consoleWriter("[SENDING TO LOWER RC] Compute path to  " + id);
                RCtoRCSignallingMessage countPathsMsg = new RCtoRCSignallingMessage();
                countPathsMsg.State = RCtoRCSignallingMessage.COUNT_ALL_PATHS_REQUEST;
                countPathsMsg.Identifier = identifier;
                countPathsMsg.RequestId = requestId;
                countPathsMsg.AllUpperNodesToCountWeights = wholeTopologyNodesAndConnectedNodesWithPorts.Keys.ToList();
                countPathsMsg.RateToCountWeights = rate;
                String dataToSend = JSON.Serialize(JSON.FromValue(countPathsMsg));
                socketHandler[id].Write(dataToSend);
            }
        }

        public void lowerRcSendedConnectionsAction(Dictionary<string, Dictionary<string, int>> nodeConnectionsAndWeights, 
            Dictionary<string, string> associatedNodes, int rate, String rcFrom)
        {
            lowerRcRequestedInAction--;
            consoleWriter("[LOWER RC] Received counted weigths from " + rcFrom);
            foreach (String node in nodeConnectionsAndWeights.Keys)
            {
                for (int i = 0; i < nodeConnectionsAndWeights[node].Count; i++)
                {
                    if (!topologyUnallocatedLayer1[node].ContainsKey(nodeConnectionsAndWeights[node].Keys.ElementAt(i)))
                    {
                        topologyUnallocatedLayer1[node]
                            .Add(nodeConnectionsAndWeights[node].Keys.ElementAt(i),
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer1[nodeConnectionsAndWeights[node].Keys.ElementAt(i)]
                            .Add(node,
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer2[node]
                            .Add(nodeConnectionsAndWeights[node].Keys.ElementAt(i),
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer2[nodeConnectionsAndWeights[node].Keys.ElementAt(i)]
                            .Add(node,
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer3[node]
                            .Add(nodeConnectionsAndWeights[node].Keys.ElementAt(i),
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer3[nodeConnectionsAndWeights[node].Keys.ElementAt(i)]
                            .Add(node,
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.Add(node + "#" +
                            nodeConnectionsAndWeights[node].Keys.ElementAt(i), rcFrom 
                            + "#" + associatedNodes[node].Substring(0, associatedNodes[node].IndexOf("#")));
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.Add(
                            nodeConnectionsAndWeights[node].Keys.ElementAt(i) + "#" +
                            node, rcFrom + "#" + associatedNodes[node].Substring(associatedNodes[node].IndexOf("#") + 1));
                    }
                }
            }
            if (lowerRcRequestedInAction == 0)
            {
                if (iAmDomain)
                {
                    consoleWriter("[PATH] Calculating shortest path between:" + requestNodeFrom + " and " + requestNodeTo);
                    List<String> path = findChepestNodesBetweenTwoNodes(requestNodeFrom, requestNodeTo, requestRate);
                    if(path == null || path.Count == 0)
                    {
                        consoleWriter("[ERROR] PATH NOT FOUND IN DOMAIN");
                        return;
                    }

                    if(mapNodeConnectedNodeAndAssociatedRCSubnetwork.ContainsKey(requestNodeFrom + "#"+ path[0]))
                    {
                        lowerRcRequestedInAction++;
                        string temp;
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.TryGetValue(requestNodeFrom + "#" + path[0], out temp);
                        Console.WriteLine("DEBUG###: " + temp);

                        ccHandler.sendRequestToSubnetworkCCToBuildPath(temp, requestNodeFrom, path[0], rate,requestId);
                    }
                    for(int i = 0; i < path.Count-1; i++)
                    {
                        lowerRcRequestedInAction++;
                        string temp;
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.TryGetValue(path[i] + "#" + path[i+1], out temp);
                        Console.WriteLine("DEBUG###: " + temp);

                        ccHandler.sendRequestToSubnetworkCCToBuildPath(temp, path[i], path[i+1], rate, requestId);
                    }
                    if (mapNodeConnectedNodeAndAssociatedRCSubnetwork.ContainsKey(path.Last() + "#" + requestNodeTo))
                    {
                        lowerRcRequestedInAction++;
                        string temp;
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.TryGetValue(path.Last() + "#" + requestNodeTo, out temp);
                        Console.WriteLine("DEBUG###: " + temp);

                        ccHandler.sendRequestToSubnetworkCCToBuildPath(temp, path.Last(), requestNodeTo, rate, requestId);
                    }
                }
                else
                {
                    sendCountedWeightsToUpperNode(rate);
                }
            }
        }


        public void startProperWeigthComputingTopBottom(Dictionary<string, Dictionary<string, int>> nodeConnectionsAndWeights,
            Dictionary<string, string> associatedNodes, int rate, String rcFrom,String nodeFrom, String nodeTo)
        {
            Console.WriteLine("[CC] Received FIB computing request for connection between:" + nodeFrom
                                + " and " + nodeTo + " with " + rate + "x VC-3");
            consoleWriter("[COUNTING FIBS]");
            Dictionary<String, String> nodeHashtagNodeAndNodeInSubnetwork = new Dictionary<string, string>();
            foreach (String node in nodeConnectionsAndWeights.Keys)
            {
                for (int i = 0; i < nodeConnectionsAndWeights[node].Count; i++)
                {
                    if (!topologyUnallocatedLayer1[node].ContainsKey(nodeConnectionsAndWeights[node].Keys.ElementAt(i)))
                    {
                        topologyUnallocatedLayer1[node]
                            .Add(nodeConnectionsAndWeights[node].Keys.ElementAt(i),
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer1[nodeConnectionsAndWeights[node].Keys.ElementAt(i)]
                            .Add(node,
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer2[node]
                            .Add(nodeConnectionsAndWeights[node].Keys.ElementAt(i),
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer2[nodeConnectionsAndWeights[node].Keys.ElementAt(i)]
                            .Add(node,
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer3[node]
                            .Add(nodeConnectionsAndWeights[node].Keys.ElementAt(i),
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        topologyUnallocatedLayer3[nodeConnectionsAndWeights[node].Keys.ElementAt(i)]
                            .Add(node,
                            nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.Add(node + "#" +
                            nodeConnectionsAndWeights[node].Keys.ElementAt(i), rcFrom
                            + "#" + associatedNodes[node].Substring(0, associatedNodes[node].IndexOf("#")));
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.Add(
                            nodeConnectionsAndWeights[node].Keys.ElementAt(i) + "#" +
                            node, rcFrom + "#" + associatedNodes[node].Substring(associatedNodes[node].IndexOf("#") + 1));
                    }
                }
            }

            ccHandler.sendFibs(findPathWithSubnetworks(
                nodeFrom, nodeTo, rate), usingTopology1, usingTopology2, usingTopology3, requestId, rate, false);
            ///////TODO zestawianie fibow
        }

        internal void lowerRcSendedRejectAction(int rate, String rcFrom)
        {
            lowerRcRequestedInAction--;
            if (lowerRcRequestedInAction == 0)
            {
                if (iAmDomain)
                {
                    List<String> path = findChepestNodesBetweenTwoNodes(requestNodeFrom, requestNodeTo, requestRate);
                }
                else
                {
                    sendCountedWeightsToUpperNode(rate);
                }
            }
        }

        public void releaseConnection(int requestIdToRealese)
        {
            consoleWriter("[CC] Received request for release for requestId=" + requestIdToRealese);

            ccHandler.sendRealeseFibs(requestForFibs[requestIdToRealese],requestIdToRealese);

            if (requestIdAndRcNames.ContainsKey(requestIdToRealese))
                for (int i = 0; i < requestIdAndRcNames[requestIdToRealese].Values.Count; i++)
            {
                    ccHandler.sendFIBRealeaseForSubnetwork(requestIdAndRcNames[requestIdToRealese].Values.ElementAt(i), requestIdToRealese);
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
            consoleWriter("[INIT]");
        }
    }
}
