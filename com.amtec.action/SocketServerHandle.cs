using com.amtec.configurations;
using com.amtec.forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace com.amtec.action
{
    class SocketServerHandle
    {
       // private IQCForm mv;
        //public SocketServerHandle(IQCForm mv1)
        //{
        //    mv = mv1;
        //    //dicIPToStation = "";//mv.StationIPMapping;
        //    process = new Thread(new ThreadStart(ProcessSocketCommand));
        //    process.Start();
        //}

        public IPEndPoint tcplisener;//将网络端点表示为IP地址和端口号
        public bool listen_flag = false;
        public Socket read;
        public Thread accept;//创建并控制线程
        public Thread monitor;//创建并控制线程
        public Thread process;//创建并控制线程
        public ManualResetEvent AcceptDone = new ManualResetEvent(false);
        ConcurrentQueue<SocketEntity> SEQueue = new ConcurrentQueue<SocketEntity>();

        /// <summary>
        /// Open port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenPort(string ipAdress, string portName)
        {
            string ipaddress = ipAdress;
            string port = portName;
            IPAddress ip = IPAddress.Parse(ipaddress.Trim());
            tcplisener = new IPEndPoint(ip, Convert.ToInt32(port.Trim()));
            read = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                read.Bind(tcplisener);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + "," + ex.StackTrace);
                //mv.SetConsoleText("Socket server run error");
                return;
            }
            read.Listen(500); //开始监听            
            //mv.SetConsoleText("Server run success,  Wait client connection");
            accept = new Thread(new ThreadStart(Listen));
            accept.Start();
            monitor = new Thread(new ThreadStart(SendHeartPackage));
            //if (mv.config.SendHeartPackage == "Y")
            //{
            //    monitor.Start();
            //}
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Listen()
        {
            Thread.CurrentThread.IsBackground = true; //后台线程
            try
            {
                while (true)
                {
                    AcceptDone.Reset();
                    read.BeginAccept(new AsyncCallback(AcceptCallback), read);  //异步调用                    
                    AcceptDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + ";" + ex.StackTrace);
               // mv.errorHandler(3, "ReadCallback error", "Error");
            }
        }

        public void SendHeartPackage()
        {
            Thread.CurrentThread.IsBackground = true; //后台线程
            try
            {
                while (true)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        Socket socket = clientList[i].workSocket;
                        DateTime dt = clientList[i].dTime;
                        IPEndPoint remotepoint = (IPEndPoint)socket.RemoteEndPoint;
                        try
                        {
                            //if (DateTime.Now.Subtract(dt).TotalSeconds >= Convert.ToInt32(mv.config.SendHeartSpan))
                            ////Send(socket, "12");
                            //{
                            //    LogHelper.Info("IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " disconnect " + DateTime.Now.Subtract(dt).TotalSeconds + "s");
                            //    clientList.Remove(clientList[i]);
                            //    //mv.SetCurrentConnectText("IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " stop connect");
                            //    socket.Shutdown(SocketShutdown.Both);
                            //    socket.Disconnect(true);
                            //    socket.Close();
                            //    LogHelper.Info("Remove IP-" + remotepoint.Address + " Port-" + remotepoint.Port);
                            //}
                        }
                        catch (Exception ex)
                        {
                            clientList.Remove(clientList[i]);
                            //mv.SetCurrentConnectText("IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " stop connect");
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Disconnect(true);
                            socket.Close();
                            LogHelper.Info("Remove IP-" + remotepoint.Address + " Port-" + remotepoint.Port);
                            LogHelper.Error(ex);
                        }
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + ";" + ex.StackTrace);
               // mv.errorHandler(3, "Send heart package error", "Error");
            }
        }

        List<StateObject> clientList = new List<StateObject>();
        Dictionary<string, string> dicIPToStation = new Dictionary<string, string>();
        public void AcceptCallback(IAsyncResult ar) //accpet的回调处理函数
        {
            try
            {
                AcceptDone.Set();
                Socket temp_socket = (Socket)ar.AsyncState;
                Socket client = temp_socket.EndAccept(ar); //获取远程的客户端
                Control.CheckForIllegalCrossThreadCalls = false;
                IPEndPoint remotepoint = (IPEndPoint)client.RemoteEndPoint;//获取远程的端口
                string remoteaddr = remotepoint.Address.ToString();        //获取远程端口的ip地址   
                //mv.SetConsoleText("IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " has connection.");
                StateObject state = new StateObject();
                state.workSocket = client;
                if (!clientList.Contains(state))
                    clientList.Add(state);
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + ";" + ex.StackTrace);
                //mv.SetConsoleText(ex.Message + ";" + ex.StackTrace);
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            if (!handler.Connected)
                return;
            state.dTime = DateTime.Now;
            IPEndPoint remotepoint = null;
            try
            {
                remotepoint = (IPEndPoint)handler.RemoteEndPoint;
            }
            catch (Exception ex)
            {
                clientList.Remove(state);
                LogHelper.Error(ex);
                return;
            }
            // Read data from the client socket. 
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.StackTrace);
                return;
            }
            if (bytesRead > 0)
            {
                string contentstr = ""; //接收到的数据
                contentstr = Encoding.GetEncoding("UTF-8").GetString(state.buffer, 0, bytesRead);
                LogHelper.Info("message:" + contentstr);

                string strSN = contentstr.Replace("#", "");
                string ipAndPort = remotepoint.Address.ToString(); //+ ";" + remotepoint.Port.ToString();             
                SocketEntity entity = new SocketEntity();
                entity.dCommand = strSN;
                entity.dState = state;
                entity.dIPAddress = ipAndPort;
                entity.dSocket = handler;
                SEQueue.Enqueue(entity);
                //mv.SetConsoleText("Receive message from (client)IP:" + remotepoint.Address + " Port:" + remotepoint.Port + " -----" + strSN);

                try
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message + ";" + ex.StackTrace);
                }
            }
            else
            {
                //mv.SetConsoleText("IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " stop connect");
                try
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Disconnect(true);
                    handler.Close();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }

        private void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.GetEncoding("UTF-8").GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void ProcessSocketCommand()
        {
            Thread.CurrentThread.IsBackground = true; //后台线程
            while (true)
            {
                try
                {
                    if (!SEQueue.IsEmpty)
                    {
                        SocketEntity seEntity = null;
                        bool isHas = SEQueue.TryDequeue(out seEntity);
                        if (isHas)
                        {
                            string returnValue = "";
                            string strSN = seEntity.dCommand;
                            string ipAndPort = seEntity.dIPAddress;
                            Socket handler = seEntity.dSocket;
                            StateObject state = seEntity.dState;
                            string[] values = strSN.Split(new char[] { ';' });
                            IPEndPoint remotepoint = null;
                            try
                            {
                                remotepoint = (IPEndPoint)handler.RemoteEndPoint;
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            if (strSN.StartsWith("01;"))
                            {
                                string workorder = values[1];
                                //get sn
                                string serialNumber = "";// mv.GenerateSerialNumber(workorder);
                                returnValue = "#01;" + serialNumber;
                            }
                            else if (strSN.StartsWith("03;"))//stock in
                            {
                                string mtoNo = values[1];
                                string plantNo = values[2];
                                //returnValue = "#03;" + mv.ProcessMTOData(mtoNo, plantNo, "STOCK_IN");
                            }
                            else if (strSN.StartsWith("04;"))
                            {
                                string mtoNo = values[1];
                                string partNo = values[2];
                                string status = values[3];
                                //string strValue = mv.ProcessStockIN04(mtoNo, partNo, status);
                                //returnValue = "#04;" + strValue;
                            }
                            else if (strSN.StartsWith("05;"))
                            {
                                string mtoNo = values[1];
                                string partNo = values[2];
                                string location = values[3];
                                string status = values[4];
                                decimal qtyStocked = Convert.ToDecimal(values[5]);
                                decimal qtyRest = Convert.ToDecimal(values[6]);
                                //string strValue = mv.ProcessStockIN05(mtoNo, partNo, location, status, qtyStocked, qtyRest);
                                //returnValue = "#05;" + strValue;
                            }
                            else if (strSN.StartsWith("06;"))//stock out
                            {
                                string mtoNo = values[1];
                                string plantNo = values[2];
                                //returnValue = "#06;" + mv.ProcessMTOData(mtoNo, plantNo, "STOCK_OUT");
                            }
                            else if (strSN.StartsWith("07;"))//stock out---Update “CUST. MATERIAL_TRANSFER_ORDER”,  For each part number in MTO Material List,
                            {
                                string mtoNo = values[1];
                                string plantNo = values[2];
                                string partNumber = values[3];
                                decimal dQty = Convert.ToDecimal(values[4]);
                                //returnValue = "#07;" + mv.ProcessStockOut07(mtoNo, plantNo, partNumber, dQty);
                            }
                            else if (strSN.StartsWith("00"))
                            {
                                try
                                {
                                    returnValue = "#00;OK#";
                                    byte[] byteData1 = Encoding.GetEncoding("UTF-8").GetBytes(returnValue);//回发信息
                                    handler.BeginSend(byteData1, 0, byteData1.Length, 0, new AsyncCallback(SendCallback), handler);
                                    //mv.SetConsoleText("Send message to (Client)IP:" + remotepoint.Address + " Port:" + remotepoint.Port + " -----" + returnValue + System.Environment.NewLine);
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Error(ex);
                                }
                                //mv.SetCurrentConnectText("IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " stop connect");
                                try
                                {
                                    handler.Shutdown(SocketShutdown.Both);
                                    handler.Disconnect(true);
                                    handler.Close();
                                    LogHelper.Info("Remove IP-" + remotepoint.Address + " Port-" + remotepoint.Port);
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Error(ex);
                                }
                            }
                            else
                            {
                                returnValue = "-----Formate error";// +valueTSA;
                            }

                            if (!strSN.StartsWith("00"))
                            {
                                returnValue = returnValue + "#";//System.Environment.NewLine;
                                byte[] byteData = Encoding.GetEncoding("UTF-8").GetBytes(returnValue);//回发信息
                                try
                                {
                                    handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
                                    //mv.SetConsoleText("Send message to (Client)IP:" + remotepoint.Address + " Port:" + remotepoint.Port + " -----" + returnValue + System.Environment.NewLine);
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Error(ex);
                                }
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }

                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }

        public class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];

            public DateTime dTime = DateTime.Now;
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        public class SocketEntity
        {
            public Socket dSocket = null;
            public string dCommand = null;
            public string dIPAddress = null;
            public StateObject dState = null;
        }
    }
}
