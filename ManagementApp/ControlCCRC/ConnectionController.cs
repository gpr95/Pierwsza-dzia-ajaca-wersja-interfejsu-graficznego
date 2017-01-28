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
using ManagementApp;

namespace ControlCCRC
{
    class ConnectionController
    {
        private String identifier;

        private TcpClient CCClient;
        private TcpClient NCCClient;

        private Thread threadconnectCC;
        private BinaryWriter ccWriter;
        private Thread threadconnectNCC;
        private BinaryWriter nccWriter;

        private RoutingController rcHandler;
        private Dictionary<String, BinaryWriter> socketHandler;

        private int lowerCcRequestedInAction;
        private Boolean iAmDomain;
        /**
         * DOMAIN [CC_ID, connect NCC]
         * SUBNETWORK [CC_ID, connect up CC, flag] 
         */
        public ConnectionController(string[] args)
        {
            iAmDomain = (args.Length == 2);
            identifier = args[0];

            if (iAmDomain)
            {
                consoleWriter("[INIT] DOMAIN - " + identifier);
                try
                {
                    int nccPort;
                    int.TryParse(args[1], out nccPort);
                    Thread.Sleep(500);
                    NCCClient = new TcpClient("localhost", nccPort);
                }
                catch (SocketException ex)
                {
                    consoleWriter("[ERROR] Cannot connect with NCC.");
                }
                this.threadconnectNCC = new Thread(new ThreadStart(nccConnect));
                threadconnectNCC.Start();
            }
            else
            {
                consoleWriter("[INIT] SUBNETWORK - " + identifier);
                try
                {
                    int ccPort;
                    int.TryParse(args[1], out ccPort);
                    CCClient = new TcpClient("localhost", ccPort);
                }
                catch (SocketException ex)
                {
                    consoleWriter("[ERROR] Cannot connect with upper CC.");
                }
                this.threadconnectCC = new Thread(new ThreadStart(ccConnect));
                threadconnectCC.Start();
            }


            consoleStart();
        }

        public void setRCHandler(RoutingController rc)
        {
            this.rcHandler = rc;
        }

        public void setSocketHandler(Dictionary<String, BinaryWriter> socketHandler)
        {
            this.socketHandler = socketHandler;
        }

        private void nccConnect()
        {
            BinaryReader reader = new BinaryReader(NCCClient.GetStream());
            nccWriter = new BinaryWriter(NCCClient.GetStream());


            CCtoNCCSingallingMessage initMsg = new CCtoNCCSingallingMessage();
            initMsg.State = CCtoNCCSingallingMessage.INIT_FROM_CC;
            String dataToSend = JMessage.Serialize(JMessage.FromValue(initMsg));
            nccWriter.Write(dataToSend);
            socketHandler.Add("NCC", nccWriter);

            Boolean noError = true;
            while (noError)
            {
                try
                {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type != typeof(CCtoNCCSingallingMessage))
                        noError = false;
                    CCtoNCCSingallingMessage msg = received_object.Value.ToObject<CCtoNCCSingallingMessage>();
                    switch (msg.State)
                    {
                        // POPRAWIC
                        case CCtoNCCSingallingMessage.NCC_SET_CONNECTION:
                            rcHandler.initConnectionRequestFromCC(msg.NodeFrom, msg.NodeTo, msg.Rate, msg.RequestID);
                            break;
                    }
                }
                catch (IOException ex)
                {
                    noError = false;
                }
            }
        }

