using com.amtec.action;
using com.amtec.configurations;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using Compal.MESComponent;
using FA_COATING.com.amtec.Bean;
using PartAssignment.com.amtec.SelectGW;
using PartAssignment.com.amtec.UpdateInsert;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace FA_COATING.com.amtec.forms
{
    public partial class Form1 : Form
    {
        private ApplicationConfiguration config;
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext = null;
        private SelectGW selectGw;
        private UpdateInsert updateInsert;
        private BeanInfo beanInfo = new BeanInfo();
        SocketClientHandler socketclient = null;
        SerialPort serialPort;
        ExecutionResult exeRes = new ExecutionResult();


        public Form1(string userName, DateTime dTime, IMSApiSessionContextStruct _sessionContext, ApplicationConfiguration _config)
        {
            InitializeComponent();
            sessionContext = _sessionContext;
            config = _config;
            serialPort = new SerialPort();
            selectGw = new SelectGW(sessionContext);
            updateInsert = new UpdateInsert(sessionContext);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 窗体标题显示版本信息
            this.Text = $@"三防漆 ( v{Assembly.GetExecutingAssembly().GetName().Version} ) by steven";
            if (config.LogInType == "COM")
            {
                serialPort.PortName = config.SerialPort;
                serialPort.Open();
                serialPort.BaudRate = int.Parse(config.BaudRate);
                serialPort.DataBits = int.Parse(config.DataBits);
                serialPort.StopBits = (StopBits)1;
                serialPort.Parity = (Parity)int.Parse(config.Parity);
                serialPort.NewLine = config.NewLineSymbol;
                serialPort.Handshake = Handshake.None;
                serialPort.ReadTimeout = 100;
                serialPort.WriteTimeout = -1;
                serialPort.ReceivedBytesThreshold = 1;
                serialPort.DataReceived += new SerialDataReceivedEventHandler(Serial_Received);
                //serialPort.Close();
            }
            // 在Form1.cs的Form1_Load事件中添加
            MessageForm.ShowMessage("程序启动成功", false);
        }

        private void Serial_Received(object sender, SerialDataReceivedEventArgs e)
        {
            Task.Run(() => CheckNumberState(sender));
        }
        private async Task CheckNumberState(object sender)
        {
            SerialPort sp = (SerialPort)sender;

            try
            {
                // 异步获取串口数据
                int count = sp.BytesToRead;
                byte[] buffer = new Byte[count];
                sp.Read(buffer, 0, count);
                if (count <= 0)
                    return;
                string s = Encoding.ASCII.GetString(buffer).Trim();
                if (s.Length <= 0)
                    return;
                LogHelper.Info($"Recieved Message: {s}");
                // 更新this.textBox1显示为 s
                this.Invoke(new MethodInvoker(delegate
                {
                    this.textBox1.Text = s;
                }));
                // 使用正则 ^([A-Z0-9]{22})(?:;([A-Z0-9]{22}))?$ 校验字符串s,并取出分组值
                Match match = Regex.Match(s, config.REGEX_PATTERN);
                // 没有匹配上，直接显示错误信息
                if (!match.Success)
                {
                    HandleError($"扫描信息格式错误,无法解析: {s}");
                    return;
                }
                List<string> indata = new List<string>();
                // 提取第一个字符串（必存在）
                indata.Add(match.Groups[1].Value);

                // 检查第二个字符串是否存在（分号后的内容）
                if (match.Groups[2].Success)
                {
                    indata.Add(match.Groups[2].Value);
                }


                // string[] indata = s.Trim().Split(new char[] { ';' }).Where(el => !string.IsNullOrEmpty(el)).ToArray();
                string errMsg = await CheckItacStateAsync(indata);
                if (errMsg.Length > 0)
                {
                    HandleError(errMsg);
                    return;
                }

                // 验证成功，根据config.CUSTOM_CODE 是否等于 "TP" 进行烧录信息验证
                if (config.CUSTOM_CODE == "TP")
                {
                    errMsg = await CheckBurnStateAsync(indata);
                    if (errMsg.Length > 0)
                    {
                        HandleError(errMsg);
                        return;
                    }

                }
                // errMsg没有内容，则表示所有板子验证成功，上传过站信息
                errMsg = await UploadState(indata);
                if (errMsg.Length > 0)
                {
                    HandleError(errMsg);
                    return;
                }
                // errMsg没有内容，则表示所有板子验证成功，上传过站信息
                string successMsg = string.Join(",", indata.Where(el => !string.IsNullOrEmpty(el))) + "过站成功";
                HandleSuccess(successMsg);
            }
            catch (Exception e)
            {
                LogHelper.Info($"Serial Port Open Error:{e}");
            }

        }
        private void HandleSuccess(string errMsg)
        {

            this.Invoke(new MethodInvoker(delegate
            {
                ErrorMSG(errMsg);
                LogHelper.Info(errMsg);
                this.textBox1.Focus();
                this.textBox1.SelectAll();
                this.TransferSNTOCOM(config.High);
                LogHelper.Info("Send High Command: " + config.High);
                // 新增：显示置顶窗体
                MessageForm.ShowMessage(errMsg, false);
            }));
        }
        private void HandleError(string errMsg)
        {

            this.Invoke(new MethodInvoker(delegate
            {
                ErrorMSG(errMsg);
                LogHelper.Info(errMsg);
                this.textBox1.Focus();
                this.textBox1.SelectAll();
                this.TransferSNTOCOM(config.Low);
                LogHelper.Info("Send Low Command:" + config.Low);

                // 新增：显示置顶窗体
                MessageForm.ShowMessage(errMsg, true);
            }));
        }

        private async Task<string> UploadState(List<string> indata)
        {

            string errMsg = "";
            LogHelper.Info("所有板子验证成功，开始上传过站信息");
            List<Task<int>> uploadTasks = indata.Select(snr => (Task.Run(() => this.trUploadState(config.StationNumber, snr, 2)))).ToList();
            int[] uploadRes = await Task.WhenAll(uploadTasks);
            for (int i = 0; i < uploadRes.Length; i++)
            {
                if (uploadRes[i] != 0)
                {
                    errMsg += "" + indata[i] + "过站失败!";
                }
            }
            return errMsg;
        }
        private string CheckBurnState(List<string> indata)
        {

            string errMsg = "";
            LogHelper.Info("开始验证板子烧录信息");
            foreach (var snr in indata)
            {
                var err = this.selectGw.GetBurn(config.DBType, config.Version, snr);
                if (err == "ERROR")
                {
                    errMsg += "" + snr + "序列号获取烧录失败!";

                }
            }
            return errMsg;
        }
        private async Task<string> CheckBurnStateAsync(List<string> indata)
        {

            string errMsg = "";
            LogHelper.Info("开始验证板子烧录信息");
            List<Task<string>> burnTasks = indata.Select(snr => Task.Run(() => this.selectGw.GetBurnAsync(config.DBType, config.Version, snr))).ToList();
            string[] burnRes = await Task.WhenAll(burnTasks);
            // 校验结果，如果有任一一个值等于ERROR就显示失败信息
            for (int i = 0; i < burnRes.Length; i++)
            {
                if (burnRes[i] == "ERROR")
                {
                    errMsg += "" + indata[i] + "序列号获取烧录失败!";
                }
            }
            return errMsg;
        }
        private async Task<string> CheckItacStateAsync(List<string> indata)
        {

            string errMsg = "";
            LogHelper.Info("Verify the status of product: " + string.Join(",", indata));
            List<Task<int>> tasks = indata.Select(snr => Task.Run(() => this.trCheckSerialNumberState(config.StationNumber, 2, 0, snr, "1"))).ToList();
            // 分别验证板子的序列号
            int[] res = await Task.WhenAll(tasks);
            // 迭代校验结果，如果有任一一个值不是0就显示失败信息并退出
            for (int i = 0; i < res.Length; i++)
            {
                if (res[i] != 0)
                {
                    errMsg += "" + indata[i] + "序列号校验失败!";
                }
            }
            return errMsg;
        }

        private void CheckNumberStateOld(object sender)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                Thread.Sleep(200);
                Byte[] bt = new Byte[sp.BytesToRead];
                sp.Read(bt, 0, sp.BytesToRead);
                string s = Encoding.ASCII.GetString(bt).Trim();
                LogHelper.Info("收到扫描枪扫描信息: " + s);
                this.Invoke(new MethodInvoker(delegate
                {
                    this.textBox1.Text = s;
                    if (config.Board_Module == "M")
                    {
                        string[] indata = s.Trim().Split(new char[] { ';' });
                        if (indata.Length == 2)
                        {
                            LogHelper.Info("分割后两个板子的序列号分别为:" + indata[0] + "和" + indata[1]);
                            int error = this.trCheckSerialNumberState(config.StationNumber, 2, 0, indata[0], "1");
                            if (error == 0)
                            {
                                int error01 = this.trCheckSerialNumberState(config.StationNumber, 2, 0, indata[1], "1");
                                if (error01 == 0)
                                {
                                    if (config.CUSTOM_CODE == "TP")
                                    {
                                        string burn_v = this.selectGw.GetBurn(config.DBType, config.Version, indata[0]);
                                        if (burn_v == "ERROR")
                                        {
                                            ErrorMSG(indata[0] + "烧录站点找不到数据!");
                                            LogHelper.Info(indata[0] + ":烧录站点找不到数据");
                                            this.textBox1.Focus();
                                            this.textBox1.SelectAll();
                                            TransferSNTOCOM(config.Low);
                                            LogHelper.Info("发送Low指令:" + config.Low);
                                        }
                                        else
                                        {
                                            LogHelper.Info(indata[0] + ":烧录站点找到数据:" + burn_v);
                                            string burn_v_1 = this.selectGw.GetBurn(config.DBType, config.Version, indata[1]);
                                            if (burn_v_1 == "ERROR")
                                            {
                                                ErrorMSG(indata[1] + "烧录站点找不到数据!");
                                                LogHelper.Info(indata[1] + ":烧录站点找不到数据");
                                                this.textBox1.Focus();
                                                this.textBox1.SelectAll();
                                                this.TransferSNTOCOM(config.Low);
                                                LogHelper.Info("发送Low指令:" + config.Low);
                                            }
                                            else
                                            {
                                                LogHelper.Info(indata[1] + ":烧录站点找到数据:" + burn_v_1);
                                                int err02 = this.trUploadState(config.StationNumber, indata[0], 2);
                                                int err03 = this.trUploadState(config.StationNumber, indata[1], 2);
                                                SuccessMSG(indata[0] + "和" + indata[1] + "过站成功");
                                                this.textBox1.Focus();
                                                this.textBox1.SelectAll();
                                                this.TransferSNTOCOM(config.High);
                                                LogHelper.Info("发送High指令:" + config.High);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int err02 = this.trUploadState(config.StationNumber, indata[0], 2);
                                        int err03 = this.trUploadState(config.StationNumber, indata[1], 2);
                                        SuccessMSG(indata[0] + "和" + indata[1] + "过站成功");
                                        this.textBox1.Focus();
                                        this.textBox1.SelectAll();
                                        this.TransferSNTOCOM(config.High);
                                        LogHelper.Info("发送High指令:" + config.High);
                                    }
                                }
                                else
                                {
                                    ErrorMSG(indata[1] + "流程检查不通过");
                                    LogHelper.Info(indata[1] + "流程检查不通过");
                                    this.textBox1.Focus();
                                    this.textBox1.SelectAll();
                                    this.TransferSNTOCOM(config.Low);
                                    LogHelper.Info("发送Low指令:" + config.Low);
                                }
                            }
                            else
                            {
                                ErrorMSG(indata[0] + "流程检查不通过");
                                LogHelper.Info(indata[0] + "流程检查不通过");
                                this.textBox1.Focus();
                                this.textBox1.SelectAll();
                                this.TransferSNTOCOM(config.Low);
                                LogHelper.Info("发送Low指令:" + config.Low);
                            }
                        }
                        else
                        {
                            ErrorMSG("目前仅支持两组板号并且需要以;隔开");
                            textBox1.Focus();
                            textBox1.SelectAll();
                        }
                    }
                    else
                    {
                        string indata = s.Trim();
                        int error = this.trCheckSerialNumberState(config.StationNumber, 2, 0, indata, "1");
                        if (error == 0)
                        {
                            if (config.CUSTOM_CODE == "TP")
                            {
                                string burn_v = this.selectGw.GetBurn(config.DBType, config.Version, indata);
                                if (burn_v == "ERROR")
                                {
                                    ErrorMSG(indata + "烧录站点找不到数据!");
                                    LogHelper.Info(indata + ":烧录站点找不到数据");
                                    this.textBox1.Focus();
                                    this.textBox1.SelectAll();
                                    TransferSNTOCOM(config.Low);
                                    LogHelper.Info("发送Low指令:" + config.Low);
                                }
                                else
                                {
                                    LogHelper.Info(indata + ":烧录站点找到数据:" + burn_v);
                                    int err01 = this.trUploadState(config.StationNumber, indata, 2);
                                    SuccessMSG(indata + "过站成功");
                                    this.textBox1.Focus();
                                    this.textBox1.SelectAll();
                                    this.TransferSNTOCOM(config.High);
                                    LogHelper.Info("发送High指令:" + config.High);
                                }
                            }
                            else
                            {
                                int err01 = this.trUploadState(config.StationNumber, indata, 2);
                                SuccessMSG(indata + "过站成功");
                                this.textBox1.Focus();
                                this.textBox1.SelectAll();
                                this.TransferSNTOCOM(config.High);
                                LogHelper.Info("发送High指令:" + config.High);
                            }
                        }
                        else
                        {
                            ErrorMSG(indata + "流程检查不通过");
                            LogHelper.Info(indata + "流程检查不通过");
                            this.textBox1.Focus();
                            this.textBox1.SelectAll();
                            this.TransferSNTOCOM(config.Low);
                            LogHelper.Info("发送Low指令:" + config.Low);
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + ";" + ex.StackTrace);
            }
            finally
            {
                sp.DiscardInBuffer();
            }
        }

        public void errorHandler(int typeOfError, string logMessage)
        {


            String errorBuilder = null;
            String isSucces = null;
            switch (typeOfError)
            {
                case 0:
                    this.Invoke(new MethodInvoker(delegate
                    {
                        //richTextBox1.SelectionColor = Color.Green;
                        isSucces = "SUCCESS";
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + "  " + logMessage + "\n";

                    }));

                    break;
                case 1:
                    this.Invoke(new MethodInvoker(delegate
                    {
                        //richTextBox1.SelectionColor = Color.Red;
                        isSucces = "FAIL";
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + "  " + logMessage + "\n";

                    }));
                    break;
                default:
                    //richTextBox1.SelectionColor = Color.Red;
                    isSucces = "info";
                    errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + "  " + logMessage + "\n";
                    break;
                    ;
            }
            this.Invoke(new MethodInvoker(delegate
            {
                //richTextBox1.AppendText(errorBuilder);
                //richTextBox1.ScrollToCaret();
            }));

        }
        public DataSet ExcelToDS(string Path)
        {
            string strConnection = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
            //string strConnection = "Provider=Microsoft.ACE.OLEDB.12.0;Extended Properties='Excel 12.0 Xml;HDR=YES;';data source=" + Path;
            OleDbConnection conn = new OleDbConnection(strConnection);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            strExcel = "select *,'待处理' as STATE from [sheet1$]";
            myCommand = new OleDbDataAdapter(strExcel, strConnection);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            return ds;
        }


        public string[] GetObjectsForAttributeValuesForMBN(string attributeCode, string attributeValue)
        {
            KeyValue[] attributeFilters = new KeyValue[] { };
            string[] objectResultKeys = new string[] { "MATERIAL_BIN_NUMBER", "PART_NUMBER" };
            string[] objectResultValues = new string[] { };
            int error = imsapi.attribGetObjectsForAttributeValues(sessionContext, config.StationNumber, 2, attributeCode, attributeValue, 10000, attributeFilters, objectResultKeys, out objectResultValues);
            LogHelper.Info("Api attribGetObjectsForAttributeValues attribute code =" + attributeCode + " ,attribute value =" + attributeValue + " ,result code =" + error);
            foreach (var item in objectResultValues)
            {
                LogHelper.Debug(item);
            }
            return objectResultValues;
        }
        public string[] GetPartnodetail(string partno, out int error)
        {
            string errorMsg = "";
            // MaterialBinEntity entity = new MaterialBinEntity();
            KeyValue[] materialBinFilters = new KeyValue[] { new KeyValue("MATERIAL_BIN_PART_NUMBER", partno) };
            string[] materialBinResultKeys = new string[] { "MATERIAL_BIN_NUMBER" };
            string[] materialBinResultValues = new string[] { };
            AttributeInfo[] attr = new AttributeInfo[] { };
            // LogHelper.Info("begin api mlGetMaterialBinData ");
            error = imsapi.mlGetMaterialBinData(sessionContext, config.StationNumber, materialBinFilters, attr, materialBinResultKeys, out materialBinResultValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            //LogHelper.Info("end api mlGetMaterialBinData (errorcode = " + error + ")");
            if (error == 0)
            {
                errorHandler(0, partno + ": get partno detaill success");
            }
            else
            {
                errorHandler(1, partno + ": get partno detaill fail");
            }
            return materialBinResultValues;
        }
        private Decimal ChangeDataToD(string strData)
        {
            Decimal dData = 0.0M;
            if (strData.Contains("E"))
            {
                dData = Convert.ToDecimal(Decimal.Parse(strData.ToString(), NumberStyles.Float));
            }
            else
            {
                dData = Convert.ToDecimal(strData);
            }
            return dData;
        }
        private DateTime ConvertStampToDate(string strValue)
        {
            long numer = long.Parse(strValue);
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(numer).ToLocalTime();
            return date;
        }

        public int AppendAttributeForAll(int objectType, string objectNumber, string objectDetail, string attributeCode, string attributeValue)
        {
            int error = 0;
            string errorMsg = "";
            string[] attributeUploadKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[] { attributeCode, attributeValue, "0" };
            string[] attributeResultValues = new string[] { };
            error = imsapi.attribAppendAttributeValues(sessionContext, config.StationNumber, objectType, objectNumber, objectDetail, -1, 1, attributeUploadKeys, attributeUploadValues, out attributeResultValues);
            LogHelper.Info("Api attribAppendAttributeValues error=" + error + ",object type=" + objectType + ",object number=" + objectNumber + ",object detail=" + objectDetail + ",attribute code=" + attributeCode);
            if (error == 0)
            {
                LogHelper.Info(objectNumber + "对应的" + attributeCode + "属性值为" + attributeValue);
            }
            else
            {
                imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
                ErrorMSG(attributeCode + "属性值更新失败 " + error + ":" + errorMsg);
            }
            return error;
        }

        public int SetBinLocation(string materialbin, string location)
        {
            int error = 0;
            string errorMsg = "";
            long X = -1;
            error = imsapi.mlSetMaterialBinLocation(sessionContext, config.StationNumber, materialbin, X, location, "-1", 201);
            LogHelper.Info("Api mlSetMaterialBinLocation error=" + error);
            if (error == 0)
            {
                errorHandler(0, materialbin + "移动到储位" + location + "成功");
            }
            else
            {
                imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
                errorHandler(1, materialbin + "移动到储位" + location + "失败");
            }
            return error;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //this.button1.Enabled = true;

        }

        public int LockObjects(int objectType, string lockGroupName, string lockInformation, string materialBinNo)
        {
            string[] objectUploadKeys = new string[] { "ERROR_CODE", "MATERIAL_BIN_NUMBER" };
            string[] objectUploadValues = new string[] { "0", materialBinNo };
            string[] objectResultValues = new string[] { };
            //int lockState = GetMaterialBinLockState(materialBinNo);
            //if (lockState == -1)
            //{
            //    LogHelper.Info("The material bin has been locked before");
            //    return 0;
            //}
            int errorCode = imsapi.lockObjects(sessionContext, config.StationNumber, objectType, lockGroupName, lockInformation, -1, 0, objectUploadKeys, objectUploadValues, out objectResultValues);
            LogHelper.Info("Api lockObjects object type =" + objectType + ", lock group name =" + lockGroupName + ", material bin number =" + materialBinNo + ", result code =" + errorCode);
            return errorCode;
        }
        //public int SetMaterialLocation(string materialBinNo, string location, int tranType)
        //{
        //    string errorMsg = "";
        //    int error = imsapi.mlSetMaterialBinLocation(sessionContext, config.StationNumber, materialBinNo, -1, location, "-1", tranType);
        //    imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
        //    LogHelper.Info("Api mlSetMaterialBinLocation material bin number =" + materialBinNo + " ,location =" + location + " , transaction type =" + tranType + ", result code =" + error + " ,message =" + errorMsg);
        //    return error;
        //}
        public string GetPartNoFromMBN(string materialBinNumber)
        {
            string strPartNumber = "";
            string errorMsg = "";
            KeyValue[] materialBinFilters = new KeyValue[] { new KeyValue("INCLUDE_EMPTY_BIN", "1"), new KeyValue("MATERIAL_BIN_NUMBER", materialBinNumber) };
            string[] materialBinResultKeys = new string[] { "MATERIAL_BIN_PART_NUMBER", "SUPPLIER_CHARGE_NUMBER", "MATERIAL_BIN_DATE_CODE" };
            string[] materialBinResultValues = new string[] { };
            AttributeInfo[] attr = new AttributeInfo[] { };
            LogHelper.Info("begin api mlGetMaterialBinData material bin number =" + materialBinNumber);
            int error = imsapi.mlGetMaterialBinData(sessionContext, config.StationNumber, materialBinFilters, attr, materialBinResultKeys, out materialBinResultValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            LogHelper.Info("end api mlGetMaterialBinData (errorcode = " + error + ")");
            if (error == 0)
            {
                strPartNumber = materialBinResultValues[0];
            }

            return strPartNumber;
        }
        public int ChangeMatDate(string materialBinNumber, string Qty)
        {
            int error = 0;
            string errorMsg = "";
            KeyValue[] materialBinDataUploadValues = new KeyValue[] { new KeyValue("MATERIAL_BIN_QTY_TOTAL", Qty) };
            LogHelper.Info("begin api mlChangeMaterialBinData (material bin number =" + materialBinNumber + ")");
            error = imsapi.mlChangeMaterialBinData(sessionContext, config.StationNumber, materialBinNumber, materialBinDataUploadValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            LogHelper.Info("end api mlChangeMaterialBinData (errorcode = " + error + ")");
            return error;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        public int trCheckSerialNumberState(string station_number, int Player, int flag, string serial_number, string pos)
        {
            string errorMsg = "";
            string[] serialNumberStateResultKeys = new string[] { "ERROR_CODE", "SERIAL_NUMBER" };
            string[] serialNumberStateResultValues = new string[] { };
            int error = imsapi.trCheckSerialNumberState(sessionContext, station_number, Player, flag, serial_number, pos, serialNumberStateResultKeys, out serialNumberStateResultValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            LogHelper.Info("Api trCheckSerialNumberState SERIAL_NUMBER =" + station_number + ", result code =" + error + " ,message =" + errorMsg);
            if (error == 0)
            {
                LogHelper.Info(serial_number + " The foolproof check has passed");
                return error;
            }
            if (serialNumberStateResultValues != null && serialNumberStateResultValues?.Length != 0)
            {
                string snr;
                int err;
                int loop = serialNumberStateResultKeys.Length;
                int count = serialNumberStateResultValues.Length;
                if (count == 0)
                {
                    imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
                    //ErrorMSG(serial_number + " " + error + " " + errorMsg);
                    LogHelper.Info(serial_number + " " + error + " " + errorMsg);
                    return error;
                }
                for (int i = 0; i < count; i += loop)
                {
                    err = int.Parse(serialNumberStateResultValues[i].ToString());
                    snr = serialNumberStateResultValues[i + 1].ToString();
                    if (err != 0)
                    {
                        imsapi.imsapiGetErrorText(sessionContext, err, out errorMsg);
                        ErrorMSG(snr + " " + err + " " + errorMsg);
                        LogHelper.Info(snr + " " + err + " " + errorMsg);
                    }
                    else
                    {
                        LogHelper.Info(snr + " " + err);
                    }
                }
            }
            return error;
        }

        public int trSwitchSerialNumber(string serial_no, string pos, string snr_new)
        {
            string errorMsg = "";
            SwitchSerialNumberData[] serialNumberArray = new SwitchSerialNumberData[1] { new SwitchSerialNumberData(0, snr_new, pos, serial_no, 0) };
            LogHelper.Info("begin api trSwitchSerialNumber SERIAL_NUMBER =" + serial_no);
            int error = imsapi.trSwitchSerialNumber(sessionContext, config.StationNumber, "-1", "-1", ref serialNumberArray);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            LogHelper.Info("end api trSwitchSerialNumber (errorcode = " + error + ")");
            if (error == 0)
            {
                LogHelper.Info(serial_no + "转换为" + snr_new + "成功");
            }
            else
            {
                ErrorMSG(serial_no + "转换为" + snr_new + "失败" + error + " " + errorMsg);
                LogHelper.Info(serial_no + "转换为" + snr_new + "失败" + error + " " + errorMsg);
            }
            return error;
        }

        public int trUploadState(string station_no, string serial_number, int Player)
        {
            int error = 0;
            string errorMsg = "";
            string[] serialNumberUploadKeys = new string[] { "ERROR_CODE", "SERIAL_NUMBER", "SERIAL_NUMBER_STATE" };
            string[] serialNumberUploadValues = new string[] { };
            string[] serialNumberResultValues = new string[] { };
            LogHelper.Info("begin api trUploadState ");
            error = imsapi.trUploadState(sessionContext, station_no, Player, serial_number, "1", 0, 0, -1, float.Parse("0"), serialNumberUploadKeys, serialNumberUploadValues, out serialNumberResultValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            LogHelper.Info("end api trUploadState (errorcode = " + error + ")");
            if (error == 0)
            {
                LogHelper.Info(serial_number + " trUploadState " + error);
            }
            else
            {
                LogHelper.Info(serial_number + " trUploadState " + error + " :" + errorMsg);
            }
            return error;
        }

        public int trUploadFailureAndResultData(int player, string serial_number, string pos, int state, int ds, string measure_name, string measure_value, string f_code)
        {
            int error = 0;
            string errorMsg = "";
            string[] measureKeys = new string[] { "ERROR_CODE", "MEASURE_FAIL_CODE", "MEASURE_NAME", "MEASURE_VALUE" };
            string[] measureValues = new string[] { "0", f_code, measure_name, measure_value };
            string[] measureResultValues = new string[] { };
            string[] failureKeys = new string[] { };
            string[] failureValues = new string[] { };
            string[] failureResultValues = new string[] { };
            string[] failureSlipKeys = new string[] { };
            string[] failureSlipValues = new string[] { };
            string[] failureSlipResultValues = new string[] { };
            LogHelper.Info("begin api trUploadFailureAndResultData ");
            error = imsapi.trUploadFailureAndResultData(sessionContext, config.StationNumber, player, serial_number, pos, state, ds, -1, -1, measureKeys, measureValues, out measureResultValues, failureKeys, failureValues, out failureResultValues, failureSlipKeys, failureSlipValues, out failureSlipResultValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            LogHelper.Info("end api trUploadFailureAndResultData (errorcode = " + error + ")");
            if (error == 0)
            {
                LogHelper.Info(serial_number + " trUploadFailureAndResultData " + error);
            }
            else
            {
                LogHelper.Info(serial_number + " trUploadFailureAndResultData " + error + " :" + errorMsg);
                ErrorMSG(serial_number + " trUploadFailureAndResultData " + error + " :" + errorMsg);
            }
            return error;
        }

        public int trUploadFailureAndResultData01(int player, string serial_number, string pos, int state, int ds, string[] measureValues)
        {
            int error = 0;
            string errorMsg = "";
            string[] measureKeys = new string[] { "ERROR_CODE", "MEASURE_NAME", "MEASURE_VALUE", "MEASURE_FAIL_CODE" };
            string[] measureResultValues = new string[] { };
            string[] failureKeys = new string[] { };
            string[] failureValues = new string[] { };
            string[] failureResultValues = new string[] { };
            string[] failureSlipKeys = new string[] { };
            string[] failureSlipValues = new string[] { };
            string[] failureSlipResultValues = new string[] { };
            LogHelper.Info("begin api trUploadFailureAndResultData ");
            error = imsapi.trUploadFailureAndResultData(sessionContext, config.StationNumber, player, serial_number, pos, state, ds, -1, -1, measureKeys, measureValues, out measureResultValues, failureKeys, failureValues, out failureResultValues, failureSlipKeys, failureSlipValues, out failureSlipResultValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            LogHelper.Info("end api trUploadFailureAndResultData (errorcode = " + error + ")");
            if (error == 0)
            {
                LogHelper.Info(serial_number + " trUploadFailureAndResultData " + error);
            }
            else
            {
                LogHelper.Info(serial_number + " trUploadFailureAndResultData " + error + " :" + errorMsg);
                ErrorMSG(serial_number + " trUploadFailureAndResultData " + error + " :" + errorMsg);
            }
            return error;
        }

        private void txtChange_Click(object sender, EventArgs e)
        {
            OpenFileDialog filetxt = new OpenFileDialog();
            filetxt.Filter = "*.xls| *.xlsx";
            filetxt.RestoreDirectory = true;
            filetxt.FilterIndex = 1;
            if (filetxt.ShowDialog() == DialogResult.OK)
            {
                string path = filetxt.FileName;
                this.textBox1.Text = path;
                errorHandler(0, "找到对应的文件:" + path);
            }
            if (!string.IsNullOrEmpty(this.textBox1.Text))
            {
                string seekfilepath = this.textBox1.Text;
                DataSet test = ExcelToDS(seekfilepath);
                //this.dg1.DataSource = test.Tables[0];
                //string count = (this.dg1.Rows.Count - 1).ToString();
                //errorHandler(0, "共查询到数量为:" + count + " 的Location的详细信息,等待导入");
            }
        }

        private void txtChangeNut_Click(object sender, EventArgs e)
        {

        }

        private void SuccessMSG(string message)
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new Action(() => SuccessMSG(message)));
            }
            else
            {
                label1.Text = message;
                label1.ForeColor = Color.Blue;
            }
        }

        private void ErrorMSG(string message)
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new Action(() => ErrorMSG(message)));
            }
            else
            {
                label1.Text = message;
                label1.ForeColor = Color.Red;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            ExecutionResult exeRes;
            exeRes = new ExecutionResult();
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text != "")
                {
                    if (config.Merge == "Y")
                    {
                    }
                    else
                    {
                        if (config.LogInType == "COM")
                        {
                            if (config.Board_Module == "M")
                            {
                                string[] s = textBox1.Text.Trim().Split(new char[] { ';' });
                                if (s.Length == 2)
                                {
                                    int error = this.trCheckSerialNumberState(config.StationNumber, 2, 0, s[0], "1");
                                    if (error == 0)
                                    {
                                        int error01 = this.trCheckSerialNumberState(config.StationNumber, 2, 0, s[1], "1");
                                        if (error01 == 0)
                                        {
                                            if (config.CUSTOM_CODE == "TP")
                                            {
                                                string burn_v = this.selectGw.GetBurn(config.DBType, config.Version, s[0]);
                                                if (burn_v == "ERROR")
                                                {
                                                    ErrorMSG(s[0] + "烧录站点找不到数据!");
                                                    LogHelper.Info(s[0] + ":烧录站点找不到数据");
                                                    this.textBox1.Focus();
                                                    this.textBox1.SelectAll();
                                                    TransferSNTOCOM(config.Low);
                                                    LogHelper.Info("发送Low指令:" + config.Low);
                                                }
                                                else
                                                {
                                                    LogHelper.Info(s[0] + ":烧录站点找到数据:" + burn_v);
                                                    string burn_v_1 = this.selectGw.GetBurn(config.DBType, config.Version, s[1]);
                                                    if (burn_v_1 == "ERROR")
                                                    {
                                                        ErrorMSG(s[1] + "烧录站点找不到数据!");
                                                        LogHelper.Info(s[1] + ":烧录站点找不到数据");
                                                        this.textBox1.Focus();
                                                        this.textBox1.SelectAll();
                                                        this.TransferSNTOCOM(config.Low);
                                                        LogHelper.Info("发送Low指令:" + config.Low);
                                                    }
                                                    else
                                                    {
                                                        LogHelper.Info(s[1] + ":烧录站点找到数据:" + burn_v_1);
                                                        int err02 = this.trUploadState(config.StationNumber, s[0], 2);
                                                        int err03 = this.trUploadState(config.StationNumber, s[1], 2);
                                                        SuccessMSG(s[0] + "和" + s[1] + "过站成功");
                                                        this.textBox1.Focus();
                                                        this.textBox1.SelectAll();
                                                        this.TransferSNTOCOM(config.High);
                                                        LogHelper.Info("发送High指令:" + config.High);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                int err02 = this.trUploadState(config.StationNumber, s[0], 2);
                                                int err03 = this.trUploadState(config.StationNumber, s[1], 2);
                                                SuccessMSG(s[0] + "和" + s[1] + "过站成功");
                                                this.textBox1.Focus();
                                                this.textBox1.SelectAll();
                                                this.TransferSNTOCOM(config.High);
                                                LogHelper.Info("发送High指令:" + config.High);
                                            }
                                        }
                                        else
                                        {
                                            ErrorMSG(s[1] + "流程检查不通过");
                                            LogHelper.Info(s[1] + "流程检查不通过");
                                            this.textBox1.Focus();
                                            this.textBox1.SelectAll();
                                            this.TransferSNTOCOM(config.Low);
                                            LogHelper.Info("发送Low指令:" + config.Low);
                                        }
                                    }
                                    else
                                    {
                                        ErrorMSG(s[0] + "流程检查不通过");
                                        LogHelper.Info(s[0] + "流程检查不通过");
                                        this.textBox1.Focus();
                                        this.textBox1.SelectAll();
                                        this.TransferSNTOCOM(config.Low);
                                        LogHelper.Info("发送Low指令:" + config.Low);
                                    }
                                }
                                else
                                {
                                    ErrorMSG("目前仅支持两组板号并且需要以分号隔开");
                                    textBox1.Focus();
                                    textBox1.SelectAll();
                                }
                            }
                            else
                            {
                                string indata = textBox1.Text.Trim();
                                int error = this.trCheckSerialNumberState(config.StationNumber, 2, 0, indata, "1");
                                if (error == 0)
                                {
                                    string burn_v = this.selectGw.GetBurn(config.DBType, config.Version, indata);
                                    if (burn_v == "ERROR")
                                    {
                                        ErrorMSG(indata + "烧录站点找不到数据!");
                                        LogHelper.Info(indata + ":烧录站点找不到数据");
                                        this.textBox1.Focus();
                                        this.textBox1.SelectAll();
                                        TransferSNTOCOM(config.Low);
                                        LogHelper.Info("发送Low指令:" + config.Low);
                                    }
                                    else
                                    {

                                        int err01 = this.trUploadState(config.StationNumber, indata, 2);
                                        SuccessMSG(indata + "过站成功");
                                        this.textBox1.Focus();
                                        this.textBox1.SelectAll();
                                        this.TransferSNTOCOM(config.High);
                                        LogHelper.Info("发送High指令:" + config.High);
                                    }

                                }
                                else
                                {
                                    ErrorMSG(indata + "流程检查不通过");
                                    LogHelper.Info(indata + "流程检查不通过");
                                    this.textBox1.Focus();
                                    this.textBox1.SelectAll();
                                    this.TransferSNTOCOM(config.Low);
                                    LogHelper.Info("发送Low指令:" + config.Low);
                                }
                            }
                        }
                        else
                        {
                            string ss = textBox1.Text.Trim();
                            int error = this.trCheckSerialNumberState(config.StationNumber, 2, 0, ss, "1");
                            if (error == 0)
                            {
                                if (config.CUSTOM_CODE == "TP")
                                {
                                    string burn_v = this.selectGw.GetBurn(config.DBType, config.Version, ss);
                                    if (burn_v == "ERROR")
                                    {
                                        ErrorMSG(ss + "烧录站点找不到数据!");
                                        LogHelper.Info(ss + ":烧录站点找不到数据");
                                        this.textBox1.Focus();
                                        this.textBox1.SelectAll();
                                    }
                                    else
                                    {
                                        int err02 = this.trUploadState(config.StationNumber, ss, 2);
                                        SuccessMSG("上传数据成功!");
                                        this.textBox1.Focus();
                                        this.textBox1.SelectAll();
                                    }
                                }
                                else
                                {
                                    int err02 = this.trUploadState(config.StationNumber, ss, 2);
                                    SuccessMSG("上传数据成功!");
                                    this.textBox1.Focus();
                                    this.textBox1.SelectAll();
                                }
                            }
                            else
                            {
                                ErrorMSG("烧录站点找不到数据!");
                                this.textBox1.Focus();
                                this.textBox1.SelectAll();
                            }
                        }
                    }
                }
                else
                {
                    ErrorMSG("请扫描或输入一个正确的条码!");
                    LogHelper.Info("请扫描或输入一个正确的条码!");
                    this.textBox1.Focus();
                    this.textBox1.SelectAll();
                }
            }
        }

        private void SendSNToCOM(string hsn) // add by robert 2022/6/9
        {
            SerialPort serialPort = new SerialPort();
            if (config.SerialPort != "")
            {
                serialPort.PortName = config.SerialPort;
                serialPort.BaudRate = int.Parse(config.BaudRate);
                serialPort.DataBits = int.Parse(config.DataBits);
                serialPort.StopBits = (StopBits)1;
                serialPort.Parity = (Parity)int.Parse(config.Parity);
                serialPort.NewLine = config.NewLineSymbol;
                serialPort.Handshake = Handshake.None;

                try
                {
                    serialPort.Open();
                }
                catch (Exception)
                {

                    MessageBox.Show("the port is invalid,pls change another.");
                    return;
                }
                char[] charArray;
                //String text = textBox2.Text;
                String tmpString = hsn.Trim();
                if (tmpString == "")
                {
                    MessageBox.Show("sn is null");
                    return;
                }
                else
                {
                    tmpString = tmpString.Replace("ESC", "*") + (char)13;
                    charArray = tmpString.ToCharArray();

                    serialPort.Write(charArray, 0, charArray.Length);
                    Thread.Sleep(Convert.ToInt32("300"));
                    serialPort.Close();
                }
            }
            else
            {
                MessageBox.Show("The Port is Empty,Pls Chose One");
                return;
            }
        }

        private void TransferSNTOCOM(string hsn) // add by robert 2022/9/4
        {
            if (config.SerialPort != "")
            {

                //char[] charArray;
                //String tmpString = hsn.Trim();

                //if (tmpString == "")
                //{
                //    MessageBox.Show("sn is null");
                //    return;
                //}
                //else
                //{

                //    tmpString = tmpString.Replace("ESC", "*") + (char)13;
                //    charArray = tmpString.ToCharArray();
                //    serialPort.Write(charArray, 0, charArray.Length);
                //}
                string tmpString = hsn.Trim();
                byte[] array = this.strToToHexByte(tmpString);
                try
                {
                    serialPort.Write(array, 0, array.Length);
                }
                catch (Exception msg)
                {
                    LogHelper.Error(msg);
                }
            }
            else
            {
                MessageBox.Show("The Port is Empty,Pls Chose One");
                return;
            }
        }

        private byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                hexString += " ";
            }
            byte[] array = new byte[hexString.Length / 2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return array;
        }
    }
}
