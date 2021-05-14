using System;
using System.Runtime.CompilerServices;
// ReSharper disable InconsistentNaming

namespace MultiDimensionalOptimization.algo
{
    public interface ISuperDuperMatrix
    {
        static Type LIB_MATRIX { get; } = typeof(LibMatrix);
        static Type DIAGONAL_MATRIX { get; } = typeof(DiagonalMatrix);
        static Type VECTOR { get; } = typeof(Vector);

        double[] ToVector();

        ISuperDuperMatrix Transpose();

        ISuperDuperMatrix Multiply(ISuperDuperMatrix other);
        
        ISuperDuperMatrix Multiply(double a);
        
        ISuperDuperMatrix Subtract(ISuperDuperMatrix other);
        
        ISuperDuperMatrix Add(ISuperDuperMatrix other);

        double Get(int i, int j);

        public double GetMaxEigenValue();

        public ISuperDuperMatrix ComputeA();

        static ISuperDuperMatrix Create(Type type, int rows, int columns, double[] values)
        {
            if (type == LIB_MATRIX)
            {
                return new LibMatrix(rows, columns, values);
            }
            if (type == DIAGONAL_MATRIX)
            {
                return new DiagonalMatrix(values);
            }

            if (type == VECTOR)
            {
                return new Vector(values);
            }

            throw new NotSupportedException("Unsupported Matrix Type");
        }
    }
}