        private void ccConnect()
        {
            BinaryReader reader = new BinaryReader(CCClient.GetStream());
            ccWriter = new BinaryWriter(CCClient.GetStream());


            CCtoCCSignallingMessage initMsg = new CCtoCCSignallingMessage();
            initMsg.Identifier = identifier;
            initMsg.State = CCtoCCSignallingMessage.CC_MIDDLE_INIT;
            String send_object = JMessage.Serialize(JMessage.FromValue(initMsg));
            ccWriter.Write(send_object);


            Boolean noError = true;
            while (noError)
            {
                try
                {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type != typeof(CCtoCCSignallingMessage))
                        noError = false;
                    CCtoCCSignallingMessage msg = received_object.Value.ToObject<CCtoCCSignallingMessage>();
                    
                    switch(msg.State)
                    {
                        case CCtoCCSignallingMessage.CC_BUILD_PATH_REQUEST:
                            Console.WriteLine("received CC_BUILD_PATH_REQUEST: " + this.identifier);
                           // lowerCcRequestedInAction = socketHandler.Keys.Where(id => id.StartsWith("CC_")).ToList().Count();
                            rcHandler.initConnectionRequestFromCC(msg.NodeFrom, msg.NodeTo, msg.Rate, 0);
                            break;
                        case CCtoCCSignallingMessage.FIB_SETTING_TOP_BOTTOM:
                            rcHandler.startProperWeigthComputingTopBottom(new Dictionary<string, Dictionary<string, int>>(),
                                      new Dictionary<string, string>(), msg.Rate, "",
                                      msg.NodeFrom, msg.NodeTo);
                            break;
                    }
                }
                catch (IOException ex)
                {
                    noError = false;
                }
            }
        }

        public void setFibsFromRC(Dictionary<String, List<FIB>> fibs)
        {
            if (fibs != null)
                for (int i = 0; i < fibs.Count; i++)
                {
                   // socketHandler[fibs.Keys.ElementAt(i)].writeFIB(fibs.Values.ElementAt(i));
                }
            else
                consoleWriter("[ERROR] FIBS null - connection can't be made.");
        }

        private void consoleStart()
        {
            consoleWriter("[INIT] Started.");
        }
        private void consoleWriter(String msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:[CC]" + msg);
            Console.Write(Environment.NewLine);
        }

        internal void sendRequestToSubnetworkCCToBuildPath(string rcName, string nodeFrom, string nodeTo, int rate)
        {
            Console.WriteLine("CC_BUILD_PATH_REQUEST to: " + rcName);
            CCtoCCSignallingMessage ccRequest = new CCtoCCSignallingMessage();
            ccRequest.State = CCtoCCSignallingMessage.CC_BUILD_PATH_REQUEST;
            ccRequest.NodeFrom = nodeFrom;
            ccRequest.NodeTo = nodeTo;
            ccRequest.Rate = rate;

            String dataToSend = JMessage.Serialize(JMessage.FromValue(ccRequest));
            socketHandler["CC_" + rcName.Substring(rcName.IndexOf("_") + 1)].Write(dataToSend);

        }

        internal void sendFibs(Dictionary<string, List<FIB>> dictionary, int using1, int using2, int using3, int requestId)
        {
            if (dictionary != null)
            {
                foreach (string nodeName in dictionary.Keys)
                {
                    CCtoCCSignallingMessage fibsMsg = new CCtoCCSignallingMessage();
                    fibsMsg.State = CCtoCCSignallingMessage.CC_UP_FIB_CHANGE;
                    fibsMsg.Fib_table = dictionary[nodeName];
                    String dataOut = JMessage.Serialize(JMessage.FromValue(fibsMsg));
                    socketHandler[nodeName].Write(dataOut);
                }
            }
            if(iAmDomain)
            {
                if (dictionary != null)
                {
                    CCtoNCCSingallingMessage finishMsg = new CCtoNCCSingallingMessage();
                    finishMsg.State = CCtoNCCSingallingMessage.CC_CONFIRM;
                    finishMsg.Vc11 = using1;
                    finishMsg.Vc12 = using2;
                    finishMsg.Vc13 = using3;
                    finishMsg.NodeTo = rcHandler.myBorderNodeAndConnectedOtherBorderNodeMap[dictionary.Keys.First()];
                    finishMsg.RequestID = requestId;
                    String dataToSend = JMessage.Serialize(JMessage.FromValue(finishMsg));
                    nccWriter.Write(dataToSend);
                }
                else
                {
                    CCtoNCCSingallingMessage finishMsg = new CCtoNCCSingallingMessage();
                    finishMsg.State = CCtoNCCSingallingMessage.CC_REJECT;
                    finishMsg.Vc11 = using1;
                    finishMsg.Vc12 = using2;
                    finishMsg.Vc13 = using3;
                    finishMsg.RequestID = requestId;
                    String dataToSend = JMessage.Serialize(JMessage.FromValue(finishMsg));
                    nccWriter.Write(dataToSend);
                }
            }
            else
            {
                if (dictionary != null)
                {
                    CCtoNCCSingallingMessage finishMsg = new CCtoNCCSingallingMessage();
                    finishMsg.State = CCtoNCCSingallingMessage.CC_CONFIRM;
                    finishMsg.Vc11 = using1;
                    finishMsg.Vc12 = using2;
                    finishMsg.Vc13 = using3;
                    finishMsg.RequestID = requestId;
                    String dataToSend = JMessage.Serialize(JMessage.FromValue(finishMsg));
                    ccWriter.Write(dataToSend);
                }
                else
                {
                    CCtoNCCSingallingMessage finishMsg = new CCtoNCCSingallingMessage();
                    finishMsg.State = CCtoNCCSingallingMessage.CC_REJECT;
                    finishMsg.RequestID = requestId;
                    String dataToSend = JMessage.Serialize(JMessage.FromValue(finishMsg));
                    ccWriter.Write(dataToSend);
                }
            }

        }

        public void sendBorderNodesToNCC(Address adr)
        {
            CCtoNCCSingallingMessage borderNodeMsg = new CCtoNCCSingallingMessage();
            borderNodeMsg.State = CCtoNCCSingallingMessage.BORDER_NODE;
            borderNodeMsg.BorderNode = adr.getName();
            String dataToSend = JMessage.Serialize(JMessage.FromValue(borderNodeMsg));
            nccWriter.Write(dataToSend);
        }

        public void sendFIBSettingRequestForSubnetwork(String nodeFrom, String nodeTo, String rcName,int rate)
        {
            String ccName = rcName.Replace("RC", "CC"); ;
            CCtoCCSignallingMessage setFIBmsg = new CCtoCCSignallingMessage();
            setFIBmsg.State = CCtoCCSignallingMessage.FIB_SETTING_TOP_BOTTOM;
            setFIBmsg.NodeFrom = nodeFrom;
            setFIBmsg.NodeTo = nodeTo;
            setFIBmsg.Rate = rate;
            String dataToSend = JMessage.Serialize(JMessage.FromValue(setFIBmsg));
            socketHandler[ccName].Write(dataToSend);
        }
    }
}
