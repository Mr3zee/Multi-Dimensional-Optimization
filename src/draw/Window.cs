using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using MultiDimensionalOptimization.algo;
using static System.Double;

namespace MultiDimensionalOptimization.draw
{
    using AlgorithmParameters = Dictionary<string, object>;
    using Grid = Dictionary<KeyValuePair<int, int>, double>;
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Window
    {
        public const int ScreenWidth = 1280;
        public const int ScreenHeight = 720;
        
        private const double Ratio = (double) ScreenHeight / ScreenWidth;
        private readonly Action _refresh;
        private readonly Control.ControlCollection _controls;
        private Bitmap _bitmap;
        private List<int[]> _vectors;
        private readonly Pen _gridPen = new (Color.LightGray);
        private readonly Brush _axesBrush = new SolidBrush(Color.Black);
        private readonly Pen _boldGridPen = new (Color.Gray, 1) {EndCap = LineCap.ArrowAnchor};
        private double[][] _grid;
        private bool _updateGrid;
        private readonly Pen _vectorsPen = new (Color.Black, 1) {EndCap = LineCap.ArrowAnchor};
        private readonly Color _minimalGradientColor = Color.Coral;
        private readonly Color _maximalGradientColor = Color.Green;
        private readonly Dictionary<string, double> _parameters = new ();
        private readonly Dictionary<string, object> _algorithmParameters = new ();
        private readonly Dictionary<string, TextBox> _results = new ();
        private readonly Dictionary<string, bool> _superGUIFlags = new ();
        private Algorithm _currentAlgorithm;

        private double minX;
        private double maxX;
        private double minY;
        private double maxY;

        private const string A11 = "a11";
        private const string A12 = "a12";
        private const string A21 = "a21";
        private const string A22 = "a22";
        private const string B1 = "b1";
        private const string B2 = "b2";
        private const string C = "c";
        private const string Epsilon = "Epsilon";
        private const string Radius = "Radius";
        private const string XStart = "xStart";
        private const string YStart = "yStart";
        private const string Precision = "Precision";
        
        private const string XResult = "X Result";
        private const string YResult = "Y Result";
        private const string Minimum = "Minimum";
        private const string Iterations = "Iterations";
        private const string FastestDescentInnerAlgorithm = Optimization.InnerAlgorithm;

        private const string ContoursSuperGUIButton = "Contours";
        private const string ArrowsSuperGUIButton = "Arrows";
        private const string AxesSuperGUIButton = "Axes";
        private const string AxesNamesSuperGUIButton = "Axes Names";

        public Window(Action refresh, Control.ControlCollection controls)
        {
            _updateGrid = true;
            _refresh = refresh;
            _controls = controls;
            
            CreateParametersInputs();
            var innerAlgoGroup = CreateInnerAlgoGroup();
            CreateAlgoGroup(innerAlgoGroup);
            CreateResultsOutputs();
            CreateSuperDuperGUI();

            Update();
        }

        private void CreateResultsOutputs()
        {
            AddResultBox(XResult, 10);
            AddResultBox(YResult, 40);
            AddResultBox(Minimum, 70);
            AddResultBox(Iterations, 100);
        }

        private void AddResultBox(string name, int offsetY)
        {
            var resultBox = new TextBox
            {
                Multiline = false,
                ReadOnly = true,
                PlaceholderText = "No result",
                Top = offsetY,
                Height = 20,
                Width = 140,
                BackColor = Color.White,
                Anchor = AnchorStyles.Right & AnchorStyles.Top
            };
            resultBox.Left = ScreenWidth - resultBox.Width - 30;
            
            var resultLabel = new Label
            {
                Top = offsetY,
                Text = name,
                Height = 23,
                Width = 80,
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Anchor = AnchorStyles.Left & AnchorStyles.Top,
            };
            resultLabel.Left = resultBox.Left - resultLabel.Width - 10;

            _results[name] = resultBox;
            
            _controls.Add(resultLabel);
            _controls.Add(resultBox);
        }

        private void CreateParametersInputs()
        {
            AddTextBox(A11, 20, 10, true);
            AddTextBox(A12, 0, 40, true);
            AddTextBox(A21, 1, 70, true);
            AddTextBox(A22, 1, 100, true);

            AddTextBox(B1, 0, 130, true);
            AddTextBox(B2, 0, 160, true);

            AddTextBox(C, 0, 190, true);

            AddTextBox(Radius, 30, 230, true);
            AddTextBox(Epsilon, 0.00001, 260, false);

            AddTextBox(XStart, 10, 300, false);
            AddTextBox(YStart, 10, 330, false);
            
            AddTextBox(Precision, 0.008, 380, true);
        }

        private const double InputChangePrecision = 0.00001;

        private void AddTextBox(string name, double value, int offsetY, bool updateGrid)
        {
            var label = new Label
            {
                Text = name,
                Left = 10,
                Top = offsetY,
                Height = 23,
                Width = 65,
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Anchor = AnchorStyles.Left & AnchorStyles.Top
            };

            var textBox = new TextBox
            {
                Multiline = false,
                AcceptsReturn = false,
                PlaceholderText = name,
                AcceptsTab = false,
                Text = value.ToString(CultureInfo.InvariantCulture),
                Top = offsetY,
                Left = 20 + label.Width,
                Height = 20,
                Anchor = AnchorStyles.Left & AnchorStyles.Top,
            };
            textBox.LostFocus += (_, _) =>
            {
                var oldV = _parameters[textBox.PlaceholderText];
                if (!TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var newV))
                {
                    textBox.BackColor = Color.OrangeRed;
                    return;
                }
                textBox.BackColor = Color.White;
                
                if (Math.Abs(oldV - newV) > newV * InputChangePrecision)
                {
                    _parameters[textBox.PlaceholderText] = newV;
                    if (updateGrid)
                    {
                        _updateGrid = true;
                    }
                    Reload();
                }
            };
            _parameters.Add(name, value);
            
            _controls.Add(label);
            _controls.Add(textBox);
        }

