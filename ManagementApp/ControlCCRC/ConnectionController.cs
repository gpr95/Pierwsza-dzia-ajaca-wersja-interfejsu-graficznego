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
            iAmDomain = (args.Length == 1);
            identifier = args[0];

            if (iAmDomain)
            {
                consoleWriter("[INIT] DOMAIN");
                identifier = "DOMAIN_" + identifier;
                try
                {
                    NCCClient = new TcpClient("localhost", Convert.ToInt32(args[1]));
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
                    CCClient = new TcpClient("localhost", Convert.ToInt32(args[1]));
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
                    //@TODO communication with upper CC
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
    }
}
