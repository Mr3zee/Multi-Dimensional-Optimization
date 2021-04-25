using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MultiDimensionalOptimization.algo
{
    public static class AdvancedMath
    {
        private static readonly Random Random = new(); 
        public static double Norm(double[] v)
        {
            return Math.Sqrt(Enumerable.Range(0, v.Length).Aggregate(0.0d, 
                (acc, i) => acc + v[i] * v[i])
            );
        }
        
        public static Matrix<double> ToVector(int n, double[] v)
        {
            return new DenseMatrix(n, 1, v);
        }

        public static Matrix<double> ToMatrix(int n, double[] m)
        {
            return new DenseMatrix(n, n, m);
        }
        
        public static double[] ToArray(Matrix<double> v)
        {
            return v.ToColumnArrays()[0];
        }

        public static double Scalar(Matrix<double> a, Matrix<double> b)
        {
            return ToArray(a).Zip(ToArray(b), (i, j) => i * j).Sum();
        }

        public static double[] GetDiagonalMatrix(int n, int k)
        {
            var m = Enumerable.Repeat(0.0, n * n).ToArray();
            for (var i = 0; i < n; i++)
            {
                m[i * (n + 1)] = Random.Next(1, k);
            }
            m[0] = 1;
            m[n * n - 1] = k;
            return m;
        }

        public static Function CreateDiagonalFunction(int n, int k)
        {
            return new Function(n, GetDiagonalMatrix(n, k), Enumerable.Repeat(0.0, n).ToArray(), 0);
        }

        private const double UpperRandomBound = 10;
        private const double LowerRandomBound = 1;

        public static double[] GenerateRandomStartVector(int n)
        {
            var retval = new double[n];
            for (var i = 0; i < n; i++)
            {
                retval[i] = Random.NextDouble() * (UpperRandomBound - LowerRandomBound) + LowerRandomBound;
            }

            return retval;
        }
        
        private const double DoublePrecision = 1E-08;

        public static bool Equals(double a, double b, double precision = DoublePrecision)
        {
            return Math.Abs(a - b) < a * precision;
        }
    }
}