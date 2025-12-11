using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Utils;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services.Distributions
{
    public class PoissonDistribution : DistributionBase
    {
        private double _lambda; // Інтенсивність

        public PoissonDistribution(double lambda)
        {
            if (lambda <= 0) throw new ArgumentException("Лямбда має бути додатною.");
            _lambda = lambda;
        }

        public override string Name => "Пуассона";
        public override string Description => $"Po(lambda={_lambda})";

        public override DiscreteRandomVariable Generate()
        {
            // approximate support until probabilities become negligible
            var dict = new Dictionary<int, double>();
            double sum = 0;
            int k = 0;
            while (true)
            {
                double p = (Math.Pow(_lambda, k) * Math.Exp(-_lambda)) / MathHelper.Factorial(k);
                dict[k] = p;
                sum += p;
                if (p < 1e-8 && k > _lambda * 5) break;
                k++;
                if (k > 1000) break; // safety
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