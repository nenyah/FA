using FA_COATING.com.amtec.forms;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace com.amtec.action
{
    public class SocketClientHandler
    {
        public TcpClient tcpc; //对服务器端建立TCP连接 
        public Socket tcpsend; //发送创建套接字 
        public bool connect_flag = true;
        public byte[] receive_buff = new byte[4096];
        public ManualResetEvent connectDone = new ManualResetEvent(false); //连接的信号 
        //public ManualResetEvent readDone = new ManualResetEvent(false); //读信号 
        public ManualResetEvent sendDone = new ManualResetEvent(false); //发送结束
        //private IQCForm view;
        private Form1 view;


        public SocketClientHandler(Form1 view)
        {
            this.view = view;
        }

        public bool connect(string address, string port)
        {
            try
            {
                tcpsend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//初始化套接字
                IPEndPoint remotepoint = new IPEndPoint(IPAddress.Parse(address), Convert.ToInt32(port));//根据ip地址和端口号创建远程终结点
                EndPoint end = (EndPoint)remotepoint;
                //view.errorHandler(0, "Start Server connection. IP:" + remotepoint.Address + " Port:" + remotepoint.Port, "");
                LogHelper.Info("Start Server connection. IP:" + remotepoint.Address + " Port:" + remotepoint.Port);
                tcpsend.BeginConnect(end, new AsyncCallback(ConnectedCallback), tcpsend); //调用回调函数
                //connectDone.WaitOne();
               return true;
            }
            catch
            {
                view.errorHandler(1,"server connect fail");
                return false;
            }
        }

        private void ConnectedCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            try
            {
                client.EndConnect(ar);
                view.errorHandler(0, "server connect success");
                LogHelper.Info("连接服务器成功");
                
            }
            catch (Exception ex)
            {
                LogHelper.Error("Connection Error. IP Cannot be reached");
                LogHelper.Error(ex.Message + "," + ex.StackTrace);
                view.errorHandler(1, "Connection Error. IP Cannot be reached.");
            }
            connect_flag = true;
            connectDone.Set();
        }

        public void send(string data)
        {
            int length = data.Length;
            Byte[] Bysend = new byte[length];
            Bysend = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data); //将字符串指定到指定Byte数组
            tcpsend.BeginSend(Bysend, 0, Bysend.Length, 0, new AsyncCallback(SendCallback), tcpsend); //异步发送数据
            //SetConsoleText("Send Serial Number: " + data);
            //view.errorHandler(0, "Connected to Server.", "");
            sendDone.WaitOne();
        }
        private void SendCallback(IAsyncResult ar) //发送的回调函数
        {
            Socket client = (Socket)ar.AsyncState;
            int bytesSend = client.EndSend(ar); //完成发送
            sendDone.Set();
        }

        public string SendData(string message)
        {
            try
            {
                
                send(message);
               // view.errorHandler(0, "send success", "");
                //readDone.Reset();
                byte[] back = new byte[1024 * 1024];
                int count = tcpsend.Receive((back));
                //string str = Encoding.GetEncoding("UTF-8").GetString(back, 0, count);
                string str = Encoding.GetEncoding("UTF-8").GetString(back, 0, count);
                //view.errorHandler(0, str, "");
                return str;
            }
            catch (Exception ex)
            {
                //view.errorHandler(2, "send error", "");
                LogHelper.Error(ex.Message + "," + ex.StackTrace);
                return null;
            }
        }

        public void CloseSocket()
        {
            try
            {
                tcpsend.Shutdown(SocketShutdown.Both);
                tcpsend.Close();
                view.errorHandler(0, "Stop connect.");
                LogHelper.Info("Stop connect.");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + ";" + ex.StackTrace);
            }

        }
    }
}
