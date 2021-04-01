using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace AlbionProxy
{
    class Program
    {
        public static IPEndPoint m_listenEp = null;
        public static EndPoint m_connectedClientEp = null;
        public static IPEndPoint m_sendEp = null;
        public static Socket m_UdpListenSocket = null;
        public static Socket m_UdpSendSocket = null;


        static void Main(string[] args)
        {

            UDPProxy chatProxy = new UDPProxy("0.0.0.0", 4535, "5.188.125.17", 4535);
            UDPProxy loginProxy = new UDPProxy("0.0.0.0", 5055, "5.188.125.14", 5055);
            UDPProxy gameProxy = new UDPProxy("0.0.0.0", 5056, "5.188.125.27", 5056);

            while (true)
            {
                chatProxy.Update();
                gameProxy.Update();
                loginProxy.Update();
            }
        }
    }
}