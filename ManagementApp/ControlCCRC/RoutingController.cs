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
        private Dictionary<String, Dictionary<String, int>> wholeTopologyNodesAndConnectedNodesWithPorts;
        Dictionary<String, String> mapNodeConnectedNodeAndAssociatedRCSubnetwork;
        private List<String> pathNeededToBeCount;
        private String upperRc;
        private BinaryWriter upperWriter;

        public String requestNodeFrom;
        public String requestNodeTo;
        public int requestRate;
        private int requestId;

        private ConnectionController ccHandler;
        private Dictionary<String, BinaryWriter> socketHandler;

        private int usingTopology1 = 0;
        private int usingTopology2 = 0;
        private int usingTopology3 = 0;

        private String associatedNodeStart;
        private String associatedNodeStop;



        /**
        * DOMAIN [RC_ID]
        * SUBNETWORK [RC_ID, connect up RC] 
        */
        public RoutingController(string[] args)
        {
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
                consoleWriter(" DOMAIN NUMBER : " + domainNumber);
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
            switch(slotVC3)
            {
                case 11:
                    topologyUnallocatedLayer1[nodeName].Remove(connectedNode);
                    break;
                case 12:
                    topologyUnallocatedLayer2[nodeName].Remove(connectedNode);
                    break;
                case 13:
                    topologyUnallocatedLayer3[nodeName].Remove(connectedNode);
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
            String send_object = JMessage.Serialize(JMessage.FromValue(initMsg));
            upperWriter.Write(send_object);

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
                        case RCtoRCSignallingMessage.COUNT_ALL_PATHS_REQUEST:
                            pathNeededToBeCount = msg.AllUpperNodesToCountWeights;
                            upperRc = msg.Identifier;
                            
                            if (socketHandler.Keys.Where(id => id.StartsWith("RC_")).Count() > 0)
                            {
                                lowerRcRequestedInAction = socketHandler.Keys.Where(id => id.StartsWith("RC_")).Count();
                                Console.WriteLine("DEBUG###1: " + lowerRcRequestedInAction);
                                foreach (String id in socketHandler.Keys.Where(id => id.StartsWith("RC")))
                                {
                                    RCtoRCSignallingMessage countPathsMsg = new RCtoRCSignallingMessage();
                                    countPathsMsg.State = RCtoRCSignallingMessage.COUNT_ALL_PATHS_REQUEST;
                                    countPathsMsg.Identifier = identifier;
                                    countPathsMsg.AllUpperNodesToCountWeights = wholeTopologyNodesAndConnectedNodesWithPorts.Keys.ToList();
                                    countPathsMsg.RateToCountWeights = msg.RateToCountWeights;
                                    String dataToSend = JMessage.Serialize(JMessage.FromValue(countPathsMsg));
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
            String dataToSend = JMessage.Serialize(JMessage.FromValue(countedPathValue));
            upperWriter.Write(dataToSend);
        }


        public int findWeightBetweenTwoNodes(String startNode, String endNode, int howMuchVC3)
        {
            int result = 0;
           

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
            consoleWriter("Calculated weight total : " + result);
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
            if (firstInMyNetwork.Equals(default(String)))
                return null;
            String lastInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)).FirstOrDefault().Key;
            if (lastInMyNetwork.Equals(default(String)))
                return null;

            switch (howMuchVC3)
            {
                case 1:
                    int whichTopology = 1;
                    usingTopology1 = 1;
                    usingTopology2 = 0;
                    usingTopology3 = 0;
                    List<String> pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
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

                    if (pathRate1 != null || pathRate1.First().Equals(firstInMyNetwork) || pathRate1.Last().Equals(lastInMyNetwork))
                    {
                        foreach (var temp in pathRate1)
                        {
                            consoleWriter("[INFO] Shortest path : " + temp);
                        }
                        foreach (String node in pathRate1)
                        {
                            result.Add(node, new List<FIB>());
                        }


                        if (pathRate1.Count == 2)
                        {
                            if (topologyUnallocatedLayer1[pathRate1.ElementAt(0)][pathRate1.ElementAt(1)] > 1)
                            {
                                String virtualNodeFrom = pathRate1.ElementAt(0);
                                String virtualNodeTo = pathRate1.ElementAt(1);
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
                                 "RC: " + rcNeededToBeSet) ;
                                ccHandler.sendFIBSettingRequestForSubnetwork(virtualNodeFrom, virtualNodeTo, rcNeededToBeSet, howMuchVC3);
                                result[pathRate1[0]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][startNode],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][internalNodeFrom],
                                    whichTopology + 10
                                    ));
                                result[pathRate1[1]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[1]][internalNodeTo],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[1]][endNode],
                                    whichTopology + 10
                                    ));
                            }
                            else
                            {
                                result[pathRate1[0]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][startNode],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][pathRate1[1]],
                                    whichTopology + 10
                                    ));
                                result[pathRate1[1]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[1]][pathRate1[0]],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[1]][endNode],
                                    whichTopology + 10
                                    ));
                            }
                        }
                        else
                        {
                            int startFibSettingFrom = 1;
                            int startFibSettingTo = pathRate1.Count - 2;
                            if (topologyUnallocatedLayer1[pathRate1.ElementAt(0)][pathRate1.ElementAt(1)] > 1)
                            {
                                String virtualNodeFrom = pathRate1.ElementAt(0);
                                String virtualNodeTo = pathRate1.ElementAt(1);
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

                                result[pathRate1[0]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][startNode],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][internalNodeFrom],
                                    whichTopology + 10
                                    ));
                                result[pathRate1[1]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[1]][internalNodeTo],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[1]][pathRate1[1]],
                                    whichTopology + 10
                                    ));
                                startFibSettingFrom = 2;
                            }
                            else
                            {
                                result.First().Value.Add(new FIB(
                                   wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][startNode],
                                   whichTopology + 10,
                                   wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][pathRate1[1]],
                                   whichTopology + 10
                                   ));
                            }
                           
                            if(topologyUnallocatedLayer1[pathRate1.ElementAt(pathRate1.Count-2)][pathRate1.ElementAt(pathRate1.Count - 1)] > 1)
                            {
                                String virtualNodeFrom = pathRate1.ElementAt(0);
                                String virtualNodeTo = pathRate1.ElementAt(1);
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

                                result[pathRate1[pathRate1.Count - 2]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][pathRate1[pathRate1.Count - 3]],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[0]][internalNodeFrom],
                                    whichTopology + 10
                                    ));
                                result[pathRate1[pathRate1.Count - 1]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[1]][internalNodeTo],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[1]][endNode],
                                    whichTopology + 10
                                    ));
                                startFibSettingTo = pathRate1.Count - 3;
                            }
                            else
                            {
                                result.Last().Value.Add(new FIB(
                                                wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1.Last()][pathRate1[pathRate1.Count - 2]],
                                                whichTopology + 10,
                                                wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1.Last()][endNode],
                                                whichTopology + 10
                                                ));
                            }
                            

                            for (int i = startFibSettingFrom; i <= startFibSettingTo; i++)
                            {
                                if (topologyUnallocatedLayer1[pathRate1.ElementAt(i)][pathRate1.ElementAt(i + 1)] > 1)
                                {
                                    String virtualNodeFrom = pathRate1.ElementAt(i);
                                    String virtualNodeTo = pathRate1.ElementAt(i + 1);
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
                                    result[pathRate1[i]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i - 1]],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][internalNodeFrom],
                                    whichTopology + 10
                                    ));
                                    result[pathRate1[i + 1]].Add(new FIB(
                                   wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i + 1]][internalNodeTo],
                                   whichTopology + 10,
                                   wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i + 1]][
                                       internalNodeTo],
                                   whichTopology + 10
                                   ));

                                    i++;
                                    continue;
                                }
                                result[pathRate1[i]].Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i - 1]],
                                    whichTopology + 10,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1[i]][pathRate1[i + 1]],
                                    whichTopology + 10
                                    ));
                            }
                        }

                        

                        foreach (var temp in result)
                        {
                            foreach (var fib in temp.Value)
                            {
                                Console.WriteLine("debug: " + temp.Key + " " + fib.toString());
                            }
                        }

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
                .Where(node => node.Value.ContainsKey(startNode)).FirstOrDefault().Key;
            if (firstInMyNetwork.Equals(default(String)))
                return null;


            String lastInMyNetwork = wholeTopologyNodesAndConnectedNodesWithPorts
                .Where(node => node.Value.ContainsKey(endNode)).FirstOrDefault().Key;
            if (lastInMyNetwork.Equals(default(String)))
                return null;

            switch (howMuchVC3)
            {
                case 1:
                    int whichTopology = 1;
                    usingTopology1 = 1;
                    usingTopology2 = 0;
                    usingTopology3 = 0;
                    List<String> pathRate1 = shortest_path(firstInMyNetwork, lastInMyNetwork, topologyUnallocatedLayer1);
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

                    if (pathRate1 != null || pathRate1.Count != 0 || pathRate1.First().Equals(firstInMyNetwork) || pathRate1.Last().Equals(lastInMyNetwork))
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
                            result.Last().Value.Add(new FIB(
                                                wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1.Last()][pathRate1[pathRate1.Count - 2]],
                                                whichTopology+10,
                                                wholeTopologyNodesAndConnectedNodesWithPorts[pathRate1.Last()][endNode],
                                                whichTopology + 10
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
                    // TODO obsluga
                    List<String> path1 = shortest_path(startNode, endNode, topologyUnallocatedLayer1);
                    List<String> path2 = shortest_path(startNode, endNode, topologyUnallocatedLayer2);
                    List<String> path3 = shortest_path(startNode, endNode, topologyUnallocatedLayer3);

                    foreach (var temp in path1)
                    {
                        consoleWriter("[INFO] Shortest path : " + temp);
                    }
                    foreach (var temp in path2)
                    {
                        consoleWriter("[INFO] Shortest path : " + temp);
                    }
                    foreach (var temp in path3)
                    {
                        consoleWriter("[INFO] Shortest path : " + temp);
                    }


                    if (path1 != null && path1.First().Equals(startNode) && path1.Last().Equals(endNode) &&
                       path2 != null && path2.First().Equals(startNode) && path2.Last().Equals(endNode))
                    {
                        usingTopology1 = 1;
                        usingTopology2 = 1;
                        usingTopology3 = 0;
                        /** Builded path in 1st layer */
                        /** Builded path in 2nd layer */
                        foreach (String node in path1)
                        {
                            result.Add(node, new List<FIB>());
                        }

                        fillFibsInResultPath(result,path1,startNode,endNode,
                            usingTopology1, usingTopology2, usingTopology3);
                        
                    }
                    else if (path1 != null && path1.First().Equals(startNode) && path1.Last().Equals(endNode) &&
                             path3 != null && path3.First().Equals(startNode) && path3.Last().Equals(endNode))
                    {
                        usingTopology1 = 1;
                        usingTopology2 = 0;
                        usingTopology3 = 1;
                        /** Builded path in 1st layer */
                        /** Builded path in 3nd layer */

                        foreach (String node in path1)
                        {
                            result.Add(node, new List<FIB>());
                        }

                        fillFibsInResultPath(result, path1, startNode, endNode,
                            usingTopology1, usingTopology2, usingTopology3);

                    }
                    else if (path2 != null && path2.First().Equals(startNode) && path2.Last().Equals(endNode) &&
                             path3 != null && path3.First().Equals(startNode) && path3.Last().Equals(endNode))
                    {
                        usingTopology1 = 0;
                        usingTopology2 = 1;
                        usingTopology3 = 1;
                        /** Builded path in 2nd layer */
                        /** Builded path in 3nd layer */

                        foreach (String node in path1)
                        {
                            result.Add(node, new List<FIB>());
                        }

                        fillFibsInResultPath(result, path2, startNode, endNode,
                            usingTopology1, usingTopology2, usingTopology3);

                    }

                    break;
                case 3:
                    // TODO obsluga
                    List<String> path31 = shortest_path(startNode, endNode, topologyUnallocatedLayer1);
                    List<String> path32 = shortest_path(startNode, endNode, topologyUnallocatedLayer2);
                    List<String> path33 = shortest_path(startNode, endNode, topologyUnallocatedLayer3);
                    if (path31 != null && path31.First().Equals(startNode) && path31.Last().Equals(endNode) &&
                       path32 != null && path32.First().Equals(startNode) && path32.Last().Equals(endNode) &&
                       path33 != null && path33.First().Equals(startNode) && path33.Last().Equals(endNode))
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

                        fillFibsInResultPath(result, path31, startNode, endNode,
                            usingTopology1, usingTopology2, usingTopology3);
                    }

                    break;
                default:
                    consoleWriter("[ERROR] Wrong VC-3 number");
                    break;
            }

            return null;
        }

        private void fillFibsInResultPath(Dictionary<string, List<FIB>> result, List<string> path, String startNode, 
            String endNode, int usingTopology1, int usingTopology2, int usingTopology3)
        {
            if(usingTopology1 != 0)
            {
                result.First().Value.Add(new FIB(
                                            wholeTopologyNodesAndConnectedNodesWithPorts[path[0]][startNode],
                                            11,
                                            wholeTopologyNodesAndConnectedNodesWithPorts[path[0]][path[1]],
                                            11
                                            ));
                result.Last().Value.Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[path.Last()][path[path.Count - 2]],
                                    11,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[path.Last()][endNode],
                                    11
                                    ));

                for (int i = 1; i < path.Count - 1; i++)
                {
                    result[path[i]].Add(new FIB(
                        wholeTopologyNodesAndConnectedNodesWithPorts[path[i]][path[i - 1]],
                        11,
                        wholeTopologyNodesAndConnectedNodesWithPorts[path[i]][path[i + 1]],
                        11
                        ));
                }

                foreach (var temp in result)
                {
                    foreach (var fib in temp.Value)
                    {
                        Console.WriteLine("debug: " + temp.Key + " " + fib.toString());
                    }
                }
            }
            if(usingTopology2 != 0)
            {
                result.First().Value.Add(new FIB(
                                           wholeTopologyNodesAndConnectedNodesWithPorts[path[0]][startNode],
                                           12,
                                           wholeTopologyNodesAndConnectedNodesWithPorts[path[0]][path[1]],
                                           12
                                           ));
                result.Last().Value.Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[path.Last()][path[path.Count - 2]],
                                    12,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[path.Last()][endNode],
                                    12
                                    ));

                for (int i = 1; i < path.Count - 1; i++)
                {
                    result[path[i]].Add(new FIB(
                        wholeTopologyNodesAndConnectedNodesWithPorts[path[i]][path[i - 1]],
                        12,
                        wholeTopologyNodesAndConnectedNodesWithPorts[path[i]][path[i + 1]],
                        12
                        ));
                }

                foreach (var temp in result)
                {
                    foreach (var fib in temp.Value)
                    {
                        Console.WriteLine("debug: " + temp.Key + " " + fib.toString());
                    }
                }
            }
            if(usingTopology3 != 0)
            {
                result.First().Value.Add(new FIB(
                                           wholeTopologyNodesAndConnectedNodesWithPorts[path[0]][startNode],
                                           13,
                                           wholeTopologyNodesAndConnectedNodesWithPorts[path[0]][path[1]],
                                           13
                                           ));
                result.Last().Value.Add(new FIB(
                                    wholeTopologyNodesAndConnectedNodesWithPorts[path.Last()][path[path.Count - 2]],
                                    13,
                                    wholeTopologyNodesAndConnectedNodesWithPorts[path.Last()][endNode],
                                    13
                                    ));

                for (int i = 1; i < path.Count - 1; i++)
                {
                    result[path[i]].Add(new FIB(
                        wholeTopologyNodesAndConnectedNodesWithPorts[path[i]][path[i - 1]],
                        13,
                        wholeTopologyNodesAndConnectedNodesWithPorts[path[i]][path[i + 1]],
                        13
                        ));
                }

                foreach (var temp in result)
                {
                    foreach (var fib in temp.Value)
                    {
                        Console.WriteLine("debug: " + temp.Key + " " + fib.toString());
                    }
                }
            }
            
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
            consoleWriter("INIT FROM: " + nodeName);
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

            Address adr = new Address(connectedNode);
            if (iAmDomain && adr.domain != domainNumber)
            {
                ccHandler.sendBorderNodesToNCC(adr);
            }
        }

        public void deleteTopologyElementFromLRM(String whoDied)
        {
            
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
        }

        public void initConnectionRequestFromCC(String nodeFrom, String nodeTo, int rate, int requestId)
        {
            this.requestNodeFrom = nodeFrom;
            this.requestNodeTo = nodeTo;
            this.requestRate = rate;
            this.requestId = requestId;
            lowerRcRequestedInAction = socketHandler.Keys.Where(id => id.StartsWith("RC_")).ToList().Count();
            Console.WriteLine("DEBUG###2: " + lowerRcRequestedInAction);

            if(lowerRcRequestedInAction == 0)
            {
                foreach(String node in wholeTopologyNodesAndConnectedNodesWithPorts.Keys)
                {
                    foreach(String connectedNode in wholeTopologyNodesAndConnectedNodesWithPorts[node].Keys)
                    {
                        consoleWriter("[DEBUG] Node:" + node + " connected with " + connectedNode + " path[" +
                            wholeTopologyNodesAndConnectedNodesWithPorts[node][connectedNode]+ "]");
                    }
                }
                ccHandler.sendFibs(findPath(nodeFrom, nodeTo, rate), usingTopology1, usingTopology2, usingTopology3,requestId);
            }

            foreach (String id in socketHandler.Keys.Where(id => id.StartsWith("RC_")))
            {
                consoleWriter("Sending info to compute path to lower rc: " + id);
                RCtoRCSignallingMessage countPathsMsg = new RCtoRCSignallingMessage();
                countPathsMsg.State = RCtoRCSignallingMessage.COUNT_ALL_PATHS_REQUEST;
                countPathsMsg.Identifier = identifier;
                countPathsMsg.AllUpperNodesToCountWeights = wholeTopologyNodesAndConnectedNodesWithPorts.Keys.ToList();
                countPathsMsg.RateToCountWeights = rate;
                String dataToSend = JMessage.Serialize(JMessage.FromValue(countPathsMsg));
                socketHandler[id].Write(dataToSend);
            }
        }

        public void lowerRcSendedConnectionsAction(Dictionary<string, Dictionary<string, int>> nodeConnectionsAndWeights, 
            Dictionary<string, string> associatedNodes, int rate, String rcFrom)
        {
            Console.WriteLine("DEBUG###3: " + lowerRcRequestedInAction);
            lowerRcRequestedInAction--;
            consoleWriter("Received counted weigths from " + rcFrom);
            foreach(String node in nodeConnectionsAndWeights.Keys)
            {
                foreach(String connected in nodeConnectionsAndWeights[node].Keys)
                {
                    consoleWriter("debug:" + node + ":" + connected + " weight:" + nodeConnectionsAndWeights[node][connected]);
                }
                
            }
            foreach (String node in associatedNodes.Keys)
            {
                foreach (String connected in nodeConnectionsAndWeights[node].Keys)
                {
                    consoleWriter("debug associated:" + node + ":" + connected + " between:" + nodeConnectionsAndWeights[node][connected]);
                }

            }
            foreach (String node in nodeConnectionsAndWeights.Keys)
            {
                for (int i = 0; i < nodeConnectionsAndWeights[node].Count; i++)
                {
                    if (!wholeTopologyNodesAndConnectedNodesWithPorts[node].ContainsKey(nodeConnectionsAndWeights[node].Keys.ElementAt(i)))
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
                        foreach(String mapNode in mapNodeConnectedNodeAndAssociatedRCSubnetwork.Keys)
                        {
                            consoleWriter("MAPNODE DEBUG : "
                                + mapNode + " :: " + mapNodeConnectedNodeAndAssociatedRCSubnetwork[mapNode]);
                        }
                        consoleWriter("Associated " + nodeConnectionsAndWeights[node].Keys.ElementAt(i) +
                            " -> " + node + " weight " + nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        consoleWriter("Associated : " +
                            node + "#" +
                            nodeConnectionsAndWeights[node].Keys.ElementAt(i) + " and " + rcFrom);
                    }
                }
            }

            Console.WriteLine("DEBUG###4: " + lowerRcRequestedInAction);
            if (lowerRcRequestedInAction == 0)
            {
                if (iAmDomain)
                {
                    consoleWriter("Calculating shortest path between:" + requestNodeFrom + " and " + requestNodeTo);
                    List<String> path = findChepestNodesBetweenTwoNodes(requestNodeFrom, requestNodeTo, requestRate);
                    if(path == null || path.Count == 0)
                    {
                        consoleWriter("[ERROR] upper path can noc be found");
                        return;
                    }
                    consoleWriter("debug cheapest path calculated:");
                    foreach(String node in path)
                    {
                        consoleWriter(node);
                    }

                    if(mapNodeConnectedNodeAndAssociatedRCSubnetwork.ContainsKey(requestNodeFrom + "#"+ path[0]))
                    {
                        lowerRcRequestedInAction++;
                        string temp;
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.TryGetValue(requestNodeFrom + "#" + path[0], out temp);
                        Console.WriteLine("DEBUG###: " + temp);

                        ccHandler.sendRequestToSubnetworkCCToBuildPath(temp, requestNodeFrom, path[0], rate);
                    }
                    for(int i = 0; i < path.Count-1; i++)
                    {
                        lowerRcRequestedInAction++;
                        string temp;
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.TryGetValue(path[i] + "#" + path[i+1], out temp);
                        Console.WriteLine("DEBUG###: " + temp);

                        ccHandler.sendRequestToSubnetworkCCToBuildPath(temp, path[i], path[i+1], rate);
                    }
                    if (mapNodeConnectedNodeAndAssociatedRCSubnetwork.ContainsKey(path.Last() + "#" + requestNodeTo))
                    {
                        lowerRcRequestedInAction++;
                        string temp;
                        mapNodeConnectedNodeAndAssociatedRCSubnetwork.TryGetValue(path.Last() + "#" + requestNodeTo, out temp);
                        Console.WriteLine("DEBUG###: " + temp);

                        ccHandler.sendRequestToSubnetworkCCToBuildPath(temp, path.Last(), requestNodeTo, rate);
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
            consoleWriter("Domain starting fib setting");
            Dictionary<String, String> nodeHashtagNodeAndNodeInSubnetwork = new Dictionary<string, string>();
            foreach (String node in nodeConnectionsAndWeights.Keys)
            {
                for (int i = 0; i < nodeConnectionsAndWeights[node].Count; i++)
                {
                    if (!wholeTopologyNodesAndConnectedNodesWithPorts[node].ContainsKey(nodeConnectionsAndWeights[node].Keys.ElementAt(i)))
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
                        foreach (String mapNode in mapNodeConnectedNodeAndAssociatedRCSubnetwork.Keys)
                        {
                            consoleWriter("MAPNODE DEBUG : "
                                + mapNode + " :: " + mapNodeConnectedNodeAndAssociatedRCSubnetwork[mapNode]);
                        }
                        consoleWriter("Associated " + nodeConnectionsAndWeights[node].Keys.ElementAt(i) +
                            " -> " + node + " weight " + nodeConnectionsAndWeights[node].Values.ElementAt(i));
                        consoleWriter("Associated : " +
                            node + "#" +
                            nodeConnectionsAndWeights[node].Keys.ElementAt(i) + " and " + rcFrom);
                    }
                }
            }

            ccHandler.sendFibs(findPathWithSubnetworks(
                nodeFrom, nodeTo, rate), usingTopology1, usingTopology2, usingTopology3, requestId);
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
