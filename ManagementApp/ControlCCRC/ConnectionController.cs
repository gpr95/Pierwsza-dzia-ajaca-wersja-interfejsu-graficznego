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
    class ConnectionController
    {
        private String identifier;

        private TcpClient CCClient;
        private TcpClient NCCClient;

        private Thread threadconnectCC;
        private Thread threadconnectNCC;

        private RoutingController rcHandler;
        private Dictionary<String, BinaryWriter> socketHandler;

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
                consoleWriter("[INIT] DOMAIN");
                identifier = "DOMAIN_" + identifier;
                try
                {
                    int nccPort;
                    int.TryParse(args[1], out nccPort);
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
                consoleWriter("[INIT] SUBNETWORK");
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
            BinaryWriter writer = new BinaryWriter(NCCClient.GetStream());


            CCtoNCCSingallingMessage initMsg = new CCtoNCCSingallingMessage();
            initMsg.State = CCtoNCCSingallingMessage.INIT_FROM_CC;
            String dataToSend = JMessage.Serialize(JMessage.FromValue(initMsg));


            Boolean noError = true;
            while (noError)
            {
                try
                {
                    string received_data = reader.ReadString();
                    socketHandler.Add("NCC", writer);
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type != typeof(CCtoNCCSingallingMessage))
                        noError = false;
                    CCtoNCCSingallingMessage msg = received_object.Value.ToObject<CCtoNCCSingallingMessage>();
                    switch (msg.State)
                    {
                        // POPRAWIC
                        case CCtoNCCSingallingMessage.NCC_SET_CONNECTION:
                            rcHandler.initConnectionRequestFromCC(msg.NodeFrom, msg.NodeTo, msg.Rate);
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
            BinaryWriter writer = new BinaryWriter(CCClient.GetStream());


            CCtoCCSignallingMessage initMsg = new CCtoCCSignallingMessage();
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
                    if (received_object.Type != typeof(CCtoCCSignallingMessage))
                        noError = false;
                    CCtoCCSignallingMessage msg = received_object.Value.ToObject<CCtoCCSignallingMessage>();
                    
                    switch(msg.State)
                    {
                        case CCtoCCSignallingMessage.CC_BUILD_PATH_REQUEST:
                            ///////////////////////////////////
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
            CCtoCCSignallingMessage ccRequest = new CCtoCCSignallingMessage();
            ccRequest.State = CCtoCCSignallingMessage.CC_BUILD_PATH_REQUEST;
            ccRequest.NodeFrom = nodeFrom;
            ccRequest.NodeTo = nodeTo;
            ccRequest.Rate = rate;

            String dataToSend = JMessage.Serialize(JMessage.FromValue(ccRequest));
            socketHandler["CC_" + rcName.Substring(rcName.IndexOf("_") + 1)].Write(dataToSend);

        }
    }
}
