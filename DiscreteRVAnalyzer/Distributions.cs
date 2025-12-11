using System;
using DiscreteRVAnalyzer.Utils;

namespace DiscreteRVAnalyzer
{
    public interface IDistribution
    {
        string Name { get; }
        double Mean { get; }
        double Variance { get; }
        double CalculateProb(int k);
        string GetFormula();
    }

    public class BinomialDist : IDistribution
    {
        private readonly int _n;
        private readonly double _p;
        public BinomialDist(int n, double p) { _n = n; _p = p; }
        public string Name => "Біноміальний";
        public double Mean => _n * _p;
        public double Variance => _n * _p * (1 - _p);
        public string GetFormula() => "C(n,k) * p^k * q^(n-k)";
        public double CalculateProb(int k) => MathHelper.Combinations(_n, k) * Math.Pow(_p, k) * Math.Pow(1 - _p, _n - k);
    }

    public class PoissonDist : IDistribution
    {
        private readonly double _lambda;
        public PoissonDist(double lambda) { _lambda = lambda; }
        public string Name => "Пуассона";
        public double Mean => _lambda;
        public double Variance => _lambda;
        public string GetFormula() => "(?^k * e^-?) / k!";
        public double CalculateProb(int k) => (Math.Pow(_lambda, k) * Math.Exp(-_lambda)) / MathHelper.Factorial(k);
    }

    public class GeometricDist : IDistribution
    {
        private readonly double _p;
        public GeometricDist(double p) { _p = p; }
        public string Name => "Геометричний";
        public double Mean => 1.0 / _p;
        public double Variance => (1 - _p) / (_p * _p);
        public string GetFormula() => "q^(k-1) * p";
        public double CalculateProb(int k) => k < 1 ? 0 : Math.Pow(1 - _p, k - 1) * _p;
    }

    public class UniformDist : IDistribution
    {
        private int _a; // Мин
        private int _b; // Макс
        private int _n; // Количество значений

        public UniformDist(int a, int b)
        {
            if (b < a)
            {
                // swap
                _a = b;
                _b = a;
            }
            else
            {
                _a = a;
                _b = b;
            }
            _n = _b - _a + 1;
        }

        public string Name => "Рівномірний (Дискретний)";

        public double Mean => (_a + _b) / 2.0;

        public double Variance => (Math.Pow(_n, 2) - 1) / 12.0;

        public string GetFormula() => "1 / (b - a + 1)";

        public double CalculateProb(int k)
        {
            if (k >= _a && k <= _b)
                return 1.0 / _n;
            return 0.0;
        }
    }

    // Гіпергеометричний розподіл (згідно зі слайдом 19)
    public class HypergeometricDist : IDistribution
    {
        private int _N; // Всього елементів
        private int _M; // Кількість "успішних" елементів у популяції
        private int _n; // Розмір вибірки

        public HypergeometricDist(int N, int M, int n)
        {
            if (M > N || n > N) throw new ArgumentException("Параметри M та n не можуть бути більші за N");
            _N = N;
            _M = M;
            _n = n;
        }

        public string Name => "Гіпергеометричний";

        // Формула: C(M, k) * C(N-M, n-k) / C(N, n)
        public string GetFormula() => "C(M,m) * C(N-M, n-m) / C(N,n)";

        // M(X) = n * M / N
        public double Mean => (double)_n * _M / _N;

        // D(X) = (n * M * (N - M) * (N - n)) / (N^2 * (N - 1))
        public double Variance
        {
            get
            {
                double numerator = (double)_n * _M * (_N - _M) * (_N - _n);
                double denominator = Math.Pow(_N, 2) * (_N - 1);
                return numerator / denominator;
            }
        }

        public double CalculateProb(int k) // тут k це "m" з лекції
        {
            // k не може бути більше ніж розмір вибірки (n) або кількість успішних (M)
            if (k < 0 || k > _n || k > _M) return 0;

            // Якщо кількість невдач (n-k) більша, ніж всього невдач (N-M), то це неможливо
            if ((_n - k) > (_N - _M)) return 0;

            double top = MathHelper.Combinations(_M, k) * MathHelper.Combinations(_N - _M, _n - k);
            double bottom = MathHelper.Combinations(_N, _n);

            return top / bottom;
        }
    }
}
