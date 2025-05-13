using System;
using System.Drawing;
using System.Windows.Forms;

namespace FA_COATING.com.amtec.forms
{
    public partial class MessageForm : Form
    {
        private static MessageForm _instance;
        private static readonly object _lock = new object();

        public MessageForm()
        {
            InitializeComponent();
            // 计算右下角位置
            int x = Screen.PrimaryScreen.WorkingArea.Right - this.Width - 10;
            int y = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height - 10;
            this.Location = new Point(x, y);

            labelMsg.MaximumSize = new Size(this.Width - 20, 0); // 宽度适应窗体，0表示高度不限
            labelMsg.Size = new Size(this.Width - 20, this.Height - 20);
        }
        public static void ShowMessage(string msg, bool isError)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                lock (_lock)
                {
                    if (_instance == null || _instance.IsDisposed)
                        _instance = new MessageForm();
                }
            }

            // 强制创建句柄，避免Invoke异常
            var handle = _instance.Handle;

            if (_instance.InvokeRequired)
            {
                _instance.Invoke(new Action(() =>
                {
                    _instance.labelMsg.Text = msg;
                    _instance.labelMsg.ForeColor = isError ? Color.Red : Color.Blue;
                    _instance.Show();
                    _instance.BringToFront();
                }));
            }
            else
            {
                _instance.labelMsg.Text = msg;
                _instance.labelMsg.ForeColor = isError ? Color.Red : Color.Blue;
                _instance.Show();
                _instance.BringToFront();
            }
        }
    }
}