        private void Reload()
        {
            Update();
            _refresh.Invoke();
        }
        
        private GroupBox CreateInnerAlgoGroup()
        {
            var innerAlgoGroup = new GroupBox
            {
                Text = FastestDescentInnerAlgorithm,
                Left = 10,
                Top = 520,
                Height = 145,
                BackColor = Color.White,
            };
            AddInnerAlgoButton("Dichotomy", false, OneDimensionalOptimization.DICHOTOMY, innerAlgoGroup, 15);
            AddInnerAlgoButton("Golden Section Search", true, OneDimensionalOptimization.GOLDEN_SECTION, innerAlgoGroup, 40);
            AddInnerAlgoButton("Fibonacci Search", false, OneDimensionalOptimization.FIBONACCI, innerAlgoGroup, 65);
            AddInnerAlgoButton("Parabolic Search", false, OneDimensionalOptimization.PARABOLIC, innerAlgoGroup, 90);
            AddInnerAlgoButton("Brent Algorithm", false, OneDimensionalOptimization.BRENT, innerAlgoGroup, 115);
            
            _controls.Add(innerAlgoGroup);
            return innerAlgoGroup;
        }

        private void CreateAlgoGroup(GroupBox innerAlgorithms)
        {
            var algoGroup = new GroupBox
            {
                Text = "Algorithms",
                Left = 10,
                Top = 410,
                BackColor = Color.White,
            };
            AddAlgoButton("Gradient Descent", false, Optimization.GRADIENT_DESCENT, algoGroup, 15, false, innerAlgorithms);
            AddAlgoButton("Fastest Descent", true, Optimization.FASTEST_DESCENT, algoGroup, 40, true, innerAlgorithms);
            AddAlgoButton("Conjugate Gradient", false, Optimization.CONJUGATE_GRADIENT, algoGroup, 65, false, innerAlgorithms);
           
            _controls.Add(algoGroup);
        }

        private void AddAlgoButton(string name, bool isChecked, Algorithm algorithm, GroupBox groupBox, int offset, bool withInner, GroupBox innerAlgorithms)
        {
            var button = new RadioButton
            {
                Text = name,
                Checked = isChecked,
                Top = offset,
                Left = 5
            };
            button.Width = (TextRenderer.MeasureText(button.Text, button.Font)).Width + 20;
            if (isChecked)
            {
                _currentAlgorithm = algorithm;
            }
            button.CheckedChanged += (_, _) =>
            {
                if (!button.Checked) return;

                innerAlgorithms.Enabled = withInner;
                _currentAlgorithm = algorithm;
                Reload();
            };
            groupBox.Controls.Add(button);
        }
        
        private void AddInnerAlgoButton(string name, bool isChecked, InnerOptimizationAlgorithm algorithm, GroupBox groupBox, int offset)
        {
            var button = new RadioButton
            {
                Text = name,
                Checked = isChecked,
                Top = offset,
                Left = 5
            };
            button.Width = (TextRenderer.MeasureText(button.Text, button.Font)).Width + 20;
            if (isChecked)
            {
                _algorithmParameters[FastestDescentInnerAlgorithm] = algorithm;
            }
            button.CheckedChanged += (_, _) =>
            {
                if (!button.Checked) return;
                
                _algorithmParameters[FastestDescentInnerAlgorithm] = algorithm;
                Reload();
            };
            groupBox.Controls.Add(button);
        }
        
