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
            // MANAGER [name, cloud port, management port]
            string[] parameters = new string[] { args[0], args[1], args[2] };
            //DEBUG
            //string[] parameters = new string[] { "CN0", "10002", "10001"};
            //client = new ClientNode(parameters);
            //GUI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ClientWindow window = new ClientWindow(parameters);
            Application.Run(window);
        }
    }
}
