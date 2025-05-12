namespace FA_COATING.com.amtec.forms
{
    public partial class Form1 : Form
    {
        private MessageForm messageForm;

        public Form1(string userName, DateTime dTime, IMSApiSessionContextStruct _sessionContext, ApplicationConfiguration _config)
        {
            InitializeComponent();
            sessionContext = _sessionContext;
            config = _config;
            serialPort = new SerialPort();
            selectGw = new SelectGW(sessionContext);
            updateInsert = new UpdateInsert(sessionContext);
            messageForm = new MessageForm();
        }

        private void HandleSuccess(string errMsg)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                ErrorMSG(errMsg);
                LogHelper.Info(errMsg);
                messageForm.ShowMessage(errMsg);
                this.textBox1.Focus();
                this.textBox1.SelectAll();
                this.TransferSNTOCOM(config.High);
                LogHelper.Info("发送Low指令:" + config.High);
            }));
        }

        private void HandleError(string errMsg)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                ErrorMSG(errMsg);
                LogHelper.Info(errMsg);
                messageForm.ShowMessage(errMsg);
                this.textBox1.Focus();
                this.textBox1.SelectAll();
                this.TransferSNTOCOM(config.Low);
                LogHelper.Info("发送Low指令:" + config.Low);
            }));
        }
        
        // Other existing methods...
    }
}