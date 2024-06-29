using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace KF
{
    public class UDPSocket
    {
        //===========================================================
        public const string IPBroadcast = "255.255.255.255";
        /// <summary>
        /// 优先启用 IPv6
        /// 仅当系统支持且dns有解析出v6
        /// </summary>
        public static bool UseIPv6 = false;

        /// <summary>
        /// UDP下行包IP检测
        /// </summary>
        public static bool CheckUDPRemoteIP = true;

        public static IPEndPoint IPEP_Any =  new IPEndPoint(IPAddress.Any, 0);
        public static IPEndPoint IPEP_IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);

        #region 工具函数:IsBroadcast,IsFatalException,GetIPEndPointAny

        public static IPEndPoint GetIPEndPointAny(AddressFamily family, int port)
        {
            if (family == AddressFamily.InterNetwork)
            {
                if (port == 0)
                {
                    return IPEP_Any;
                }

                return new IPEndPoint(IPAddress.Any, port);
            }
            else if (family == AddressFamily.InterNetworkV6)
            {
                if (port == 0)
                {
                    return IPEP_IPv6Any;
                }

                return new IPEndPoint(IPAddress.IPv6Any, port);
            }

            return null;
        }

        #endregion

        //===========================================================

        public Socket SystemSocket => m_SystemSocket;
        private bool m_IsActive;
        private Socket m_SystemSocket;
        private AddressFamily m_AddrFamily;
        private IPEndPoint m_remoteEndPoint;
        private bool m_IsBroadcast;

        private bool m_enableBlockRecv = false; //是否允许阻塞
        private string m_ip = null;
        private int m_port = 0;

        public string ip => m_ip;

        public int port => m_port;

        //===========================================================

        #region 构造与析构

        public UDPSocket(AddressFamily family = AddressFamily.InterNetwork, bool enableBlockRecv = false)
        {
            m_enableBlockRecv = enableBlockRecv;
            m_AddrFamily = family;
            m_SystemSocket = new Socket(m_AddrFamily, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (m_SystemSocket != null)
            {
                try
                {
                    m_SystemSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception  e)
                {
                    //KHDebugTool.KHDebug.LogWarning("[PVP][UpdSocket.Close]" + e.Message);
                }

                m_SystemSocket.Close();
                m_SystemSocket = null;
                m_AddrFamily = AddressFamily.Unknown;
            }

            m_IsActive = false;
            GC.SuppressFinalize(this);
        }

        #endregion

        //------------------------------------------------------------

        #region 连接函数，主要用于Client

        public IPEndPoint Connect(string ipv4, string ipv6, int port)
        {
            if (UseIPv6 && Socket.OSSupportsIPv6 && !string.IsNullOrEmpty(ipv6))
            {
                IPAddress addrv6 = null;
                if (IPAddress.TryParse(ipv6, out addrv6))
                {
                    if (m_SystemSocket != null)
                    {
                        if (m_AddrFamily != AddressFamily.InterNetworkV6)
                        {
                            m_SystemSocket.Close();
                            m_SystemSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram,
                                System.Net.Sockets.ProtocolType.Udp);
                        }
                    }
                    else
                    {
                        m_SystemSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram,
                            System.Net.Sockets.ProtocolType.Udp);
                    }

                    m_ip = ipv6;
                    m_port = port;
                    m_IsActive = true;
                    m_AddrFamily = AddressFamily.InterNetworkV6;
                    m_remoteEndPoint = new IPEndPoint(addrv6, port);
                    return m_remoteEndPoint;
                }
            }
            else if (!string.IsNullOrEmpty(ipv4) && Socket.OSSupportsIPv4)
            {
                IPAddress addrv4 = null;
                if (IPAddress.TryParse(ipv4, out addrv4))
                {
                    if (m_SystemSocket != null)
                    {
                        if (m_AddrFamily != AddressFamily.InterNetwork)
                        {
                            m_SystemSocket.Close();
                            m_SystemSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                                System.Net.Sockets.ProtocolType.Udp);
                        }
                    }
                    else
                    {
                        m_SystemSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                            System.Net.Sockets.ProtocolType.Udp);
                    }

                    //IP地址为广播IP，并且当前该类并没有标志为广播
                    if (!m_IsBroadcast && ipv4 == IPBroadcast)
                    {
                        m_IsBroadcast = true;
                        m_SystemSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    }

                    m_ip = ipv4;
                    m_port = port;
                    m_IsActive = true;
                    m_AddrFamily = AddressFamily.InterNetwork;
                    m_remoteEndPoint = new IPEndPoint(addrv4, port);
                    return m_remoteEndPoint;
                }
            }

            return null;
        }

        #endregion

        //------------------------------------------------------------

        #region 绑定端口函数，主要用于Server

        public int Bind(int port)
        {
            if (m_SystemSocket == null)
            {
                return 0;
            }

            var ipep = GetIPEndPointAny(m_AddrFamily, port);
            m_SystemSocket.Bind(ipep);
            m_IsActive = true;
            ipep = m_SystemSocket.LocalEndPoint as IPEndPoint;

            //KHDebugTool.KHDebug.Log($"绑定本机端口:{ipep.Port}");
            return ipep.Port;
        }

        #endregion

        //------------------------------------------------------------

        #region 用于Server的ReceiveFrom和SendTo函数

        public int ReceiveFrom(byte[] buffer, int maxsize)
        {
            if (m_SystemSocket == null)
            {
                throw new ArgumentNullException("m_SystemSocket");
            }

            if (buffer == null || maxsize <= 0)
            {
                throw new ArgumentNullException("buffer");
            }

            if (!m_enableBlockRecv)
            {
                if (m_SystemSocket.Available <= 0)
                {
                    //KHDebugTool.KHDebug.Log("[PVP][UdpSocket.ReceiveFrom] m_SystemSocket.Available <= 0");
                    return 0;
                }
            }

            var cnt = 0;

            if (m_AddrFamily == AddressFamily.InterNetwork)
            {
                //ipv4
                EndPoint ip = GetIPEndPointAny(m_AddrFamily, 0);
                cnt = m_SystemSocket.ReceiveFrom(buffer, maxsize, SocketFlags.None, ref ip);

                if (CheckUDPRemoteIP && cnt > 0 && m_remoteEndPoint != null && !m_remoteEndPoint.Equals(ip))
                {
                    //KHDebugTool.KHDebug.Log($"[PVP][UdpSocket.ReceiveFrom]过滤未授信数据包, ip:{ip}, size:{cnt}");
                    return 0;
                }
            }
            else
            {
                //ipv6
                EndPoint ip = m_remoteEndPoint;
                cnt = m_SystemSocket.ReceiveFrom(buffer, maxsize, SocketFlags.None, ref ip);
            }

            return cnt;
        }

        public int SendTo(byte[] buffer, int size)
        {
            if (m_SystemSocket == null)
            {
                throw new ArgumentNullException("m_SystemSocket");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (m_remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEP");
            }

            if (m_IsActive == false)
            {
                throw new InvalidOperationException("NotConnected!");
            }

            var cnt = 0;
            cnt = m_SystemSocket.SendTo(buffer, 0, size, SocketFlags.None, m_remoteEndPoint);
            return cnt;
        }

        #endregion
    }
}
