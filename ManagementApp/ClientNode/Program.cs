using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientWindow
{
    class Program
    {

       
        static void Main(string[] args)
        {

            string[] parameters = new string[] { args[0], args[1], args[2], args[3] };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ClientWindow window = new ClientWindow(parameters);
            Application.Run(window);
        }
    }
}
