using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientNode
{
    class Program
    {

        private static ClientNode client = null;
        static void Main(string[] args)
        {
            // MANAGER
            string[] parameters = new string[] { args[0], args[1], args[2] };
            // client = new ClientNode(args);
            //DEBUG jakies paramsy (ip, port WE/WY, nazwa noda ta same co w apce-do logow)
            //TODO jaki typ mam wysyłac a jeśli vc3 to w którą pozycje 0, 1 czy 2
            //virtual ip, listener in/out, port do managmentu
            //string[] parameters = new string[] { "165.23.12.32", "10002", "10001"};
            //client = new ClientNode(parameters);
            //GUI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ClientWindow window = new ClientWindow(parameters);
            Application.Run(window);
        }
    }
}
