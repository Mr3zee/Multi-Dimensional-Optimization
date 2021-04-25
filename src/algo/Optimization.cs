using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using MathNet.Numerics.LinearAlgebra;

namespace MultiDimensionalOptimization.algo
{
    using AlgorithmParameters = Dictionary<string, object>;

    public class Result
    {
        public double[] X { get; set; }

        public double Y { get; set; }

        public double Itr { get; set; }

        public List<double[]> Levels { get; } = new();

        public void AddLevel(double[] level)
        {
            Levels.Add(level);
        }
    }

    public delegate Result Algorithm(Function f, double[] xArray, double epsilon,
        AlgorithmParameters parameters = null);

    public delegate double
        InnerOptimizationAlgorithm(Func<double, double> f, double left, double right, double epsilon);

    /**
     * double diagonal elements and normal other
     */
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Optimization
    {
        private const double MAX_ITR = 10000;
        public const string InnerAlgorithm = "Inner Algorithm";

        private delegate Matrix<double> InnerAlgo(Function f, Matrix<double> x, double epsilon,
            AlgorithmParameters parameters, Result result);

        private static Algorithm UnwrapAlgorithm(InnerAlgo algo)
        {
            return (f, xArray, epsilon, parameters) =>
            {
                var n = xArray.Length;
                var result = new Result();
                var x = algo.Invoke(f, AdvancedMath.ToVector(n, xArray), epsilon, parameters, result);
                if (needToLog)
                {
                    Console.WriteLine();
                }

                result.X = AdvancedMath.ToArray(x);
                result.Y = f.Apply(x);
                return result;
            };
        }

        public static readonly Algorithm GRADIENT_DESCENT = UnwrapAlgorithm((f, x, epsilon, parameters, result) =>
        {
            var itr = 0;

            var alpha = 2 / GetMaxEigenValue(f);

            var fx = f.Apply(x);
            Matrix<double> grad;
            while (AdvancedMath.Norm(AdvancedMath.ToArray(grad = f.Gradient(x))) > epsilon && itr < MAX_ITR)
            {
                result.AddLevel(AdvancedMath.ToArray(x));
                itr++;
                LogItr(itr);
                var y = x.Subtract(grad.Multiply(alpha));
                var fy = f.Apply(y);
                if (fy < fx)
                {
                    fx = fy;
                    x = y;
                }
                else
                {
                    alpha /= 2;
                }
            }

            result.Itr = itr;

            return x;
        });

        public static readonly Algorithm FASTEST_DESCENT = UnwrapAlgorithm((f, x, epsilon, parameters, result) =>
        {
            var itr = 0;
            var maxEigenValue = GetMaxEigenValue(f);

            Matrix<double> grad;
            var oneDAlgo = parameters is not null
                ? (InnerOptimizationAlgorithm) parameters[InnerAlgorithm]
                : OneDimensionalOptimization.GOLDEN_SECTION;

            while (AdvancedMath.Norm(AdvancedMath.ToArray(grad = f.Gradient(x))) > epsilon && itr < MAX_ITR)
            {
                result.AddLevel(AdvancedMath.ToArray(x));
                itr++;
                LogItr(itr);
                // TODO what is love?
                var x1 = x;
                var grad1 = grad;

                var alpha = oneDAlgo.Invoke(
                    arg => f.Apply(x1.Subtract(grad1.Multiply(arg))),
                    0,
                    2 / maxEigenValue,
                    epsilon
                );

                result.Itr = itr;

                x = x.Subtract(grad.Multiply(alpha));
            }

            return x;
        });

        public static readonly Algorithm CONJUGATE_GRADIENT = UnwrapAlgorithm((f, x, epsilon, parameters, result) =>
        {
            var grad = f.Gradient(x);
            var norm = AdvancedMath.Norm(AdvancedMath.ToArray(grad));
            var p = grad.Multiply(-1);
            var itr = 0;

            while (norm > epsilon && itr < MAX_ITR)
            {
                result.AddLevel(AdvancedMath.ToArray(x));
                itr++;
                LogItr(itr);
                var mult = f.A.Multiply(p);
                var alpha = norm * norm / AdvancedMath.Scalar(mult, p);
                x = x.Add(p.Multiply(alpha));
                grad = grad.Add(mult.Multiply(alpha));
                var newNorm = AdvancedMath.Norm(AdvancedMath.ToArray(grad));
                var beta = newNorm * newNorm / (norm * norm);
                norm = newNorm;
                p = grad.Multiply(-1).Add(p.Multiply(beta));
            }

            result.Itr = itr;

            return x;
        });

        private static double GetMaxEigenValue(Function f)
        {
            alglib.smatrixevd(f.GetMatrix(), f.N, 0, true, out var eigenValues, out _);
            return eigenValues.Max();
        }

        public static class OneDimensionalOptimization
        {
            private static bool CheckBounds(double left, double right, double epsilon) {
                return Math.Abs(left - right) >= epsilon;
            }
            
            public static readonly InnerOptimizationAlgorithm GOLDEN_SECTION = (f, left, right, epsilon) =>
            {
                var delta = (right - left) * ReversedGoldenConst;

                var x2 = left + delta;
                var x1 = right - delta;

                var f2 = f.Invoke(x2);
                var f1 = f.Invoke(x1);

                do
                {
                    delta = ReversedGoldenConst * delta;
                    if (f1 >= f2)
                    {
                        left = x1;
                        x1 = x2;
                        x2 = left + delta;
                        f1 = f2;
                        f2 = f.Invoke(x2);
                    }
                    else
                    {
                        right = x2;
                        x2 = x1;
                        x1 = right - delta;
                        f2 = f1;
                        f1 = f.Invoke(x1);
                    }
                } while (delta > epsilon);

                return GetMiddle(left, right);
            };

            public static readonly InnerOptimizationAlgorithm DICHOTOMY = (f, left, right, epsilon) =>
            {
                double x;
                do
                {
                    x = GetMiddle(left, right);
                    var f1 = f.Invoke(x - epsilon / 2);
                    var f2 = f.Invoke(x + epsilon / 2);
                    if (f1 < f2)
                    {
                        right = x;
                    }
                    else
                    {
                        left = x;
                    }
                } while (CheckBounds(left, right, epsilon));

                return x;
            };

            private static readonly double ReversedGoldenConst = (Math.Sqrt(5) - 1) / 2;

            private static double GetMiddle(double a, double b)
            {
                return (a - b) / 2 + b;
            }

            public static readonly InnerOptimizationAlgorithm FIBONACCI = (f, left, right, epsilon) =>
            {
                var n = calculateFibonacciConst(left, right, epsilon);
                var k = 0;
                var lambda = getFibonacciVar(left, right, n, 2, 0);
                var mu = getFibonacciVar(left, right, n, 1, 0);
                double f_mu = f.Invoke(mu), f_lambda = f.Invoke(lambda);

                double an, bn;
                while (true)
                {
                    k++;
                    if (k == n - 2)
                    {
                        mu = lambda + epsilon;
                        if (f_mu >= f_lambda)
                        {
                            an = lambda;
                            bn = right;
                        }
                        else
                        {
                            an = left;
                            bn = mu;
                        }

                        break;
                    }

                    if (f_lambda > f_mu)
                    {
                        left = lambda;
                        lambda = mu;
                        f_lambda = f_mu;
                        mu = getFibonacciVar(left, right, n, k + 1, k);
                        f_mu = f.Invoke(mu);
                    }
                    else
                    {
                        right = mu;
                        mu = lambda;
                        f_mu = f_lambda;
                        lambda = getFibonacciVar(left, right, n, k + 2, k);
                        f_lambda = f.Invoke(lambda);
                    }
                }

                return GetMiddle(an, bn);
            };

            private static int calculateFibonacciConst(double left, double right, double epsilon)
            {
                return Math.Min(1475, Math.Abs(FIBONACCI_NUMBERS.BinarySearch((right - left) / epsilon)) + 1);
            }

            private static double getFibonacciVar(double a, double b, int n, int i, int j)
            {
                return a + FIBONACCI_NUMBERS[n - i] / FIBONACCI_NUMBERS[n - j] * (b - a);
            }

            private static readonly List<double> FIBONACCI_NUMBERS = getNFibonacci();

            private static List<double> getNFibonacci()
            {
                var arr = new List<double>(1476) {1.0, 1.0};
                for (var i = 2; i < 1476; i++)
                {
                    arr.Add(arr[i - 1] + arr[i - 2]);
                }

                return arr;
            }

            public static readonly InnerOptimizationAlgorithm PARABOLIC = (f, a, c, epsilon) =>
            {
                var b = GetMiddle(a, c);
                double fa = f.Invoke(a), fb = f.Invoke(b), fc = f.Invoke(c);
                while (CheckBounds(a, c, epsilon))
                {
                    var x = ParabolicMinimum(a, b, c, fa, fb, fc);
                    var fx = f.Invoke(x);
                    if (fx < fb)
                    {
                        if (x < b)
                        {
                            c = b;
                            fc = fb;
                        }
                        else
                        {
                            a = b;
                            fa = fb;
                        }

                        b = x;
                        fb = fx;
                    }
                    else
                    {
                        if (x < b)
                        {
                            a = x;
                            fa = fx;
                        }
                        else
                        {
                            c = x;
                            fc = fx;
                        }
                    }
                }

                return b;
            };

            private static double ParabolicMinimum(double a, double b, double c, double fa, double fb, double fc)
            {
                return b + 0.5 * ((fa - fb) * (c - b) * (c - b) - (fc - fb) * (b - a) * (b - a))
                    / ((fa - fb) * (c - b) + (fc - fb) * (b - a));
            }

            public static readonly InnerOptimizationAlgorithm BRENT = (f, a, c, epsilon) =>
            {
                double x, w, v, d, e, g, u, fx, fw, fv;
                x = w = v = a + ReversedGoldenConst * (c - a);
                fx = fw = fv = f.Invoke(x);
                d = e = c - a;
                while (CheckBounds(a, c, epsilon))
                {
                    g = e;
                    e = d;
                    if (Different(w, x, v) && Different(fw, fx, fv)
                                           && (u = ParabolicMinimum(w, x, v, fw, fx, fv)) == u
                                           && a <= u && u <= c && Math.Abs(u - x) < (g / 2))
                    {
                        // u - accepted
                    }
                    else
                    {
                        // u - rejected, u - golden section
                        if (x < GetMiddle(a, c))
                        {
                            e = c - x;
                            u = x + ReversedGoldenConst * e;
                        }
                        else
                        {
                            e = x - a;
                            u = x - ReversedGoldenConst * e;
                        }
                    }

                    double fu = f.Invoke(u);
                    if (fu <= fx)
                    {
                        if (u >= x)
                        {
                            a = x;
                        }
                        else
                        {
                            c = x;
                        }

                        v = w;
                        w = x;
                        x = u;
                        fv = fw;
                        fw = fx;
                        fx = fu;
                    }
                    else
                    {
                        if (u >= x)
                        {
                            c = u;
                        }
                        else
                        {
                            a = u;
                        }

                        if (fu <= fw || w == x)
                        {
                            v = w;
                            w = u;
                            fv = fw;
                            fw = fu;
                        }
                        else if (fu <= fv || v == x || v == w)
                        {
                            v = u;
                            fv = fu;
                        }
                    }

                    d = c - a;
                }

                return x;
            };
            
            private static bool Different(double a, double b, double c) {
                return a != b && b != c && c != a;
            }
        }

        private static int _lastSize = 0;

        public static bool needToLog = false;

        private static void LogItr(int itr)
        {
            if (!needToLog) return;

            Console.Out.Write(new string('\b', _lastSize));
            var newStr = $"Iterations: {itr}";
            Console.Out.Write(newStr);
            _lastSize = newStr.Length;
        }
    }
}