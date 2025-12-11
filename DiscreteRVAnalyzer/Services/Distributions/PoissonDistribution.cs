using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services.Distributions
{
    public sealed class PoissonDistribution : DistributionBase
    {
        private readonly double _lambda;

        public PoissonDistribution(double lambda)
        {
            if (lambda <= 0) throw new ArgumentException("λ > 0");
            _lambda = lambda;
        }

        public override string Name => "Пуассона";
        public override string Description => $"Po(λ={_lambda:F3})";

        public override DiscreteRandomVariable Generate()
        {
            var dict = new Dictionary<int, double>();
            double prob = Math.Exp(-_lambda);
            dict[0] = prob;

            int k = 1;
            while (k < 100 && prob > 1e-15)
            {
                prob *= _lambda / k;
                dict[k] = prob;
                k++;
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
