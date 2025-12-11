using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Models;
using DiscreteRVAnalyzer.Services.Distributions;

namespace DiscreteRVAnalyzer.Services
{
    /// <summary>
    /// Factory для создания распределений по типу
    /// Использует Strategy паттерн для расширяемости
    /// </summary>
    public class DistributionFactory
    {
        private readonly Dictionary<DistributionType, Func<object[], DistributionBase>>
            _distributionCreators;

        public DistributionFactory()
        {
            _distributionCreators = new()
            {
                {
                    DistributionType.Binomial,
                    args => new BinomialDistribution((int)args[0], (double)args[1])
                },
                {
                    DistributionType.Poisson,
                    args => new PoissonDistribution((double)args[0])
                },
                {
                    DistributionType.Geometric,
                    args => new GeometricDistribution((double)args[0])
                },
                {
                    DistributionType.Hypergeometric,
                    args => new HypergeometricDistribution((int)args[0], (int)args[1], (int)args[2])
                }
            };
        }

        /// <summary>
        /// Создать распределение по типу и параметрам
        /// </summary>
        public DistributionBase Create(DistributionType type, params object[] parameters)
        {
            if (!_distributionCreators.TryGetValue(type, out var creator))
                throw new NotSupportedException($"Распределение {type} не поддерживается");

            try
            {
                return creator(parameters);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    $"Ошибка при создании {type} с параметрами: {string.Join(", ", parameters)}", ex);
            }
        }

        /// <summary>
        /// Получить все доступные типы распределений
        /// </summary>
        public IEnumerable<DistributionType> GetAvailableTypes() =>
            _distributionCreators.Keys;
    }
}
