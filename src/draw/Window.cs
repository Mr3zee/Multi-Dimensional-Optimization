using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultyDimentionalOptimization.algo;

namespace MultyDimentionalOptimization.draw
{
    public class Window
    {
        public const int WIDTH = 1280;
        public const int HEIGHT = 720;
        private const double RATIO = (double) HEIGHT / WIDTH;
        private readonly Action refresh;
        private Bitmap bitmap;
        private readonly Pen GridPen;

        public Window(Action refresh)
        {
            this.refresh = refresh;
            GridPen = new Pen(Color.LightGray);

            var f = AdvancedMath.CreateDiagonalFunction(2, 1);
            var values = new double[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
            CreateFunctionContoursBitmap(f, values,0, 0, 10);
        }

        private void CreateFunctionContoursBitmap(Function f, double[] values, double x, double y, double r)
        {
            var gradientColors = GetColorsGradient(Color.Coral, Color.Aqua, values.Length);
            var grid = GenerateGrid(f, x - r, x + r, y - r * RATIO, y + r * RATIO);
            bitmap = new Bitmap(WIDTH, HEIGHT);

            for (int i = 0; i < values.Length; i++)
            {
                GenerateBitmap(values[i], grid, gradientColors[i], bitmap);
            }
        }

        public void Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics, 35);
            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        private void DrawGrid(Graphics g, int count)
        {
            int centerX = WIDTH / 2;
            int centerY = HEIGHT / 2;
            int delta = WIDTH / count;

            for (int i = 0; i < (count + 1) / 2; i++)
            {
                g.DrawLine(GridPen, centerX - i * delta, 0, centerX - i * delta, HEIGHT);
                g.DrawLine(GridPen, centerX + i * delta, 0, centerX + i * delta, HEIGHT);
                g.DrawLine(GridPen, 0, centerY - i * delta, WIDTH, centerY - i * delta);
                g.DrawLine(GridPen, 0, centerY + i * delta, WIDTH, centerY + i * delta);
            }
        }

        private const double PRESICION = 0.02;

        private static void GenerateBitmap(double value, double[][] grid, Color color, Bitmap bitmap)
        {
            var epsilon = value * PRESICION;
            for (int i = 0; i < WIDTH; i++)
            {
                for (int j = 0; j < HEIGHT; j++)
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
            var grid = new double[WIDTH][];
            double stepX = (maxX - minX) / WIDTH;
            double stepY = (maxY - minY) / HEIGHT;
            var vector = new double[2];
            for (int i = 0; i < WIDTH; i++)
            {
                grid[i] = new double[HEIGHT];
                for (int j = 0; j < HEIGHT; j++)
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