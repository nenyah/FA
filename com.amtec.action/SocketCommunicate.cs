using JYGCloud.JOBMonitor.Common;
using JYGCloud.JOBMonitor.ICommunicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JYGCloud.JOBMonitor.Communicate
{
    /// <summary>
    /// 单点采集器GPRS通讯对象
    /// </summary>
    public sealed class SCMGPRSCommunicate : IBaseCommunicate<string>
    {
        #region 变量

        /// <summary>
        /// 是否释放
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 是否打开服务
        /// </summary>
        private bool _isOpen;

        /// <summary>
        /// 无线服务IP
        /// </summary>
        private string _wsIP;

        /// <summary>
        /// 无线服务端口
        /// </summary>
        private int _wsPort;

        /// <summary>
        /// 无线服务监听队列的长度
        /// </summary>
        private int _wsListenCount;

        /// <summary>
        /// 无线服务工作模式
        /// </summary>
        private SocketType _wsSocketType;

        /// <summary>
        /// 无线服务类型
        /// </summary>
        private ProtocolType _wsProtocolType;

        /// <summary>
        /// 无线服务IP版本
        /// </summary>
        private AddressFamily _wsAddressFamily = AddressFamily.InterNetwork;

        /// <summary>
        /// 无线服务对象
        /// </summary>
        Socket _wsSocket = null;

        /// <summary>
        /// 网络嵌套监控进程
        /// </summary>
        Thread _wsSocketWatch = null;

        /// <summary>
        /// 是否监控
        /// </summary>
        bool _isWatch = false;

        #endregion

        #region 属性



        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="wsIP">无线服务IP</param>
        /// <param name="wsPort">无线服务端口</param>
        /// <param name="wsListenCount">无线服务监听队列的长度</param>
        /// <param name="wsAddressFamily">无线服务IP版本</param>
        /// <param name="wsSocketType">无线服务工作模式</param>
        /// <param name="wsProtocolType">无线服务类型</param>
        public SCMGPRSCommunicate(
            string wsIP, 
            int wsPort,
            int wsListenCount,
            AddressFamily wsAddressFamily = AddressFamily.InterNetwork,
            SocketType wsSocketType = SocketType.Stream,
            ProtocolType wsProtocolType = ProtocolType.Tcp)
        {
            // 变量初始化
            _wsIP = wsIP;
            _wsPort = wsPort;
            _wsListenCount = wsListenCount;
            _wsSocketType = wsSocketType;
            _wsProtocolType = wsProtocolType;

            // 初始化其为未打开
            _isOpen = false;
        }

        #endregion

        #region 接口方法

        /// <summary>
        /// 开启通讯通道
        /// </summary>
        public bool OpenCommunicationChannel()
        {
            try
            {
                // 如果对象已经被回收
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name, "对象已经被回收");
                }

                // 实例化无线服务对象
                _wsSocket = new Socket(_wsAddressFamily, _wsSocketType, _wsProtocolType);

                // 获取IP地址
                IPAddress _wsIPAddress = IPAddress.Parse(_wsIP);

                // 创建包含IP和端口号的网络节点对象  
                IPEndPoint _wsEndPoint = new IPEndPoint(_wsIPAddress, _wsPort);

                // 将负责监听的套接字绑定到唯一的IP和端口上
                _wsSocket.Bind(_wsEndPoint);

                // 设置监听队列的长度
                _wsSocket.Listen(_wsListenCount);

                // 设置监控
                _isWatch = true;

                // 开启监控进程
                _wsSocketWatch = new Thread(WatchSocketConnecting);
 
                // 置为后台线程
                _wsSocketWatch.IsBackground = true;

                // 监控线程启动
                _wsSocketWatch.Start();  
                
                // 返回成功
                return true;
            }
            catch
            {
                // 返回失败
                return false;
            }
        }

        /// <summary>
        /// 关闭通讯通道
        /// </summary>
        public bool CloseCommunicationChannel()
        {
            // 
            try
            {
                // 如果对象已经被回收
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name, "对象已经被回收");
                }

                // 如果对象被打开
                if (_wsSocket != null)
                {
                    // 关闭监听的全部线程
                    _isWatch = false;

                    // 关闭Socket对象
                    _wsSocket.Close();

                    // 设置为NULL
                    _wsSocket = null;
                }

                // 返回成功
                return true;
            }
            catch
            {
                // 返回失败
                return false;
            }
        }

        /// <summary>
        /// 重置通讯通道
        /// </summary>
        /// <param name="configEntity">重置通讯</param>
        public void ResetCommunicationChannel(Object configEntity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取通讯数据包（算法未启用）
        /// </summary>
        /// <param name="cmdString"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public CommunicateDataEntity<string> GetCommunicateDataPackage(string cmdString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取通讯数据包（算法未启用）
        /// </summary>
        /// <param name="cmdString"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public CommunicateDataEntity<string> GetCommunicateDataPackage(byte[] cmdByte)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        public void Dispose()
        {
            // 如果通讯串口处在打开状态，则进行关闭
            if (_isOpen.Equals(true))
            {
                // 停止监听
                _isWatch = false;

                // 关闭服务
                _wsSocket.Close();

                // 设置
                _wsSocket = null;

                // 设置释放标志为True
                this._disposed = true;
            }

            // 回收
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 函数

        /// <summary>
        /// 监控网络嵌套链接
        /// </summary>
        void WatchSocketConnecting()
        {
            // 持续不断的监听客户端的连接请求
            // 如果是否监控被关闭
            while (_isWatch)
            {
                try
                {
                    // 开始监听客户端连接请求，Accept方法会阻断当前的线程
                    // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的套接字
                    Socket socketClientConn = _wsSocket.Accept();

                    // 创建客户端连接线程
                    Thread clientThread = new Thread(SocketMessagePush);

                    // 设置客户端连接线程为后台
                    clientThread.IsBackground = true;

                    // 开启客户端连接线程
                    clientThread.Start(socketClientConn);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// 嵌套信息推送
        /// </summary>
        /// <param name="soketConnection">客户端链接</param>
        void SocketMessagePush(object socketClientConn)
        {
            // 转换对象
            Socket client = socketClientConn as Socket;

            // 创建单点无线通讯实体
            SCMSocketEntity scmSocket = new SCMSocketEntity(null, EnumDTUStatus.Online.ToString(), client);

            // 如果是否监控被关闭
            while (_isWatch)
            {
                // 设置数据缓冲区
                byte[] arrDataBuff = new byte[Const.C_WIRELESS_SCM_DATA_BUFFER_SIZE];

                // 设置变量记录输出数据长度 
                int dataLength = -1;

                try
                {
                    // 接收数据，并返回数据的长度
                    dataLength = client.Receive(arrDataBuff); 

                    // 如果接收到的数据为0，说明已经断线
                    if (dataLength.Equals(0))
                    {
                        throw new Exception("远程连接关闭");
                    }
                }
                catch
                {
                    // 设置离线
                    scmSocket.DTUStatus = EnumDTUStatus.Offline.ToString();

                    // 推送下线
                    PushSocketDataPacketEvent(null, scmSocket);

                    // 跳出循环
                    break;
                }

                // 将信息抛入委托
                PushSocketDataPacketEvent(arrDataBuff.ToHexString(dataLength), scmSocket);

                // 如果Socket状态被置为下线，则退出循环
                if (scmSocket.DTUStatus.Equals(EnumDTUStatus.Offline.ToString()))
                {
                    break;
                }
            }

            // 关闭客户端链接
            if (client != null)
            {
                // 如果客户端还处于链接状态
                if (client.Connected)
                {
                    client.Close();
                }

                // 释放资源
                client.Dispose();
            }
        }

        #endregion

        #region 委托

        /// <summary>
        /// 推送网络数据包
        /// </summary>
        /// <param name="dataPacket">网络数据包</param>
        public delegate void PushSocketDataPacketDelegate(string dataPacket, SCMSocketEntity scmSocket);

        #endregion

        #region 委托事件

        /// <summary>
        /// 推送网络数据包事件
        /// </summary>
        /// <param name="dataPacket">网络数据包</param>
        public event PushSocketDataPacketDelegate PushSocketDataPacketEvent;

        #endregion
    }
}