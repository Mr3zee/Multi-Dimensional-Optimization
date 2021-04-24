using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private List<double[]> _vectors;
        private readonly Pen _gridPen;
        private readonly Pen _vectorsPen;
        private readonly Color _minimalGradientColor = Color.Aqua;
        private readonly Color _maximalGradientColor = Color.Coral;

        public Window(Action refresh)
        {
            _refresh = refresh;
            _gridPen = new Pen(Color.LightGray);
            _vectorsPen = new Pen(Color.Black, 1) {EndCap = LineCap.ArrowAnchor};

            Update();
        }

        private void Update()
        {
            var f = GetFunction();
            var algorithm = GetAlgorithm();
            var epsilon = GetEpsilon();
            var radius = GetRadius();

            var result = algorithm.Invoke(f, AdvancedMath.GenerateRandomStartVector(f.N), epsilon);
            
            CreateFunctionContoursBitmap(f, result.Levels, result.X[0], result.X[1], radius);
        }

        private Function GetFunction()
        {
            return new(2, new double[] {2, 0, 0, 1}, new double[] {0, 0}, 0);
        }

        private Algorithm GetAlgorithm()
        {
            return Optimization.GRADIENT_DESCENT;
        }

        private double GetEpsilon()
        {
            return 0.00001;
        }

        private double GetRadius()
        {
            return 30;
        }

        private void CreateFunctionContoursBitmap(Function f, List<double[]> values, double x, double y, double r)
        {
            var gradientColors = GetColorsGradient(_maximalGradientColor, _minimalGradientColor, values.Count);
            var minX = x - r; 
            var maxX = x + r;
            var minY = y - r * Ratio;
            var maxY = y + r * Ratio;

            var grid = GenerateGrid(f, minX, maxX, minY, maxY);
            _bitmap = new Bitmap(Width, Height);

            for (var i = 0; i < values.Count; i++)
            {
                GenerateBitmap(f.Apply(values[i]), grid, gradientColors[i], _bitmap);
            }
            
            _vectors = new List<double[]>();
            values.Add(new []{x, y});
            
            foreach (var value in values)
            {
                var windowX = (value[0] - minX) / (2 * r) * Width;
                var windowY = (value[1] - minY) / (2 * r * Ratio) * Height;
                _vectors.Add(new [] { windowX, windowY });
            }
        }

        private const int LinesCount = 35;

        public void Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics, LinesCount);
            e.Graphics.DrawImage(_bitmap, 0, 0);
            for (var i = 0; i < _vectors.Count - 1; i++)
            {
                e.Graphics.DrawLine(
                    _vectorsPen, 
                    (float) _vectors[i][0],
                    (float) _vectors[i][1],
                    (float) _vectors[i + 1][0],
                    (float) _vectors[i + 1][1]
                );
            }
        }

        private void DrawGrid(Graphics g, int count)
        {
            const int centerX = Width / 2;
            const int centerY = Height / 2;
            var delta = Width / count;

            for (var i = 0; i < (count + 1) / 2; i++)
            {
                g.DrawLine(_gridPen, centerX - i * delta, 0, centerX - i * delta, Height);
                g.DrawLine(_gridPen, centerX + i * delta, 0, centerX + i * delta, Height);
                g.DrawLine(_gridPen, 0, centerY - i * delta, Width, centerY - i * delta);
                g.DrawLine(_gridPen, 0, centerY + i * delta, Width, centerY + i * delta);
            }
        }

        private const double Precision = 0.02;

        private static void GenerateBitmap(double value, double[][] grid, Color color, Bitmap bitmap)
        {
            var epsilon = value * Precision;
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

        private static double[][] GenerateGrid(
            Function f,
            double minX,
            double maxX,
            double minY,
            double maxY
        )
        {
            var grid = new double[Width][];
            var stepX = (maxX - minX) / Width;
            var stepY = (maxY - minY) / Height;
            var vector = new double[2];
            for (var i = 0; i < Width; i++)
            {
                grid[i] = new double[Height];
                for (var j = 0; j < Height; j++)
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