using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualBasic;

namespace MultyDimentionalOptimization.algo
{
    /**
     * double diagonal elements and normal other
     */
    public class Optimization
    {
        private const double MAX_ITR = 10000;

        public static double GRADIENT_DESCENT(Function f, double[] xArray, double epsilon)
        {
            var itr = 0;
            var n = xArray.Length;
            alglib.smatrixevd(f.GetMatrix(), n, 0, true, out var eigenValues, out _);

            double alpha = 2 / eigenValues.Max();

            var x = AdvancedMath.ToVector(n, xArray);
            var fx = f.Apply(x);
            Matrix<double> grad;
            while (AdvancedMath.Norm(AdvancedMath.ToArray(grad = f.Gradient(x))) > epsilon && itr < MAX_ITR)
            {
                itr++;
                logItr(itr);
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

            Console.Out.WriteLine("\n" + x);
            Console.Out.WriteLine("itr: " + itr);
            // TODO return x
            return fx;
        }

        private static int _lastSize = 0;

        private static void logItr(int itr)
        {
            Console.Out.Write(new string('\b', _lastSize));
            Console.Out.Write(itr);
            _lastSize = itr.ToString().Length;
        }

        public static double FASTEST_DESCENT(Function f, double[] xArray, double epsilon)
        {
            var itr = 0;
            var n = xArray.Length;
            alglib.smatrixevd(f.GetMatrix(), n, 0, true, out var eigenValues, out _);

            var x = AdvancedMath.ToVector(n, xArray);
            Matrix<double> grad;
            while (AdvancedMath.Norm(AdvancedMath.ToArray(grad = f.Gradient(x))) > epsilon && itr < MAX_ITR)
            {
                itr++;
                logItr(itr);
                // TODO what is love?
                var alpha = OneDimensionalOptimization.GOLDEN_SECTION(
                    arg => f.Apply(x.Subtract(grad.Multiply(arg))),
                    0,
                    2 / eigenValues.Max(),
                    epsilon
                );

                x = x.Subtract(grad.Multiply(alpha));
            }

            Console.Out.WriteLine("\n" + x);
            Console.Out.WriteLine("itr: " + itr);
            // TODO return x
            return f.Apply(x);
        }

        public static double CONJUGATE_GRADIENT(Function f, double[] xArray, double epsilon)
        {
            var n = xArray.Length;
            var x = AdvancedMath.ToVector(n, xArray);
            var grad = f.Gradient(x);
            double norm = AdvancedMath.Norm(AdvancedMath.ToArray(grad));
            var p = grad.Multiply(-1);
            var itr = 0;
            while (norm > epsilon && itr < MAX_ITR)
            {
                itr++;
                logItr(itr);
                var mult = f.A.Multiply(p);
                var alpha = norm * norm / AdvancedMath.Scalar(mult, p);
                x = x.Add(p.Multiply(alpha));
                grad = grad.Add(mult.Multiply(alpha));
                var newNorm = AdvancedMath.Norm(AdvancedMath.ToArray(grad));
                var beta = newNorm * newNorm / (norm * norm);
                norm = newNorm;
                p = grad.Multiply(-1).Add(p.Multiply(beta));
            }

            Console.Out.WriteLine("\n" + x);
            Console.Out.WriteLine("itr: " + itr);
            return f.Apply(x);
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
    }
}