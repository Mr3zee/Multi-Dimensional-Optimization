using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MultiDimensionalOptimization.algo
{
    public class Vector : ISuperDuperMatrix
    {
        public Matrix<double> MatrixVector { get; }

        public Vector(double[] vector)
        {
            MatrixVector = new DenseMatrix(vector.Length, 1, vector);
        }

        private Vector(Matrix<double> other)
        {
            MatrixVector = other;
        }

        public double[] ToVector()
        {
            return MatrixVector.ToColumnArrays()[0];
        }

        public ISuperDuperMatrix Transpose()
        {
            return new LibMatrix(1, MatrixVector.RowCount, ToVector());
        }

        public ISuperDuperMatrix Multiply(ISuperDuperMatrix other)
        {
            if (other is LibMatrix libMatrix)
            {
                return new LibMatrix(MatrixVector.Multiply(libMatrix.Matrix));
            }

            throw new NotSupportedException("Cannot multiply");
        }

        public ISuperDuperMatrix Multiply(double a)
        {
            return new Vector(MatrixVector.Multiply(a));
        }

        public ISuperDuperMatrix Subtract(ISuperDuperMatrix other)
        {
            return new Vector(MatrixVector.Subtract(Cast(other).MatrixVector));
        }

        public ISuperDuperMatrix Add(ISuperDuperMatrix other)
        {
            return new Vector(MatrixVector.Add(Cast(other).MatrixVector));
        }

        public double Get(int i, int j)
        {
            if (j != 0)
            {
                throw new ArgumentException("Cannot take value not from first column in vector");
            }

            return MatrixVector[i, j];
        }

        public double GetMaxEigenValue()
        {
            throw new NotSupportedException("No Eigen Values available");
        }

        public ISuperDuperMatrix ComputeA()
        {
            throw new NotSupportedException("Cannot compute A for the Vector");
        }

        private static Vector Cast(ISuperDuperMatrix other)
        {
            if (other is not Vector vector)
            {
                throw new NotSupportedException("Other should be the Vector too");
            }

            return vector;
        } 

    }
}