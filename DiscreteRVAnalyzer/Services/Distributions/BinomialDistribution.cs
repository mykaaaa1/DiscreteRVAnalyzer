using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Utils;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services.Distributions
{
    public class BinomialDistribution : DistributionBase
    {
        private int _n;      // Кількість випробувань
        private double _p;   // Ймовірність успіху

        public BinomialDistribution(int n, double p)
        {
            if (p < 0 || p > 1) throw new ArgumentException("Ймовірність має бути від 0 до 1.");
            if (n < 0) throw new ArgumentException("n має бути невід'ємним цілим числом.");
            _n = n;
            _p = p;
        }

        public override string Name => "Біноміальний";
        public override string Description => $"B(n={_n}, p={_p})";

        public override DiscreteRandomVariable Generate()
        {
            var dict = new Dictionary<int, double>();
            for (int k = 0; k <= _n; k++)
            {
                double prob = BinomialCoefficient(_n, k) * Math.Pow(_p, k) * Math.Pow(1 - _p, _n - k);
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