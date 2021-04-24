using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiDimensionalOptimization.algo;

namespace MultiDimensionalOptimization.draw
{
    public class Window
    {
        public const int Width = 1280;
        public const int Height = 720;
        private const double Ratio = (double) Height / Width;
        private readonly Action _refresh;
        private Bitmap _bitmap;
        private readonly Pen _gridPen;
        private readonly Color _minimalGradientColor = Color.Aqua;
        private readonly Color _maximalGradientColor = Color.Coral;

        public Window(Action refresh)
        {
            _refresh = refresh;
            _gridPen = new Pen(Color.LightGray);

            // var f = AdvancedMath.CreateDiagonalFunction(2, 4);
            Update();
        }

        private void Update()
        {
            var f = GetFunction();
            var algorithm = GetAlgorithm();
            var epsilon = GetEpsilon();
            var radius = GetRadius();

            var result = algorithm.Invoke(f, AdvancedMath.GenerateRandomStartVector(f.N), epsilon);
            var values = new double[] {1, 2, 3, 4, 5, 6, 7};
            CreateFunctionContoursBitmap(f, values,0, 0, radius);
        }

        private Function GetFunction()
        {
            return new(2, new double[] {1, 0, 0, 0}, new double[] {0, 1}, 0);
        }

        private Algorithm GetAlgorithm()
        {
            return Optimization.FASTEST_DESCENT;
        }

        private double GetEpsilon()
        {
            return 0.00001;
        }

        private double GetRadius()
        {
            return 30;
        }

        private void CreateFunctionContoursBitmap(Function f, double[] values, double x, double y, double r)
        {
            var gradientColors = GetColorsGradient(_maximalGradientColor, _minimalGradientColor, values.Length);
            var grid = GenerateGrid(f, x - r, x + r, y - r * Ratio, y + r * Ratio);
            _bitmap = new Bitmap(Width, Height);

            for (var i = 0; i < values.Length; i++)
            {
                GenerateBitmap(values[i], grid, gradientColors[i], _bitmap);
            }
        }

        public void Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics, 35);
            e.Graphics.DrawImage(_bitmap, 0, 0);
        }

        private void DrawGrid(Graphics g, int count)
        {
            int centerX = Width / 2;
            int centerY = Height / 2;
            int delta = Width / count;

            for (int i = 0; i < (count + 1) / 2; i++)
            {
                g.DrawLine(_gridPen, centerX - i * delta, 0, centerX - i * delta, Height);
                g.DrawLine(_gridPen, centerX + i * delta, 0, centerX + i * delta, Height);
                g.DrawLine(_gridPen, 0, centerY - i * delta, Width, centerY - i * delta);
                g.DrawLine(_gridPen, 0, centerY + i * delta, Width, centerY + i * delta);
            }
        }

        private const double PRESICION = 0.02;

        private static void GenerateBitmap(double value, double[][] grid, Color color, Bitmap bitmap)
        {
            var epsilon = value * PRESICION;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Math.Abs(grid[i][j] - value) <= epsilon)
                    {
                        bitmap.SetPixel(i, j, color);
                    }
                }
            }
        }

        private double[][] GenerateGrid(
            Function f,
            double minX,
            double maxX,
            double minY,
            double maxY
        )
        {
            var grid = new double[Width][];
            double stepX = (maxX - minX) / Width;
            double stepY = (maxY - minY) / Height;
            var vector = new double[2];
            for (int i = 0; i < Width; i++)
            {
                grid[i] = new double[Height];
                for (int j = 0; j < Height; j++)
                {
                    vector[0] = minX + i * stepX;
                    vector[1] = minY + j * stepY;
                    grid[i][j] = f.Apply(vector);
                }
            }

            return grid;
        }

        private static List<Color> GetColorsGradient(Color color1, Color color2, int size)
        {
            int rMax = color1.R;
            int rMin = color2.R;
            int gMin = color1.G;
            int gMax = color1.G;
            int bMin = color1.B;
            int bMax = color1.B;
            var colorList = new List<Color>();
            for (int i = 0; i < size; i++)
            {
                var rAverage = rMin + (rMax - rMin) * i / size;
                var gAverage = gMin + (gMax - gMin) * i / size;
                var bAverage = bMin + (bMax - bMin) * i / size;
                colorList.Add(Color.FromArgb(rAverage, gAverage, bAverage));
            }

            return colorList;
        }
    }
}