using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using PhotonPackageParser;


namespace AlbionProxy
{
    class UDPProxy : PhotonParser
    {
        string _bindAddress;
        int _srcPort;
        string _dstAddress;
        int _dstPort;

        public IPEndPoint m_listenEp = null;
        public EndPoint m_connectedClientEp = null;
        public IPEndPoint m_sendEp = null;
        public Socket m_UdpListenSocket = null;
        public Socket m_UdpSendSocket = null;

        bool running = false;

        byte[] data;


        public UDPProxy(string bindAddress, int srcPort, string dstAddress, int dstPort, int dataSize = 4096)
        {
            this._bindAddress = bindAddress;
            this._srcPort = srcPort;
            this._dstAddress = dstAddress;
            this._dstPort = dstPort;
            this.data = new byte[dataSize];
            

            Start();
            running = true;
        }

        void Start()
        {
            if (running)
                return;

            // Creates Listener UDP Server
            m_listenEp = new IPEndPoint(IPAddress.Parse(_bindAddress), _srcPort);
            m_UdpListenSocket = new Socket(m_listenEp.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            m_UdpListenSocket.Bind(m_listenEp);

            //Connect to zone IP EndPoint

            m_sendEp = new IPEndPoint(IPAddress.Parse(_dstAddress), _dstPort);
            m_connectedClientEp = new IPEndPoint(IPAddress.Any, _srcPort);
        }

        public void Update()
        {
            if (!running)
                return;

            if (m_UdpListenSocket.Available > 0)
            {
                int size = m_UdpListenSocket.ReceiveFrom(data, ref m_connectedClientEp); //client to listener

                try
                {
                    ProcessPhotonData(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception");
                }

                if (m_UdpSendSocket == null)
                {
                    // Connect to UDP Game Server.
                    m_UdpSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                }

                m_UdpSendSocket.SendTo(data, size, SocketFlags.None, m_sendEp); //listener to server.

            }

            if (m_UdpSendSocket != null && m_UdpSendSocket.Available > 0)
            {
                int size = m_UdpSendSocket.Receive(data); //server to client.

                try
                {
                    ProcessPhotonData(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception");
                }

                m_UdpListenSocket.SendTo(data, size, SocketFlags.None, m_connectedClientEp); //listener

            }
        }

        protected override void OnRequest(byte operationCode, Dictionary<byte, object> parameters)
        {
            int iCode = 0;
            if (int.TryParse(parameters[253].ToString(), out iCode))
            {
                OperationCodes opCode = (OperationCodes)iCode;
                string loggerName = "Request." + opCode.ToString();
                Console.WriteLine(loggerName);
            }
        }

        protected override void OnResponse(byte operationCode, short returnCode, string debugMessage, Dictionary<byte, object> parameters)
        {
            int iCode = 0;
            if (int.TryParse(parameters[253].ToString(), out iCode))
            {
                OperationCodes opCode = (OperationCodes)iCode;
                string loggerName = "Response." + opCode.ToString();
                Console.WriteLine(loggerName);
            }
        }

        protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
        {
            if (code == 2)
            {
                return;
            }
            object val;
            parameters.TryGetValue(252, out val);
            if (val == null)
            {
                return;
            }
            int iCode = 0;
            if (!int.TryParse(val.ToString(), out iCode))
            {
                return;
            }

            EventCodes eventCode = (EventCodes)iCode;

            Console.WriteLine("Event." + eventCode.ToString());
        }

        protected override byte[] OnSpecial(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
