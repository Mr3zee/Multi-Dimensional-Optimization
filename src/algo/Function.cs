using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MultiDimensionalOptimization.algo
{
    public class Function
    {
        public int N { get; }

        private readonly ISuperDuperMatrix _a;
        public ISuperDuperMatrix A { get; }

        private readonly ISuperDuperMatrix _b;
        private readonly double _c;
        public Type Type { get; }

        public Function(Type type, int n, double[] a, double[] b, double c)
        {
            // TODO check dimensions 
            Type = type;
            N = n;
            _a = AdvancedMath.ToMatrix(type, n, a);
            A = ComputeA();
            _b = AdvancedMath.ToVector(type, n, b);
            _c = c;
        }

        public double[,] GetMatrix()
        {
            return A.ToArray();
        }

        public double Apply(double[] x)
        {
            return Apply(AdvancedMath.ToVector(Type, N, x));
        }
        
        public double Apply(ISuperDuperMatrix x)
        {
            return 0.5 * x.Transpose().Multiply(_a).Multiply(x).Get(0, 0) -
                _b.Transpose().Multiply(x).Get(0, 0) + _c;
        }

        public ISuperDuperMatrix Gradient(ISuperDuperMatrix x)
        {
            return A.Multiply(x).Subtract(_b);
        }
        
        private ISuperDuperMatrix ComputeA()
        {
            var vA = new double[N * N];
            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < N; j++)
                {
                    vA[i * N + j] = 0.5 * (_a.Get(i, j) + _a.Get(j, i));
                }
            }

            return ISuperDuperMatrix.Create(Type, N, N, vA);
        }
    }
}