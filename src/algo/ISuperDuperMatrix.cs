using System;
using System.Runtime.CompilerServices;

namespace MultiDimensionalOptimization.algo
{
    public interface ISuperDuperMatrix
    {
        double[,] ToArray();

        double[] ToVector();

        ISuperDuperMatrix Transpose();

        ISuperDuperMatrix Multiply(ISuperDuperMatrix other);
        
        ISuperDuperMatrix Multiply(double a);
        
        ISuperDuperMatrix Subtract(ISuperDuperMatrix other);
        
        ISuperDuperMatrix Add(ISuperDuperMatrix other);

        double Get(int i, int j);

        static ISuperDuperMatrix Create(Type type, int rows, int columns, double[] values)
        {
            if (type == typeof(LibMatrix))
            {
                return new LibMatrix(rows, columns, values);
            }

            throw new NotSupportedException("Unsupported Matrix Type");
        }
    }
}