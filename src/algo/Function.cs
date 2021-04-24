using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MultiDimensionalOptimization.algo
{
    public class Function
    {
        private readonly int _n;
        public int N => _n;
        
        private readonly Matrix<double> _a;
        public Matrix<double> A { get; }

        private readonly Matrix<double> _b;
        private readonly double _c;

        public Function(int n, double[] a, double[] b, double c)
        {
            // TODO check dimensions 
            _n = n;
            _a = AdvancedMath.ToMatrix(n, a);
            A = ComputeA(n, _a);
            _b = AdvancedMath.ToVector(n, b);
            _c = c;
        }

        public double[,] GetMatrix()
        {
            return A.ToArray();
        }

        public double Apply(double[] x)
        {
            return Apply(AdvancedMath.ToVector(_n, x));
        }
        
        public double Apply(Matrix<double> x)
        {
            return 0.5 * x.Transpose().Multiply(_a).Multiply(x)[0, 0] -
                _b.Transpose().Multiply(x)[0, 0] + _c;
        }

        public Matrix<double> Gradient(Matrix<double> x)
        {
            return A.Multiply(x).Subtract(_b);
        }
        
        public Matrix<double> Gradient(double[] x)
        {
            return Gradient(AdvancedMath.ToVector(_n, x));
        }
        
        public double[] GradientArray(double[] x)
        {
            return Gradient(x).ToColumnArrays()[0];
        }

        private static Matrix ComputeA(int n, Matrix<double> a)
        {
            var vA = new double[n * n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    vA[i * n + j] = 0.5 * (a[i, j] + a[j, i]);
                }
            }

            return new DenseMatrix(n, n, vA);
        }
    }
}