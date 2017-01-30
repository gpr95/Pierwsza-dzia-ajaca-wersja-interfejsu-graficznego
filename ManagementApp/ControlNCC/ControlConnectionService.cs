using ClientNode;
using ClientWindow;
using ControlCCRC.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagementApp;

namespace ControlNCC
{
    class ControlConnectionService
    {
        private TcpClient client;
        private BinaryWriter writer;
        private NetworkCallControl handlerNCC;
        private TcpClient connection;
        private Thread thread;
        private string ip;
        BinaryWriter writerToCC;
        int counter;

        public ControlConnectionService(TcpClient clientHandler, NetworkCallControl handlerNCC)
        {
            this.client = clientHandler;
            this.handlerNCC = handlerNCC;
            init(client);
            ip = "127.0.0.1";
            counter = 4;

        }

        private void init(TcpClient client)
        {
            Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
            clientThread.Start(client);
        }

        private void ListenThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            writer = new BinaryWriter(clienttmp.GetStream());
            while (true)
            {
                try
                {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type == typeof(ControlPacket))
                    {
                        ControlPacket packet = received_object.Value.ToObject<ControlPacket>();
                        if (packet.virtualInterface == ControlInterface.CALL_REQUEST)
                        {
                            handlerNCC.addService(packet.RequestID, this);
                            Console.WriteLine("[CPCC]Receive call request for " + packet.destinationIdentifier + " on " + ControlInterface.CALL_REQUEST_ACCEPT + " interface");
                            Console.WriteLine("[DIRECTORY]Send directory request");
                            if (handlerNCC.checkIfInDirectory(packet.destinationIdentifier))
                            {
                                Console.WriteLine("[DIRECTORY]Receive local name");
                                Console.WriteLine("[POLICY]Send policy out");
                                Console.WriteLine("[POLICY]Call accept");
                                Console.WriteLine("[CPCC]Send call indication");
                                Console.WriteLine("[CPCC]Call confirmed");
                                Console.WriteLine("[CC]Send connection request");
                                CCtoNCCSingallingMessage packetToCC = new CCtoNCCSingallingMessage();
                                packetToCC.State = CCtoNCCSingallingMessage.NCC_SET_CONNECTION;
                                packetToCC.NodeFrom = packet.originIdentifier;
                                packetToCC.NodeTo = packet.destinationIdentifier;
                                packetToCC.Rate = packet.speed;
                                packetToCC.RequestID = packet.RequestID;
                                packetToCC.Vc11 = 1;
                                packetToCC.Vc12 = 1;
                                packetToCC.Vc13 = 1;
                                ControlConnectionService CCService = this.handlerNCC.getCCService();
                                CCService.sendCCRequest(packetToCC);
                            }
                            else
                            {
                                Console.WriteLine("[DIRECTORY]This client is not in my network");
                                Address address = new Address(packet.destinationIdentifier);
                                ControlConnectionService serviceToNCC = handlerNCC.getService(address.domain);
                                handlerNCC.addCNAddressesForInterdomainCalls(packet.RequestID, packet.originIdentifier);

                                Address addressFromOtherDomain = new Address(packet.destinationIdentifier);
                                List<string> borderGWAddresses = new List<string>();
                                borderGWAddresses = handlerNCC.returnBorderGateway(addressFromOtherDomain.domain);
                                string borderGWAddress = borderGWAddresses.First();
                                handlerNCC.initInterdomanCallTask(packet.RequestID, borderGWAddress);
                                ControlPacket packetToNCC = new ControlPacket(ControlInterface.CALL_INDICATION, ControlPacket.IN_PROGRESS, packet.speed, packet.destinationIdentifier, borderGWAddress, packet.RequestID);
                                packetToNCC.domain = handlerNCC.domainNumber;
                                packetToNCC.Vc11 = 1;
                                packetToNCC.Vc12 = 1;
                                packetToNCC.Vc13 = 1;
                                serviceToNCC.send(packetToNCC);
                                Console.WriteLine("[NCC]Send call request to next NCC, destination: " + packet.destinationIdentifier + " origin(BG address): " + borderGWAddress);
                            }

                        }
                        else if (packet.virtualInterface == ControlInterface.CALL_RELEASE_IN)
                        {
                            //RELEASE
                            int id = packet.RequestID;
                            Console.WriteLine("[CPCC] Release connection id: " + id);

                            if (!handlerNCC.checkIfInterdomainRequest(id))
                            {
                                Console.WriteLine("[CC]Send connection release");
                                CCtoNCCSingallingMessage packetToCC = new CCtoNCCSingallingMessage();
                                packetToCC.State = CCtoNCCSingallingMessage.NCC_RELEASE_WITH_ID;
                                packetToCC.RequestID = packet.RequestID;
                                ControlConnectionService CCService = this.handlerNCC.getCCService();
                                CCService.sendCCRequest(packetToCC);
                            }
                            else
                            {
                                Console.WriteLine("[CC]Send connection release");
                                CCtoNCCSingallingMessage packetToCC = new CCtoNCCSingallingMessage();
                                packetToCC.State = CCtoNCCSingallingMessage.NCC_RELEASE_WITH_ID;
                                packetToCC.RequestID = packet.RequestID;
                                ControlConnectionService CCService = this.handlerNCC.getCCService();
                                CCService.sendCCRequest(packetToCC);
                                //////
                                //Address address = new Address(packet.destinationIdentifier);
                                //ControlConnectionService serviceToNCC = handlerNCC.getService(address.domain);
                                //ControlPacket packetToNCC = new ControlPacket(ControlInterface.CALL_INDICATION, ControlPacket.REJECT, 0, packet.destinationIdentifier, packet.originIdentifier, id);
                                //serviceToNCC.send(packetToNCC);
                                //Console.WriteLine("[NCC]Send call release to next NCC: ");
                            }
                        }

                        else if (packet.virtualInterface == ControlInterface.NETWORK_CALL_COORDINATION_IN)
                        {
                            Console.WriteLine("[NCC] Receive NCC invitation from NCC in domain" + packet.RequestID);
                            handlerNCC.addService(packet.RequestID, this);
                            ControlPacket packetToNCCResponse = new ControlPacket(ControlInterface.NETWORK_CALL_COORDINATION_OUT, ControlPacket.IN_PROGRESS, 0, "", "", handlerNCC.domainNumber);
                            send(packetToNCCResponse);
                            Console.WriteLine("[NCC] Send invitation response to NCC in domain" + packetToNCCResponse.RequestID);
                        }
                        else if (packet.virtualInterface == ControlInterface.NETWORK_CALL_COORDINATION_OUT)
                        {
                            Console.WriteLine("[NCC] NCC handshake completed with NCC in doman" + packet.RequestID);
                            handlerNCC.addService(packet.RequestID, this);
                        }
                        else if (packet.virtualInterface == ControlInterface.CALL_INDICATION)
                        {
                            if (packet.state == ControlPacket.IN_PROGRESS)
                            {
                                Console.WriteLine("[NCC] Recived request to setup call from " + packet.originIdentifier + " to: " + packet.destinationIdentifier);
                                if (handlerNCC.interdomainRequests.ContainsKey(packet.RequestID))
                                {
                                    //Console.WriteLine("KOLEJNA PROBA Z INNYM GW: " + packet.originIdentifier);
                                }
                                else
                                handlerNCC.addInterdomainRequest(packet.RequestID, packet.domain);
                                // ZAKLADAMY TU ZE KAZDE NCC MA HANDLER NA INNE, INACZEJ SPRAWDZ DOMENE CZY TWOJA, NIE TO SLIJ DALEJ
                                Console.WriteLine("[DIRECTORY]Receive local name");
                                Console.WriteLine("[POLICY]Send policy out");
                                Console.WriteLine("[POLICY]Call accept");
                                Console.WriteLine("[CPCC]Send call indication");
                                Console.WriteLine("[CPCC]Call confirmed");
                                Console.WriteLine("[CC]Send connection request");
                                CCtoNCCSingallingMessage packetToCC = new CCtoNCCSingallingMessage();
                                packetToCC.State = CCtoNCCSingallingMessage.NCC_SET_CONNECTION;
                                packetToCC.NodeFrom = packet.originIdentifier;
                                packetToCC.NodeTo = packet.destinationIdentifier;
                                handlerNCC.rejectedDestinations.Add(packet.RequestID, packet.destinationIdentifier);
                                packetToCC.Rate = packet.speed;
                                packetToCC.RequestID = packet.RequestID;
                                packetToCC.Vc11 = packet.Vc11;
                                packetToCC.Vc12 = packet.Vc12;
                                packetToCC.Vc13 = packet.Vc13;
                                ControlConnectionService CCService = this.handlerNCC.getCCService();
                                CCService.sendCCRequest(packetToCC);
                            }
                            else if (packet.state == ControlPacket.REJECT)
                            {
                                
                                Console.WriteLine("[NCC] Receive Call Release from previous NCC");
                                CCtoNCCSingallingMessage packetToCC = new CCtoNCCSingallingMessage();
                                packetToCC.State = CCtoNCCSingallingMessage.NCC_RELEASE_WITH_ID;
                                packetToCC.RequestID = packet.RequestID;
                                ControlConnectionService CCService = handlerNCC.getCCService();
                                CCService.sendCCRequest(packetToCC);

                            }

                        }
                        else if (packet.virtualInterface == ControlInterface.CALL_REQUEST_ACCEPT)
                        {
                            Console.WriteLine("[NCC] Recived CALL_REQUEST_ACCEPT");
                            // ZAKLADAMY TU ZE KAZDE NCC MA HANDLER NA INNE, INACZEJ SPRAWDZ DOMENE CZY TWOJA, NIE TO ODESLIJ DALEJ
                            if (packet.state == ControlPacket.ACCEPT)
                            {
                                Console.WriteLine("[Other NCC] CALL ACCEPTED");

                                CCtoNCCSingallingMessage packetToCC = new CCtoNCCSingallingMessage();
                                packetToCC.State = CCtoNCCSingallingMessage.NCC_SET_CONNECTION;
                                packetToCC.NodeFrom = handlerNCC.getCNAddressesForInterdomainCalls(packet.RequestID);
                                packetToCC.NodeTo = packet.destinationIdentifier;
                                Console.WriteLine("[CC]Send connection request from: " + packetToCC.NodeFrom + " to: " + packetToCC.NodeTo);
                                packetToCC.Rate = packet.speed;
                                packetToCC.RequestID = packet.RequestID;
                                packetToCC.Vc11 = packet.Vc11;
                                packetToCC.Vc12 = packet.Vc12;
                                packetToCC.Vc13 = packet.Vc13;
                                ControlConnectionService CCService = this.handlerNCC.getCCService();
                                CCService.sendCCRequest(packetToCC);
                            }
                            else
                            {
                                counter--;
                                if (counter < 0)
                                    break;
                                Console.WriteLine("[Other NCC] CALL REJECTED");
                                // handlerNCC.showInterdomainAttemptsForRequestID(packet.RequestID);
                               // Console.WriteLine("Szukam borderow dla ip: " + packet.destinationIdentifier);
                                String anotherBorderGWAddress = handlerNCC.getAnotherBorderGatewayAddress(packet.RequestID, packet.originIdentifier);
                                //Console.WriteLine("Znaleziony border GW: " + anotherBorderGWAddress);
                                if (anotherBorderGWAddress == null)
                                {
                                    ControlConnectionService cpccCallService = handlerNCC.getService(packet.RequestID);
                                    ControlPacket packetToCPCC = new ControlPacket(ControlInterface.CALL_ACCEPT, ControlPacket.REJECT, packet.speed, packet.originIdentifier, handlerNCC.getCNAddressesForInterdomainCalls(packet.RequestID), packet.RequestID);
                                    cpccCallService.send(packetToCPCC);
                                    Console.WriteLine("[NCC] ALL INTERDOMAINS CONNECTIONS BUSY, SEND REJECT TO CPCC");
                                    handlerNCC.clearCNAddressesForInterdomainCalls(packet.RequestID);
                                    handlerNCC.clearInterdomainCallAttempt(packet.RequestID);
                                    //NIE UDALO SIE U NAS, WYSLAC DO TAMTEGO NCC NIECH ROZLACZY JEDNAK
                                    //W DOMAIN Z TMATEGO NCC JEGO DOMAIN, ZEBY ODESLAC MU NIECH ROZLACZY
                                    // ControlConnectionService nccCallService = handlerNCC.getService(packet.domain);
                                    // ControlPacket packetToNCC = new ControlPacket(ControlInterface.CALL_INDICATION, ControlPacket.REJECT, packet.speed, "BORDER_GATEWAY", packet.destinationIdentifier, packet.RequestID);
                                    //nccCallService.send(packetToNCC);
                                }
                                else
                                {
                                    //Console.WriteLine("Znalazłem nowy border wyjsciowy: " + anotherBorderGWAddress);
                                    handlerNCC.addIntrerdomainCallsAttempts(packet.RequestID, anotherBorderGWAddress);
                                    Address address = new Address(packet.originIdentifier);
                                    ControlConnectionService serviceToNCC = handlerNCC.getService(address.domain);
                                    ControlPacket packetToNCC = new ControlPacket(ControlInterface.CALL_INDICATION, ControlPacket.IN_PROGRESS, packet.speed, packet.originIdentifier, anotherBorderGWAddress, packet.RequestID);
                                    packetToNCC.domain = handlerNCC.domainNumber;
                                    packetToNCC.Vc11 = 1;
                                    packetToNCC.Vc12 = 1;
                                    packetToNCC.Vc13 = 1;
                                    packetToNCC.speed = packet.speed;
                                    Console.WriteLine("[NCC]Send call request to next NCC with another border GW, destination: " + packetToNCC.destinationIdentifier + " borderGW: " + anotherBorderGWAddress);
                                    serviceToNCC.send(packetToNCC);

                                }

                            }
                        }

                    }
                    else if (received_object.Type == typeof(CCtoNCCSingallingMessage))
                    {

                        CCtoNCCSingallingMessage packet = received_object.Value.ToObject<CCtoNCCSingallingMessage>();
                        if (packet.State == CCtoNCCSingallingMessage.INIT_FROM_CC)
                        {
                            Console.WriteLine("[CC]Connection established");
                            handlerNCC.setCCService(this);
                        }
                        else if (packet.State == CCtoNCCSingallingMessage.CC_CONFIRM)

                       {


                            Console.WriteLine("[CC]Receive connection request confirm");
                            
                            if (handlerNCC.rejectedDestinations.ContainsKey(packet.RequestID))
                                handlerNCC.rejectedDestinations.Remove(packet.RequestID);
                            if (handlerNCC.checkIfInterdomainRequest(packet.RequestID))
                            {
                                ControlConnectionService NCCService = handlerNCC.getService(handlerNCC.getDomainService(packet.RequestID));
                                //Console.WriteLine("[CC]Border gateway to previous ncc: " + packet.NodeTo);
                                //Nodeto GW, NodeFrom CN in other domain address
                                ControlPacket packetToNCC = new ControlPacket(ControlInterface.CALL_REQUEST_ACCEPT, ControlPacket.ACCEPT, packet.Rate, packet.NodeTo, packet.NodeFrom, packet.RequestID);
                                packetToNCC.domain = handlerNCC.domainNumber;
                                packetToNCC.Vc11 = packet.Vc11;
                                packetToNCC.Vc12 = packet.Vc12;
                                packetToNCC.Vc13 = packet.Vc13;
                                NCCService.send(packetToNCC);
                                handlerNCC.clearCNAddressesForInterdomainCalls(packet.RequestID);
                            }
                            else
                            {
                                ControlConnectionService cpccCallService = handlerNCC.getService(packet.RequestID);
                                ControlPacket packetToCPCC = new ControlPacket(ControlInterface.CALL_ACCEPT, ControlPacket.ACCEPT, packet.Rate, packet.NodeTo, packet.NodeTo, packet.RequestID);
                                List<int> slots = new List<int>();
                                if (packet.Vc11 != 0)
                                {

                                    packetToCPCC.Vc11 = 1;
                                    slots.Add(11);
                                }
                                if (packet.Vc12 != 0)
                                {
                                    packetToCPCC.Vc12 = 1;
                                    slots.Add(12);
                                }
                                if (packet.Vc13 != 0)
                                {
                                    packetToCPCC.Vc13 = 1;
                                    slots.Add(13);
                                }
                                cpccCallService.send(packetToCPCC);
                                handlerNCC.management.send(packet.RequestID, packet.NodeTo);
                                Console.WriteLine("[NCC] Send Call Accept to CPCC");
                                //if (handlerNCC.checkIfInterdomainRequest(packet.RequestID))
                                //  handlerNCC.clearCNAddressesForInterdomainCalls(packet.RequestID);
                            }


                        }
                        else if (packet.State == CCtoNCCSingallingMessage.CC_REJECT)
                        {
                            Console.WriteLine("[CC]Receive connection reject");
                            if (handlerNCC.checkIfInterdomainRequest(packet.RequestID))
                            {
                                ControlConnectionService NCCService = handlerNCC.getService(handlerNCC.getDomainService(packet.RequestID));
                                //Nodeto GW, NodeFrom CN in other domain address
                               // Console.WriteLine("[CC]Destination node to previous ncc: " + handlerNCC.rejectedDestinations[packet.RequestID]);
                                ControlPacket packetToNCC = new ControlPacket(ControlInterface.CALL_REQUEST_ACCEPT, ControlPacket.REJECT, packet.Rate, packet.NodeTo, handlerNCC.rejectedDestinations[packet.RequestID], packet.RequestID);
                                packetToNCC.domain = handlerNCC.domainNumber;
                                NCCService.send(packetToNCC);
                                handlerNCC.rejectedDestinations.Remove(packet.RequestID);
                            }
                            else
                            {
                                ControlConnectionService cpccCallService = handlerNCC.getService(packet.RequestID);
                                ControlPacket packetToCPCC = new ControlPacket(ControlInterface.CALL_ACCEPT, ControlPacket.REJECT, packet.Rate, packet.NodeTo, packet.NodeTo, packet.RequestID);
                                Console.WriteLine("[NCC] Send Call Request Reject to CPCC");
                                cpccCallService.send(packetToCPCC);
                            }

                        }
                        else if (packet.State == CCtoNCCSingallingMessage.BORDER_NODE)
                        {
                            Console.WriteLine("[CC]Get border node address: " + packet.BorderNode + " to domain: " + packet.BorderDomain);
                            handlerNCC.addBorderGateway(packet.BorderDomain, packet.BorderNode);
                        }

                    }
                    else
                    {
                        Console.WriteLine("Wrong control packet format");
                    }

                }
                catch (IOException e)
                {
                    Console.WriteLine("Connection closed");
                    break;
                }
            }
        }

        public void send(ControlPacket packet)
        {
            //ControlPacket packet = new ControlPacket(ControlInterface.CALL_REQUEST, 0, resourceIdentifier);
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);

        }

        public void sendCCRequest(CCtoNCCSingallingMessage packet)
        {
            string data = JMessage.Serialize(JMessage.FromValue(packet));
            writer.Write(data);
        }


    }

}

