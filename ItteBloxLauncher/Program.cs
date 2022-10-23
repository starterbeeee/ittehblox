using System;
using System.Windows.Forms;

namespace ItteBloxLauncher
{
    static class Program
    {
        public static string[] args;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arguments)
        {
            args = arguments;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LauncherWindow());
        }
    }
}
