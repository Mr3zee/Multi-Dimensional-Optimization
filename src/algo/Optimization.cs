using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                if (NeedToLog)
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

        private static int _lastSize = 0;

        private const bool NeedToLog = false;

        private static void LogItr(int itr)
        {
            if (!NeedToLog) return;

            Console.Out.Write(new string('\b', _lastSize));
            var newStr = $"Iterations: {itr}";
            Console.Out.Write(newStr);
            _lastSize = newStr.Length;
        }
    }
}