using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services.Distributions
{
    public sealed class GeometricDistribution : DistributionBase
    {
        private readonly double _p;

        public GeometricDistribution(double p)
        {
            if (p <= 0 || p > 1) throw new ArgumentException("0 < p <= 1");
            _p = p;
        }

        public override string Name => "Геометрическое";
        public override string Description => $"Ge(p={_p:F3})";

        public override DiscreteRandomVariable Generate()
        {
            var dict = new Dictionary<int, double>();
            double q = 1 - _p;
            double qPow = 1.0;

            for (int k = 1; k <= 200; k++)
            {
                double prob = qPow * _p;
                if (prob < 1e-15) break;
                dict[k] = prob;
                qPow *= q;
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
