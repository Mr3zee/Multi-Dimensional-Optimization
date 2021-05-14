using System;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MultiDimensionalOptimization.algo
{
    public class LibMatrix : ISuperDuperMatrix
    {
        public Matrix<double> Matrix { get; }
        
        public int N { get; }

        public LibMatrix(int rows, int columns, double[] values)
        {
            N = rows;
            Matrix = new DenseMatrix(rows, columns, values);
        }

        public LibMatrix(Matrix<double> matrix)
        {
            N = matrix.RowCount;
            Matrix = matrix;
        }

        public double[,] ToArray()
        {
            return Matrix.ToArray();
        }

        public double[] ToVector()
        {
            return Matrix.ToColumnArrays()[0];
        }

        public ISuperDuperMatrix Transpose()
        {
            return new LibMatrix(Matrix.Transpose());
        }

        public ISuperDuperMatrix Multiply(ISuperDuperMatrix other)
        {
            switch (other)
            {
                case Vector vector:
                    return new Vector(Matrix.Multiply(vector.MatrixVector).ToColumnArrays()[0]);
                case DiagonalMatrix diagonalMatrix:
                {
                    var vectorT = Matrix.ToRowArrays()[0];
                    for (var i = 0; i < vectorT.Length; i++)
                    {
                        vectorT[i] *= diagonalMatrix.Get(i, i);
                    }

                    return new LibMatrix(1, vectorT.Length, vectorT);
                }
                default:
                    return new LibMatrix(Matrix.Multiply(Cast(other).Matrix));
            }
        }

        public ISuperDuperMatrix Multiply(double a)
        {
            return new LibMatrix(Matrix.Multiply(a));
        }

        public ISuperDuperMatrix Subtract(ISuperDuperMatrix other)
        {
            return new LibMatrix(Matrix.Subtract(Cast(other).Matrix));
        }

        public ISuperDuperMatrix Add(ISuperDuperMatrix other)
        {
            return new LibMatrix(Matrix.Add(Cast(other).Matrix));
        }

        public double Get(int i, int j)
        {
            return Matrix[i, j];
        }
        
        public double GetMaxEigenValue()
        {
            alglib.smatrixevd(ToArray(), N, 0, true, out var eigenValues, out _);
            return eigenValues.Max();
        }
        
        public ISuperDuperMatrix ComputeA()
        {
            var vA = new double[N * N];
            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < N; j++)
                {
                    vA[i * N + j] = 0.5 * (Get(i, j) + Get(j, i));
                }
            }

            return ISuperDuperMatrix.Create(ISuperDuperMatrix.LIB_MATRIX, N, N, vA);
        }
        
        private static LibMatrix Cast(ISuperDuperMatrix other)
        {
            if (other is not LibMatrix libMatrix)
            {
                throw new NotSupportedException("Other should be The Lib Matrix too");
            }

            return libMatrix;
        }

    }
}