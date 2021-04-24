using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualBasic;

namespace MultiDimentionalOptimization.algo
{
    public class Result
    {
        private double[] x;
        private double y;
        private readonly List<double[]> levels;

        public double[] X
        {
            get => x;
            set => x = value;
        }

        public double Y
        {
            get => y;
            set => y = value;
        }

        public List<double[]> Levels => levels;

        public void AddLevel(double[] level)
        {
            levels.Add(level);
        }
    }

    public delegate Result Algorithm(Function f, double[] xArray, double epsilon);
    
    /**
     * double diagonal elements and normal other
     */
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Optimization
    {
        private const double MAX_ITR = 10000;

        private delegate Matrix<double> InnerAlgo(Function f, Matrix<double> x,  double epsilon, Result result);

        private static Algorithm UnwrapAlgorithm(InnerAlgo algo)
        {
            return (f, xArray, epsilon) =>
            {
                int n = xArray.Length;
                var result = new Result();
                var x = algo.Invoke(f, AdvancedMath.ToVector(n, xArray), epsilon, result);
                result.X = AdvancedMath.ToArray(x);
                result.Y = f.Apply(x);
                return result;
            };
        }

        public static readonly Algorithm GRADIENT_DESCENT = UnwrapAlgorithm((f, x, epsilon, result) =>
        {
            var itr = 0;
            
            double alpha = 2 / GetMaxEigenValue(f);

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

            return x;
        });

        public static readonly Algorithm FASTEST_DESCENT = UnwrapAlgorithm((f, x, epsilon, result) =>
        {
            var itr = 0;
            var maxEigenValue = GetMaxEigenValue(f);

            Matrix<double> grad;
            while (AdvancedMath.Norm(AdvancedMath.ToArray(grad = f.Gradient(x))) > epsilon && itr < MAX_ITR)
            {
                result.AddLevel(AdvancedMath.ToArray(x));
                itr++;
                LogItr(itr);
                // TODO what is love?
                var alpha = OneDimensionalOptimization.GOLDEN_SECTION(
                    arg => f.Apply(x.Subtract(grad.Multiply(arg))),
                    0,
                    2 / maxEigenValue,
                    epsilon
                );

                x = x.Subtract(grad.Multiply(alpha));
            }
            
            return x;
        });

        public static readonly Algorithm CONJUGATE_GRADIENT = UnwrapAlgorithm((f, x, epsilon, result) =>
        {
            var grad = f.Gradient(x);
            double norm = AdvancedMath.Norm(AdvancedMath.ToArray(grad));
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

            return x;
        });

        private static double GetMaxEigenValue(Function f)
        {
            alglib.smatrixevd(f.GetMatrix(), f.N, 0, true, out var eigenValues, out _);
            return eigenValues.Max();
        }

        private static class OneDimensionalOptimization
        {
            public static double GOLDEN_SECTION(Func<double, double> f, double left, double right, double epsilon)
            {
                double delta = (right - left) * ReversedGoldenConst;

                double x2 = left + delta;
                double x1 = right - delta;

                double f2 = f.Invoke(x2);
                double f1 = f.Invoke(x1);

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
            }

            public static double DICHOTOMY(Func<double, double> f, double left, double right, double epsilon)
            {
                double x;
                do
                {
                    x = GetMiddle(left, right);
                    double f1 = f.Invoke(x - epsilon / 2);
                    double f2 = f.Invoke(x + epsilon / 2);
                    if (f1 < f2)
                    {
                        right = x;
                    }
                    else
                    {
                        left = x;
                    }
                } while (right - left > epsilon);

                return x;
            }

            private static readonly double ReversedGoldenConst = (Math.Sqrt(5) - 1) / 2;

            private static double GetMiddle(double a, double b)
            {
                return (a - b) / 2 + b;
            }
        }
        
        private static int _lastSize = 0;

        public static bool needToLog = false;

        private static void LogItr(int itr)
        {
            if (!needToLog) return;
            
            Console.Out.Write(new string('\b', _lastSize));
            Console.Out.Write(itr);
            _lastSize = itr.ToString().Length;
        }
    }
}