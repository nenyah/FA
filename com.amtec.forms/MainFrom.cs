using com.amtec.configurations;
using com.itac.mes.imsapi.domain.container;
using System;
using System.IO.Ports;
using System.Windows.Forms;
namespace FA_COATING.com.amtec.forms
{
    public partial class MainFrom : Form
    {
        public ApplicationConfiguration config;
        private readonly IMSApiSessionContextStruct sessionContext;
        private readonly SerialPort serialPort;
        public MainFrom(string userName, DateTime dTime, IMSApiSessionContextStruct _sessionContext, ApplicationConfiguration _config)
        {
            InitializeComponent();
            sessionContext = _sessionContext;
            config = _config;
            serialPort = new SerialPort();
        }
        private void MainFrom_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

