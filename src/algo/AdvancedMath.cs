using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MultyDimentionalOptimization.algo
{
    public class AdvancedMath
    {
        public static double Norm(double[] v)
        {
            return System.Math.Sqrt(Enumerable.Range(0, v.Length).Aggregate(0.0d, 
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
                m[i * (n + 1)] = new Random().Next(1, k);
            }
            m[0] = 1;
            m[n * n - 1] = k;
            return m;
        }

        public static Function CreateDiagonalFunction(int n, int k)
        {
            return new Function(n, GetDiagonalMatrix(n, k), Enumerable.Repeat(0.0, n).ToArray(), 0);
        }
    }
}