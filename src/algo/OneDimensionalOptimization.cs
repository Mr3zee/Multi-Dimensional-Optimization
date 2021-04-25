using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MultiDimensionalOptimization.algo
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
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
}