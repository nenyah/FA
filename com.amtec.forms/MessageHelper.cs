using System.Drawing;
namespace SendPartno
{
    class MessageHelper
    {
        private Form1 _form1;
        public MessageHelper(Form1 form1)
        {
            _form1 = form1;
        }
        private void SuccessMSG(string message)
        {
            _form1.label1.Text = message;
            _form1.label1.ForeColor = Color.Blue;
        }
        private void ErrorMSG(string message)
        {
            _form1.label1.Text = message;
            _form1.label1.ForeColor = Color.Red;
        }
    }
}
