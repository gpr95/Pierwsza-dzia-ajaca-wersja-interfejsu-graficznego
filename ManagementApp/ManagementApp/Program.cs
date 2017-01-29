using System;
using System.Windows.Forms;

namespace ManagementApp
{
    static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //MainWindow mainWindow = new MainWindow();
            Application.Run(new MainWindow());
        }

        static void sw(IntPtr hWnd, bool fAltTab)
        {
            SwitchToThisWindow(hWnd, fAltTab);
            return;
        }
    }
}
