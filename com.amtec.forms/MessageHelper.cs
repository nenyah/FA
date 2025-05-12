using System.Drawing;
namespace FA_COATING.com.amtec.forms
{
    class MessageHelper
    {
        private readonly Form1 _form1;
        public MessageHelper(Form1 form1)
        {
            _form1 = form1;
        }
        public void SuccessMsg(string message)
        {
            _form1.label1.Text = message;
            _form1.label1.ForeColor = Color.Blue;
        }
        public void ErrorMsg(string message)
        {
            _form1.label1.Text = message;
            _form1.label1.ForeColor = Color.Red;
        }
    }
}
