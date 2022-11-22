using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using com.amtec.configurations;
using com.itac.mes.imsapi.domain.container;
using com.itac.mes.imsapi.client.dotnet;
using com.amtec.forms;
using System.Threading;
using com.amtec.action;
using SendPartno;

namespace com.amtec.forms
{
    public partial class LoginForm : Form
    {
        private ApplicationConfiguration config;
        private IMSApiSessionValidationStruct sessionValidationStruct;
        public IMSApiSessionContextStruct sessionContext = null;
        private SessionContextHeandler sessionContextHandler;
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        public string UserName = "";
        public int LoginResult = 0;
        public bool isCanLogin = false;

        public LoginForm()
        {
            InitializeComponent();
            this.progressBar1.Value = 0;
            this.progressBar1.Maximum = 100;
            this.progressBar1.Step = 1;

            this.timer1.Interval = 100;
            this.timer1.Tick += new EventHandler(timer_Tick);

            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(worker_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!VerifyLoginInfo())
                return;
            LogHelper.Info("Login start...");
            backgroundWorker1.RunWorkerAsync();
            this.lblErrorMsg.Text = "Loading application....";
            this.timer1.Start();
            SetControlStatus(false);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.progressBar1.Value < this.progressBar1.Maximum - 5)
            {
                this.progressBar1.Value++;
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer1.Stop();
            this.progressBar1.Value = this.progressBar1.Maximum;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //添加你初始化的代码
            config = new ApplicationConfiguration();
            if (!isCanLogin)
            {
                sessionContextHandler = new SessionContextHeandler(config, this);
            }
            sessionValidationStruct = new IMSApiSessionValidationStruct();
            sessionValidationStruct.stationNumber = config.StationNumber;
            sessionValidationStruct.stationPassword = "";
            sessionValidationStruct.user = this.txtUserName.Text.Trim();
            sessionValidationStruct.password = this.txtPassword.Text.Trim();
            sessionValidationStruct.client = config.Client;
            sessionValidationStruct.registrationType = config.RegistrationType;
            sessionValidationStruct.systemIdentifier = config.StationNumber;
            UserName = this.txtUserName.Text.Trim();

            LoginResult = imsapi.regLogin(sessionValidationStruct, out sessionContext);

            if (LoginResult != IMSApiDotNetConstants.RES_OK)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    SetStatusLabelText("api regLogin error.(result code=" + LoginResult + ")", 1);
                    SetControlStatus(true);
                }));
                return;
            }
            else
            {
                LogHelper.Info("api regLogin success.(result code=" + LoginResult + ")");
                if (!VerifyTeamNumber())
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        SetStatusLabelText("验证Team number失败", 1);
                        SetControlStatus(true);
                    }));
                    return;
                }
                this.Invoke(new MethodInvoker(delegate
                {
                    this.Hide();
                    Form1 view = new Form1(this.txtUserName.Text.Trim(), DateTime.Now, sessionContext, config);
                    view.ShowDialog();
                }));
            }
            LogHelper.Info("Login end...");
        }

        public delegate void SetStatusLabelTextDel(string strText, int iCase);
        public void SetStatusLabelText(string strText, int iCase)
        {
            if (this.lblErrorMsg.InvokeRequired)
            {
                SetStatusLabelTextDel setText = new SetStatusLabelTextDel(SetStatusLabelText);
                Invoke(setText, new object[] { strText, iCase });
            }
            else
            {
                this.lblErrorMsg.Text = strText;
                if (iCase == 0)
                {
                    this.lblErrorMsg.ForeColor = Color.Black;
                }
                else if (iCase == 1)
                {
                    this.lblErrorMsg.ForeColor = Color.Red;
                }
            }
        }

        private void SetControlStatus(bool isOK)
        {
            this.btnOK.Enabled = isOK;
            this.txtPassword.Enabled = isOK;
            this.txtUserName.Enabled = isOK;
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOK_Click(null, null);
            }
        }

        private bool VerifyLoginInfo()
        {
            bool isValidate = true;
            if (string.IsNullOrEmpty(this.txtUserName.Text.Trim()) || string.IsNullOrEmpty(this.txtPassword.Text.Trim()))
            {
                SetStatusLabelText("Pls input user name/password.", 1);
                isValidate = false;
            }

            return isValidate;
        }

        private bool VerifyTeamNumber()
        {
            bool isValid = false;
            UtilityFunction utilityHandler = new UtilityFunction(sessionContext, config.StationNumber);
            string teamNo = utilityHandler.GetTeamNumberByUser(this.txtUserName.Text.Trim());
            if (string.IsNullOrEmpty(config.AUTH_TEAM))
            {
                isValid = true;
            }
            else if (string.IsNullOrEmpty(teamNo))
            {
                isValid = false;
            }
            else
            {
                if (config.AUTH_TEAM.Contains(teamNo))
                {
                    isValid = true;
                }
            }
            return isValid;
        }
    }
}
