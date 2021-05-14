using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace MultiDimensionalOptimization.algo
{
    public class DiagonalMatrix : ISuperDuperMatrix
    {
        private readonly double[] _diagonal;

        public DiagonalMatrix(double[] diagonal)
        {
            _diagonal = diagonal;
        }

        public double[] ToVector()
        {
            return _diagonal;
        }

        public ISuperDuperMatrix Transpose()
        {
            return new DiagonalMatrix(_diagonal);
        }

        public ISuperDuperMatrix Multiply(ISuperDuperMatrix other)
        {
            var isVector = other is Vector;
            var multiplied = isVector ? ((Vector) other).ToVector() : Cast(other)._diagonal;
            for (var i = 0; i < _diagonal.Length; i++)
            {
                multiplied[i] *= _diagonal[i];
            }
            return isVector ? new Vector(multiplied) : new DiagonalMatrix(multiplied);
        }

        public ISuperDuperMatrix Multiply(double a)
        {
            var multiplied = _diagonal;
            for (var i = 0; i < _diagonal.Length; i++)
            {
                multiplied[i] *= a;
            }
            return new DiagonalMatrix(multiplied);
        }

        public ISuperDuperMatrix Subtract(ISuperDuperMatrix other)
        {
            var subtracted = Cast(other)._diagonal;
            for (var i = 0; i < _diagonal.Length; i++)
            {
                subtracted[i] = _diagonal[i] - subtracted[i];
            }
            return new DiagonalMatrix(subtracted);
        }

        public ISuperDuperMatrix Add(ISuperDuperMatrix other)
        {
            var added = Cast(other)._diagonal;
            for (var i = 0; i < _diagonal.Length; i++)
            {
                added[i] += _diagonal[i];
            }
            return new DiagonalMatrix(added);
        }

        public double Get(int i, int j)
        {
            return i == j ? _diagonal[i] : 0;
        }

        public double GetMaxEigenValue()
        {
            return _diagonal.Max();
        }

        public ISuperDuperMatrix ComputeA()
        {
            return new DiagonalMatrix(_diagonal);
        }

        private static DiagonalMatrix Cast(ISuperDuperMatrix other)
        {
            if (other is not DiagonalMatrix diagonalMatrix)
            {
                throw new NotSupportedException("Other should be Diagonal too");
            }

            return diagonalMatrix;
        }
    }
}