        private void CreateSuperDuperGUI()
        {
            var SuperGUIBox = new GroupBox
            {
                Text = "Super Duper GUI Buttons",
                Top = 550,
                Width = 160,
                Height = 120,
                BackColor = Color.White
            };
            SuperGUIBox.Left = ScreenWidth - SuperGUIBox.Width - 30;
            AddSuperGUIElement(ContoursSuperGUIButton, true, SuperGUIBox, 15);
            AddSuperGUIElement(ArrowsSuperGUIButton, true, SuperGUIBox, 40);
            AddSuperGUIElement(AxesSuperGUIButton, true, SuperGUIBox, 65);
            AddSuperGUIElement(AxesNamesSuperGUIButton, true, SuperGUIBox, 90);
            _controls.Add(SuperGUIBox);
        }

        private void AddSuperGUIElement(string name, bool isChecked, GroupBox box, int offset)
        {
            var button = new CheckBox
            {
                Text = name,
                Checked = isChecked,
                Top = offset,
                Left = 5
            };
            button.CheckedChanged += (_, _) =>
            {
                _superGUIFlags[name] = button.Checked;
                Reload();
            };
            _superGUIFlags[name] = isChecked;
            box.Controls.Add(button);
        }

        private void Update()
        {
            var f = GetFunction();
            var algorithm = GetAlgorithm();
            var epsilon = GetEpsilon();
            var radius = GetRadius();
            var startVector = GetStartVector();
            var algorithmParameters = GetAlgorithmParameters();

            var result = algorithm.Invoke(f, startVector, epsilon, algorithmParameters);
            SetResult(result);

            CreateFunctionContoursBitmap(f, result.Levels, result.X[0], result.X[1], radius);
        }

        private void SetResult(Result result)
        {
            SetResult(XResult, result.X[0]);
            SetResult(YResult, result.X[1]);
            SetResult(Minimum, result.Y);
            SetResult(Iterations, result.Itr);
        }
        
        private void SetResult(string name, double value)
        {
            _results[name].Text = value.ToString(CultureInfo.InvariantCulture);
        }

        private Function GetFunction()
        {
            return new(ISuperDuperMatrix.LIB_MATRIX, 2, new []
            {
                GetParameter(A11), GetParameter(A12),
                GetParameter(A21), GetParameter(A22)
            }, new [] {GetParameter(B1), GetParameter(B2)}, GetParameter(C));
        }

        private double[] GetStartVector()
        {
            return new[] { GetParameter(XStart), GetParameter(YStart) };
        }

        private double GetParameter(string name)
        {
            return _parameters[name];
        }

        private Algorithm GetAlgorithm()
        {
            return _currentAlgorithm;
        }

        private double GetEpsilon()
        {
            return GetParameter(Epsilon);
        }

        private double GetRadius()
        {
            return GetParameter(Radius);
        }
        
        private double GetPrecision()
        {
            return GetParameter(Precision);
        }

        private AlgorithmParameters GetAlgorithmParameters()
        {
            return _algorithmParameters;
        }

        private void CreateFunctionContoursBitmap(Function f, IList<double[]> values, double x, double y, double r)
        {
            var gradientColors = GetColorsGradient(_maximalGradientColor, _minimalGradientColor, values.Count);
            minX = x - r; 
            maxX = x + r;
            minY = y - r * Ratio;
            maxY = y + r * Ratio;

            if (_updateGrid)
            {
                _grid = GenerateGrid(f, minX, maxX, minY, maxY);
                _updateGrid = false;
            }

            if (_superGUIFlags[ContoursSuperGUIButton])
            {
                CreateContours(f, values, gradientColors);
            }
            else
            {
                _bitmap = new Bitmap(ScreenWidth, ScreenHeight);
            }

            CreateVectorsPoints(values, x, y, r);
        }

        private void CreateContours(Function f, IList<double[]> values, IReadOnlyList<Color> gradientColors)
        {
            _bitmap = new Bitmap(ScreenWidth, ScreenHeight);
            var precision = GetPrecision() * Math.Pow(10, Math.Log10(maxY - minY) / 4);
            var valuesMap = new SortedDictionary<double, Color>(new DoubleComparer(precision));

            for (var i = 0; i < values.Count; i++)
            {
                valuesMap[f.Apply(values[i])] = gradientColors[i];
            }

            GenerateBitmap(valuesMap, _grid, _bitmap);
        }
        
        private class DoubleComparer : IComparer<double>
        {
            private readonly double _precision;

            public DoubleComparer(double precision)
            {
                _precision = precision;
            }

            public int Compare(double x, double y)
            {
                var diff = x - y;
                return IsNaN(diff) ? -1 : Math.Abs(diff) <= Math.Abs(x * _precision) ? 0 : Math.Sign(diff);
            }
        }

