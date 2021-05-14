using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MultiDimensionalOptimization
{
    internal static class Program
    {
        [DllImport( "kernel32.dll" )]
        private static extern bool AttachConsole( int dwProcessId );
        private const int AttachParentProcess = -1;
        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AttachConsole(AttachParentProcess);
            
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ContourForm());
        }
    }
}