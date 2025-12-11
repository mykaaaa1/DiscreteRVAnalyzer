using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services.Distributions
{
    public sealed class HypergeometricDistribution : DistributionBase
    {
        private readonly int _N, _K, _n;

        public HypergeometricDistribution(int N, int K, int n)
        {
            if (N <= 0 || K < 0 || K > N || n <= 0 || n > N)
                throw new ArgumentException("Некорректные параметры гипергеометрического распределения.");
            _N = N;
            _K = K;
            _n = n;
        }

        public override string Name => "Гипергеометрическое";
        public override string Description => $"H(N={_N}, K={_K}, n={_n})";

        public override DiscreteRandomVariable Generate()
        {
            var dict = new Dictionary<int, double>();

            int kMin = Math.Max(0, _n - (_N - _K));
            int kMax = Math.Min(_n, _K);

            double denom = BinomialCoefficient(_N, _n);

            for (int k = kMin; k <= kMax; k++)
            {
                double num = BinomialCoefficient(_K, k) *
                             BinomialCoefficient(_N - _K, _n - k);
                dict[k] = num / denom;
            }

            var rv = new DiscreteRandomVariable
            {
                Name = "X",
                Description = Description
            };
            rv.LoadDistribution(dict);
            rv.Normalize();
            return rv;
        }
    }
}
