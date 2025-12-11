using System;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services.Distributions
{
    /// <summary>
    /// Базовый класс для всех распределений.
    /// </summary>
    public abstract class DistributionBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract DiscreteRandomVariable Generate();

        protected static double BinomialCoefficient(int n, int k)
        {
            if (k < 0 || k > n) return 0;
            if (k == 0 || k == n) return 1;

            k = Math.Min(k, n - k);
            double result = 1;
            for (int i = 0; i < k; i++)
            {
                result *= (n - i);
                result /= (i + 1);
            }
            return result;
        }
    }
}
