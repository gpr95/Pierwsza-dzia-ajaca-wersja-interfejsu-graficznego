﻿using ClientNode;
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

        public ControlConnectionService(TcpClient clientHandler, NetworkCallControl handlerNCC)
        {
            this.client = clientHandler;
            this.handlerNCC = handlerNCC;
            init(client);
            ip = "127.0.0.1";
            
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
                try {
                    string received_data = reader.ReadString();
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type == typeof(ControlPacket))
                    {
                        ControlPacket packet = received_object.Value.ToObject<ControlPacket>();
                        if(packet.virtualInterface == ControlInterface.CALL_REQUEST)
                        {
                            handlerNCC.addService(packet.RequestID, this);
                            Console.WriteLine("[CPCC]Receive call request for "+packet.destinationIdentifier+" on " + ControlInterface.CALL_REQUEST_ACCEPT + " interface");
                            Console.WriteLine("[DIRECTORY]Send directory request");//sprawdzenie czy w naszej domenie 
                            if (handlerNCC.checkIfInDirectory(packet.destinationIdentifier))
                            {
                                Console.WriteLine("[DIRECTORY]Receive local name");
                                Console.WriteLine("[POLICY]Send policy out");
                                Console.WriteLine("[POLICY]Call accept");
                                Console.WriteLine("[CPCC]Send call indication");
                                Console.WriteLine("[CPCC]Call confirmed");
                                //wysylanie do innego cpcc
                                Console.WriteLine("[CC]Send connection request");
                                CCtoNCCSingallingMessage packetToCC = new CCtoNCCSingallingMessage();
                                packetToCC.State = CCtoNCCSingallingMessage.NCC_SET_CONNECTION;
                                packetToCC.NodeFrom = packet.originIdentifier;
                                packetToCC.NodeTo = packet.destinationIdentifier;
                                packetToCC.Rate = packet.speed;
                                packetToCC.RequestID = packet.RequestID;
                                ControlConnectionService CCService = this.handlerNCC.getCCService();
                                CCService.sendCCRequest(packetToCC);
                                
                                //connection = new TcpClient(ip, connectionControlPort);
                                // thread = new Thread(new ParameterizedThreadStart(connectionControlThread));
                                // thread.Start(packet.resourceIdentifier);
                            }
                            else
                            {
                                Console.WriteLine("[DIRECTORY]This client is not in my network");
                                Address address = new Address(packet.destinationIdentifier);
                                ControlConnectionService serviceToNCC = handlerNCC.getService(address.domain);
                                //DODAC DO CONTROL PACKET ID REQUEST PLUS DOMENY, I W CALYM PROJEKCIE TEZ, NAJWYZEJ W KLIENCIE SIE DA Z DUPY
                                ControlPacket packetToNCC = new ControlPacket(ControlInterface.CALL_INDICATION, ControlPacket.IN_PROGRESS, packet.speed, "BRODER_GATEWAY", packet.destinationIdentifier, handlerNCC.domainNumber);
                                //DOROBIC TU ELS IFA CO MA ZROBIC NA CALL INDICATION I CALL_REQUEST ACCEPT
                                //serviceToNCC.send()
                                Console.WriteLine("[NCC]Send call request to next NCC");
                            }
                            
                           
                            
                            Console.WriteLine("[CPCC] Call confirmed or not");


                        }else if(packet.virtualInterface == ControlInterface.NETWORK_CALL_COORDINATION_IN){
                            Console.WriteLine("[NCC] Receive NCC invitation from NCC in domain"+ packet.RequestID);
                            handlerNCC.addService(packet.RequestID, this);
                            ControlPacket packetToNCCResponse = new ControlPacket(ControlInterface.NETWORK_CALL_COORDINATION_OUT, ControlPacket.IN_PROGRESS, 0, "", "", handlerNCC.domainNumber);
                            send(packetToNCCResponse);
                            Console.WriteLine("[NCC] Send invitation response to NCC in domain" + packetToNCCResponse.RequestID);
                        }
                        else if(packet.virtualInterface == ControlInterface.NETWORK_CALL_COORDINATION_OUT) {
                            Console.WriteLine("[NCC] NCC handshake completed with NCC in doman" + packet.RequestID);
                            handlerNCC.addService(packet.RequestID, this);
                        }

                    }else if (received_object.Type == typeof(CCtoNCCSingallingMessage))
                    {
                        
                        CCtoNCCSingallingMessage packet = received_object.Value.ToObject<CCtoNCCSingallingMessage>();
                        if (packet.State == CCtoNCCSingallingMessage.INIT_FROM_CC)
                        {
                            Console.WriteLine("[CC]Connection established");
                            handlerNCC.setCCService(this);
                        }
                        else if(packet.State == CCtoNCCSingallingMessage.CC_CONFIRM)
                        {
                            Console.WriteLine("[CC]Receive connection confirm");
                            ControlConnectionService cpccCallService = handlerNCC.getService(packet.RequestID);
                            ControlPacket packetToCPCC = new ControlPacket(ControlInterface.CALL_ACCEPT,ControlPacket.ACCEPT,packet.Rate,packet.NodeTo,packet.NodeTo, packet.RequestID);
                            if(packet.Vc11 != 0)
                            {

                                packetToCPCC.Vc11 = 1;
                            }
                            if(packet.Vc12 != 0)
                            {
                                packetToCPCC.Vc12 = 1;
                            }
                            if(packet.Vc13 != 0)
                            {
                                packetToCPCC.Vc13 = 1;
                            }
                            cpccCallService.send(packetToCPCC);

                        }else if(packet.State == CCtoNCCSingallingMessage.CC_REJECT)
                        {
                            Console.WriteLine("[CC]Receive connection reject");
                            ControlConnectionService cpccCallService = handlerNCC.getService(packet.RequestID);
                            ControlPacket packetToCPCC = new ControlPacket(ControlInterface.CALL_ACCEPT, ControlPacket.REJECT,packet.Rate, packet.NodeTo, packet.NodeTo, packet.RequestID);
                            cpccCallService.send(packetToCPCC);
                        }

                    }else
                    {
                        Console.WriteLine("Wrong control packet format");
                    }

                }catch(IOException e)
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

