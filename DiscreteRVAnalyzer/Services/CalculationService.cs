using System;
using System.Collections.Generic;
using System.Linq;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services
{
    public static class CalculationService
    {
        public static StatisticalCharacteristics Calculate(DiscreteRandomVariable rv)
        {
            rv.Validate();

            var xs = rv.GetSortedSupport().ToList();
            var (min, max) = rv.GetRange();

            // начальные моменты v_k = M(X^k)
            double v1 = 0, v2 = 0, v3 = 0, v4 = 0;

            foreach (var x in xs)
            {
                double p = rv.PMF(x);
                double x1 = x;
                double x2 = x1 * x1;
                double x3 = x2 * x1;
                double x4 = x3 * x1;

                v1 += x1 * p;
                v2 += x2 * p;
                v3 += x3 * p;
                v4 += x4 * p;
            }

            double mean = v1;

            // D(X) = v2 - v1^2  [file:68]
            double variance = v2 - mean * mean;
            if (variance < 0) variance = 0;
            double sigma = Math.Sqrt(variance);

            // центральные моменты [file:68]:
            // μ2 = D(X)
            // μ3 = v3 - 3 v2 v1 + 2 v1^3
            // μ4 = v4 - 4 v3 v1 + 6 v2 v1^2 - 3 v1^4
            double mu2 = variance;
            double mu3 = v3 - 3 * v2 * v1 + 2 * Math.Pow(v1, 3);
            double mu4 = v4 - 4 * v3 * v1 + 6 * v2 * v1 * v1 - 3 * Math.Pow(v1, 4);

            double skew = 0, kurt = 0;
            if (sigma > 1e-12)
            {
                skew = mu3 / Math.Pow(sigma, 3);
                kurt = mu4 / Math.Pow(sigma, 4) - 3;
            }

            // мода, медиана, квартили
            int mode = xs.MaxBy(x => rv.PMF(x));
            int median = xs.First(x => rv.CDF(x) >= 0.5);

            int q1 = xs.First(x => rv.CDF(x) >= 0.25);
            int q3 = xs.First(x => rv.CDF(x) >= 0.75);
            double iqr = q3 - q1;

            double cv = Math.Abs(mean) > 1e-12 ? sigma / Math.Abs(mean) : 0;
            double rsd = cv * 100.0;

            return new StatisticalCharacteristics
            {
                Mean = mean,
                SecondMoment = v2,
                ThirdMoment = v3,
                FourthMoment = v4,
                Variance = variance,
                StandardDeviation = sigma,
                CentralSecondMoment = mu2,
                CentralThirdMoment = mu3,
                CentralFourthMoment = mu4,
                Skewness = skew,
                Kurtosis = kurt,
                Mode = mode,
                Median = median,
                QuantileQ1 = q1,
                QuantileQ3 = q3,
                InterquartileRange = iqr,
                MinValue = min,
                MaxValue = max,
                Range = max - min,
                CoefficientOfVariation = cv,
                RelativeStandardDeviation = rsd
            };
        }
    }
}
