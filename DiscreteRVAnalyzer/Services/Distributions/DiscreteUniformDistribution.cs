using System;

namespace DiscreteRVAnalyzer.Distributions
{
    public class DiscreteUniformDistribution : IDistribution
    {
        private int _a; // Початок діапазону
        private int _b; // Кінець діапазону
        private int _n; // Кількість елементів

        public string Name => "Рівномірний (Дискретний)";

        public DiscreteUniformDistribution(int a, int b)
        {
            if (b < a) throw new ArgumentException("Кінець діапазону має бути більшим за початок.");
            _a = a;
            _b = b;
            _n = b - a + 1;
        }

        public double CalculateProbability(int k)
        {
            if (k >= _a && k <= _b) return 1.0 / _n;
            return 0;
        }

        public double Mean => (_a + _b) / 2.0;

        public double Variance => (Math.Pow(_n, 2) - 1) / 12.0;

        public double StandardDeviation => Math.Sqrt(Variance);
    }
}