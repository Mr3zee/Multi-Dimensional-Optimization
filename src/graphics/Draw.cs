using System;
using System.Linq;
// using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms;
using MultyDimentionalOptimization.algo;

namespace MultyDimentionalOptimization.graphics
{
    public class Draw
    {
        public static void StartDrawing()
        {
            // var n = 1000;
            // var f = AdvancedMath.CreateDiagonalFunction(n, 1000);
            // Console.Out.WriteLine(Optimization.FASTEST_DESCENT(f, Enumerable.Repeat(10.0, n).ToArray(), 0.0001));
        }
        
        public static void Paint(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var pen = new Pen(Color.Aqua);
            var brush = new SolidBrush(Color.Red);
            
            graphics.DrawLine(pen, 0, 0, 500, 500);
            
        }
    }
}