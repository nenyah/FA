using System;
using System.Windows.Forms;

namespace FA_COATING.com.amtec.forms
{
    public partial class MessageForm : Form
    {
        public MessageForm()
        {
            InitializeComponent();
        }

        public void DisplayMessage(string message, bool isError)
        {
            labelMessage.Text = message;
            labelMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Blue;
            this.Show();
        }
    }
}