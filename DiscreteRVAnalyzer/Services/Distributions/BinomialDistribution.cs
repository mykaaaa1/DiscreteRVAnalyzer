using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services.Distributions
{
    public sealed class BinomialDistribution : DistributionBase
    {
        private readonly int _n;
        private readonly double _p;

        public BinomialDistribution(int n, double p)
        {
            if (n <= 0) throw new ArgumentException("n > 0");
            if (p < 0 || p > 1) throw new ArgumentException("0 <= p <= 1");
            _n = n;
            _p = p;
        }

        public override string Name => "Биномиальное";
        public override string Description => $"B(n={_n}, p={_p:F3})";

        public override DiscreteRandomVariable Generate()
        {
            var dict = new Dictionary<int, double>();
            double q = 1 - _p;

            for (int k = 0; k <= _n; k++)
            {
                double prob = BinomialCoefficient(_n, k) *
                              Math.Pow(_p, k) *
                              Math.Pow(q, _n - k);
                dict[k] = prob;
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
