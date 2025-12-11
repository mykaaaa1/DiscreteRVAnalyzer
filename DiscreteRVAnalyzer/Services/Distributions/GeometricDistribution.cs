using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services.Distributions
{
    public class GeometricDistribution : DistributionBase
    {
        private double _p; // Ймовірність успіху

        public GeometricDistribution(double p)
        {
            if (p <= 0 || p > 1) throw new ArgumentException("Ймовірність має бути > 0 і <= 1.");
            _p = p;
        }

        public override string Name => "Геометричний";
        public override string Description => $"Ge(p={_p})";

        public override DiscreteRandomVariable Generate()
        {
            var dict = new Dictionary<int, double>();
            // geometric support k = 1..infty, truncate when prob becomes negligible
            double sum = 0;
            int k = 1;
            while (true)
            {
                double p = Math.Pow(1 - _p, k - 1) * _p;
                dict[k] = p;
                sum += p;
                if (p < 1e-10 && k > 1000) break;
                if (sum > 1 - 1e-12) break;
                k++;
                if (k > 100000) break; // safety
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

        public double CalculateProbability(int k)
        {
            if (k < 1) return 0; // Успіх не може бути на 0-й спробі
            // P = (1-p)^(k-1) * p
            return Math.Pow(1 - _p, k - 1) * _p;
        }

        public double Mean => 1.0 / _p;

        public double Variance => (1 - _p) / Math.Pow(_p, 2);

        public double StandardDeviation => Math.Sqrt(Variance);
    }
}