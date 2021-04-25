using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using MultiDimensionalOptimization.algo;
using static System.Double;

namespace MultiDimensionalOptimization.draw
{
    public class Window
    {
        public const int Width = 1280;
        public const int Height = 720;
        private const double Ratio = (double) Height / Width;
        private readonly Action _refresh;
        private readonly Control.ControlCollection _controls;
        private Bitmap _bitmap;
        private List<double[]> _vectors;
        private readonly Pen _gridPen;
        private double[][] _grid;
        private bool _updateGrid;
        private readonly Pen _vectorsPen;
        private readonly Color _minimalGradientColor = Color.Aqua;
        private readonly Color _maximalGradientColor = Color.Coral;
        private readonly Dictionary<string, double> _parameters; 

        public Window(Action refresh, Control.ControlCollection controls)
        {
            _updateGrid = true;
            _refresh = refresh;
            _controls = controls;
            _gridPen = new Pen(Color.LightGray);
            _vectorsPen = new Pen(Color.Black, 1) {EndCap = LineCap.ArrowAnchor};
            _parameters = new Dictionary<string, double>();
            
            AddTextBox("a11", 2, 10, true);
            AddTextBox("a12", 0, 40, true);
            AddTextBox("a21", 0, 70, true);
            AddTextBox("a22", 1, 100, true);
            
            AddTextBox("b1", 0, 130, true);
            AddTextBox("b2", 0, 160, true);
            
            AddTextBox("c", 0, 190, true);
            
            AddTextBox("r", 30, 250, true);
            AddTextBox("epsilon", 0.00001, 280, false);
            
            AddTextBox("s1", 10, 340, false);
            AddTextBox("s2", 10, 370, false);
            
            Update();
        }

        private void AddTextBox(string name, double value, int offsetY, bool updateGrid)
        {
            var textBox = new TextBox
            {
                Multiline = false,
                AcceptsReturn = false,
                PlaceholderText = name,
                AcceptsTab = false,
                Text = value.ToString(CultureInfo.InvariantCulture),
                Top = offsetY,
                Left = 10,
                Height = 20,
                Anchor = AnchorStyles.Left & AnchorStyles.Top,
                
            };

            textBox.LostFocus += (sender, e) =>
            {
                var oldV = _parameters[textBox.PlaceholderText];
                TryParse(textBox.Text, out var newV);
                if (Math.Abs(oldV - newV) > newV * 0.0001)
                {
                    _parameters[textBox.PlaceholderText] = newV;
                    if (updateGrid)
                    {
                        _updateGrid = true;
                    }
                    Update();
                    _refresh.Invoke();   
                }
            };

            _controls.Add(textBox);
            _parameters.Add(name, value);
        }

        private void Update()
        {
            var f = GetFunction();
            var algorithm = GetAlgorithm();
            var epsilon = GetEpsilon();
            var radius = GetRadius();
            var startVector = GetStartVector();

            var result = algorithm.Invoke(f, startVector, epsilon);
            
            CreateFunctionContoursBitmap(f, result.Levels, result.X[0], result.X[1], radius);
        }

        private Function GetFunction()
        {
            return new(2, new []
            {
                GetParameter("a11"), GetParameter("a12"),
                GetParameter("a21"), GetParameter("a22")
            }, new [] {GetParameter("b1"), GetParameter("b2")}, GetParameter("c"));
        }

        private double[] GetStartVector()
        {
            return new[] { GetParameter("s1"), GetParameter("s2") };
        }

        private double GetParameter(string name)
        {
            // TODO check
            return _parameters[name];
        }

        private Algorithm GetAlgorithm()
        {
            return Optimization.GRADIENT_DESCENT;
        }

        private double GetEpsilon()
        {
            return GetParameter("epsilon");
        }

        private double GetRadius()
        {
            return GetParameter("r");
        }

        private void CreateFunctionContoursBitmap(Function f, IList<double[]> values, double x, double y, double r)
        {
            var gradientColors = GetColorsGradient(_maximalGradientColor, _minimalGradientColor, values.Count);
            var minX = x - r; 
            var maxX = x + r;
            var minY = y - r * Ratio;
            var maxY = y + r * Ratio;

            if (_updateGrid)
            {
                _grid = GenerateGrid(f, minX, maxX, minY, maxY);
                _updateGrid = false;
            }

            _bitmap = new Bitmap(Width, Height);

            for (var i = 0; i < values.Count; i++)
            {
                GenerateBitmap(f.Apply(values[i]), _grid, gradientColors[i], _bitmap);
            }
            
            _vectors = new List<double[]>();
            values.Add(new [] {x, y});
            
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
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    if (!(Math.Abs(grid[i][j] - value) <= epsilon)) continue;
                    
                    bitmap.SetPixel(i, j, color);
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