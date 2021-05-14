using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MultiDimensionalOptimization.algo;

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

            // const int n = 10000;
            // var function = AdvancedMath.CreateDiagonalFunction(n, 1218);
            // var result = Optimization.GRADIENT_DESCENT.Invoke(function, Enumerable.Repeat(10.0, n).ToArray(), 0.0001);
            // Console.WriteLine(result.Y);
            // return;
            
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ContourForm());
        }
    }
}