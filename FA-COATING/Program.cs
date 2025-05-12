using System;
using System.Windows.Forms;

namespace FA_COATING
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new com.amtec.forms.Form1());
        }
    }
}