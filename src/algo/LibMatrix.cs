using System;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MultiDimensionalOptimization.algo
{
    public class LibMatrix : ISuperDuperMatrix
    {
        private Matrix<double> Matrix { get; set; }

        public LibMatrix(int rows, int columns, double[] values)
        {
            Matrix = new DenseMatrix(rows, columns, values);
        }

        private LibMatrix(Matrix<double> matrix)
        {
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
            return new LibMatrix(Matrix.Multiply(Cast(other).Matrix));
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
        private static LibMatrix Cast(ISuperDuperMatrix other)
        {
            if (other is not LibMatrix libMatrix)
            {
                throw new NotSupportedException("Other should be same type");
            }
            
            return libMatrix;
        }
    }
}