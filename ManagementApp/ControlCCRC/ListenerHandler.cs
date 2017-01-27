using ClientWindow;
using ControlCCRC.Protocols;
using Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCCRC
{
    class ListenerHandler
    {
        private Thread thread;
        private String identifier;
        private bool lastNode;
        private BinaryWriter writer;

        private TcpClient client;
        private RoutingController rc;
        private ConnectionController cc;
        private Dictionary<String, ListenerHandler> socketHandler;



        public ListenerHandler(TcpClient client, RoutingController rc, ConnectionController cc,ref Dictionary<String, ListenerHandler> socketHandler)
        {
            this.client = client;
            this.rc = rc;
            this.cc = cc;
            this.socketHandler = socketHandler;

            thread = new Thread(new ParameterizedThreadStart(handleThread));
            thread.Start(client);
        }

        private void handleThread(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            BinaryReader reader = new BinaryReader(client.GetStream());
            writer = new BinaryWriter(client.GetStream());
            Boolean noError = true;
            while (noError)
            {
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(RCtoLRMSignallingMessage))
                {
                    RCtoLRMSignallingMessage lrmMsg = received_object.Value.ToObject<RCtoLRMSignallingMessage>();
                    switch (lrmMsg.State)
                    {
                        case RCtoLRMSignallingMessage.LRM_INIT:
                            identifier = lrmMsg.NodeName;
                            rc.initLRMNode(identifier);
                            socketHandler.Add(identifier, this);
                            break;
                        case RCtoLRMSignallingMessage.LRM_TOPOLOGY_ADD:
                            rc.addTopologyElementFromLRM(identifier, lrmMsg.ConnectedNode, lrmMsg.ConnectedNodePort);
                            break;
                        case RCtoLRMSignallingMessage.LRM_TOPOLOGY_DELETE:
                            rc.deleteTopologyElementFromLRM(lrmMsg.ConnectedNode);
                            break;
                    }
                }
                else if (received_object.Type == typeof(RCtoRCSignallingMessage))
                {

                }
                else if (received_object.Type == typeof(CCtoCCSignallingMessage))
                {

                }
            }
        }
        public bool isLastNode()
        {
            return lastNode;
        }

        public void writeFIB(List<FIB> fibs)
        {
            CCtoCCSignallingMessage msg = new CCtoCCSignallingMessage();
            msg.Fib_table = fibs;
            msg.State = CCtoCCSignallingMessage.CC_UP_FIB_CHANGE;

            String send_object = JMessage.Serialize(JMessage.FromValue(msg));
            writer.Write(send_object);
        }
    }
}