        private void CreateVectorsPoints(ICollection<double[]> values, double x, double y, double r)
        {
            _vectors = new List<int[]>();
            values.Add(new[] {x, y});

            foreach (var value in values)
            {
                var windowX = (value[0] - minX) / (2 * r) * ScreenWidth;
                var windowY = (value[1] - minY) / (2 * r * Ratio) * ScreenHeight;

                if (!InWindow(windowX) || !InWindow(windowY)) continue;

                _vectors.Add(new[] {(int) windowX, (int) windowY});
            }
        }

        private const int LinesCount = 35;

        public void Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics, LinesCount);
            e.Graphics.DrawImage(_bitmap, 0, 0);
            DrawVectors(e.Graphics);
            DrawAxesNames(e.Graphics);
        }

        private void DrawVectors(Graphics g)
        {
            _vectorsPen.EndCap = _superGUIFlags[ArrowsSuperGUIButton] ? LineCap.ArrowAnchor : LineCap.NoAnchor;
            for (var i = 0; i < _vectors.Count - 1; i++)
            {
                var x1 = _vectors[i][0];
                var y1 = _vectors[i][1];
                var x2 = _vectors[i + 1][0];
                var y2 = _vectors[i + 1][1];
                g.DrawLine(_vectorsPen, x1, y1, x2, y2);
            }
        }

        private static bool InWindow(double value)
        {
            return Math.Abs(value) <= 2 * ScreenWidth;
        }

        private void DrawGrid(Graphics g, int count)
        {
            const int centerX = ScreenWidth / 2;
            const int centerY = ScreenHeight / 2;
            var delta = ScreenWidth / count;

            for (var i = 1; i < (count + 1) / 2; i++)
            {
                g.DrawLine(_gridPen, centerX - i * delta, 0, centerX - i * delta, ScreenHeight);
                g.DrawLine(_gridPen, centerX + i * delta, 0, centerX + i * delta, ScreenHeight);
                g.DrawLine(_gridPen, 0, centerY - i * delta, ScreenWidth, centerY - i * delta);
                g.DrawLine(_gridPen, 0, centerY + i * delta, ScreenWidth, centerY + i * delta);
            }

            var axesPen = _superGUIFlags[AxesSuperGUIButton] ? _boldGridPen : _gridPen;
            g.DrawLine(axesPen, centerX, 0, centerX , ScreenHeight);
            g.DrawLine(axesPen, 0, centerY, ScreenWidth, centerY);

        }

        private void DrawAxesNames(Graphics g)
        {
            const int centerX = ScreenWidth / 2;
            const int centerY = ScreenHeight / 2;
            
            if (!_superGUIFlags[AxesSuperGUIButton] || !_superGUIFlags[AxesNamesSuperGUIButton]) return;

            var minXStr = minX.ToString(CultureInfo.InvariantCulture);
            var XAxis = $"X Axes, {maxX.ToString(CultureInfo.InvariantCulture)}";
            var minYStr = minY.ToString(CultureInfo.InvariantCulture);
            var YAxis = $"Y Axes, {maxY.ToString(CultureInfo.InvariantCulture)}";
            
            g.DrawString(minXStr, SystemFonts.DialogFont, _axesBrush, centerX, 0);
            g.DrawString(XAxis, SystemFonts.DialogFont, _axesBrush, centerX, ScreenHeight - 55);
            g.DrawString(minYStr, SystemFonts.DialogFont, _axesBrush, 0, centerY);
            g.DrawString(YAxis, SystemFonts.DialogFont, _axesBrush, ScreenWidth - YAxis.Length * 5 - 37, centerY);
        }

        private static void GenerateBitmap(IReadOnlyDictionary<double, Color> values, double[][] grid, Bitmap bitmap)
        {
            for (var i = 0; i < grid.Length; i++)
            {
                for (var j = 0; j < grid[i].Length; j++)
                {
                    var v = grid[i][j];
                    if (!values.ContainsKey(v)) continue;
                    
                    bitmap.SetPixel(i, j, values[v]);
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
            var grid = new double[ScreenWidth][];
            var stepX = (maxX - minX) / ScreenWidth;
            var stepY = (maxY - minY) / ScreenHeight;
            var vector = new double[2];
            for (var i = 0; i < ScreenWidth; i++)
            {
                grid[i] = new double[ScreenHeight];
                for (var j = 0; j < ScreenHeight; j++)
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
            int rMin = color1.R;
            int rMax = color2.R;
            int gMin = color1.G;
            int gMax = color2.G;
            int bMin = color1.B;
            int bMax = color2.B;
            var colorList = new List<Color>();
            for (var i = 0; i < size; i++)
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