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
        private TcpListener CCListener;
        private TcpClient CCClient;
        private TcpClient NCCClient;

        private Thread threadListenCC;
        private Thread threadconnectCC;
        private Thread threadconnectNCC;

        private RoutingController rcHandler;

        private Dictionary<String, CCThread> lastCCNodes;

        private Boolean iAmDomain;
        /**
         * DOMAIN [listen CC, connect NCC]
         * SUBNETWORK [listen CC, connect up CC, JUST_FLAG]
         */
        public ConnectionController(string[] args)
        {
            iAmDomain = (args.Length == 2);
            lastCCNodes = new Dictionary<string, CCThread>();

            if(iAmDomain)
            {
                consoleWriter("[INIT] DOMAIN");
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

            this.CCListener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[0]));
            this.threadListenCC = new Thread(new ThreadStart(ccListen));
            threadListenCC.Start();

            consoleStart();
        }

        public void setRCHandler(RoutingController rc)
        {
            this.rcHandler = rc;
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
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type != typeof(CCtoNCCSingallingMessage))
                        noError = false;
                    CCtoNCCSingallingMessage msg = received_object.Value.ToObject<CCtoNCCSingallingMessage>();
                    switch(msg.State)
                    {
                        case CCtoNCCSingallingMessage.NCC_SET_CONNECTION:
                            Dictionary<String,List<FIB>> fibs = rcHandler.findPath(msg.NodeFrom, msg.NodeTo, msg.Rate);
                            if(fibs != null)
                                for(int i = 0; i<fibs.Count; i++)
                                {
                                    foreach(FIB fib in fibs[fibs.Keys.ElementAt(i)])
                                    {
                                        lastCCNodes[fibs.Keys.ElementAt(i)].writeFIB(fib);
                                    }
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

        private void ccConnect()
        {
            BinaryReader reader = new BinaryReader(CCClient.GetStream());

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

        private void ccListen()
        {
            this.CCListener.Start();

            Boolean noError = true;
            while (noError)
            {
                try
                {
                    TcpClient client = CCListener.AcceptTcpClient();
                    CCThread thread = new CCThread(client, ref lastCCNodes);
                }
                catch (SocketException ex)
                {
                    consoleWriter("[ERROR] Socket failed. CC Listener.");
                    noError = false;
                }
            }
        }

        private void consoleStart()
        {
            consoleWriter("[INIT] CC started.");
        }
        private void consoleWriter(String msg)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:" + msg);
            Console.Write(Environment.NewLine);
        }
    }

    class CCThread
    {
        private Thread thread;
        private BinaryWriter writer;
        private Dictionary<String, CCThread> lastCCNodes;
        private String nodeName;
        public CCThread(TcpClient con, ref Dictionary<String, CCThread> lastCCNodes)
        {
            this.lastCCNodes = lastCCNodes;
            thread = new Thread(new ParameterizedThreadStart(ccListen));
            thread.Start(con);
        }

        private void ccListen(Object con)
        {
            TcpClient ccClient = (TcpClient)con;
            BinaryReader reader = new BinaryReader(ccClient.GetStream());
            writer = new BinaryWriter(ccClient.GetStream());

            Boolean noError = true;
            Boolean lastCC = false;
            while (noError)
            {
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type != typeof(CCtoCCSignallingMessage))
                {
                    consoleWriter("[ERROR] Received wrong data format.");
                    return;
                }

                CCtoCCSignallingMessage msg = received_object.Value.ToObject<CCtoCCSignallingMessage>();
                lastCC = msg.LastCC;

                if(lastCC)
                {
                    //@TODO communication with NetNode CC
                    switch(msg.State)
                    {
                        case CCtoCCSignallingMessage.CC_LOW_INIT:
                            nodeName = msg.NodeName;
                            lastCCNodes.Add(nodeName, this);
                            break;

                    }
                }
                else
                {
                    //@TODO communication with subnetwork CC
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

        internal void writeFIB(FIB fib)
        {
            try
            {
                CCtoCCSignallingMessage msg = new CCtoCCSignallingMessage();
                msg.State = CCtoCCSignallingMessage.CC_UP_FIB_CHANGE;
                string data = JMessage.Serialize(JMessage.FromValue(msg));
                writer.Write(data);
            }
            catch (Exception e)
            {
                consoleWriter("[ERROR] Sending FIB failed");
            }
        }
    }
}
