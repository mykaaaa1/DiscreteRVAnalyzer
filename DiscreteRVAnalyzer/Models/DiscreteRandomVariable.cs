using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscreteRVAnalyzer.Models
{
    public class DiscreteRandomVariable : IEquatable<DiscreteRandomVariable?>
    {
        private Dictionary<int, double> _distribution = new();
        private const double TOL = 1e-10;

        public string Name { get; set; } = "X";
        public string Description { get; set; } = string.Empty;

        public IReadOnlyDictionary<int, double> Distribution => _distribution;

        public int SupportSize => _distribution.Count;

        public bool IsNormalized
        {
            get
            {
                if (_distribution.Count == 0) return false;
                double sum = _distribution.Values.Sum();
                return Math.Abs(sum - 1.0) < TOL;
            }
        }

        public void LoadDistribution(Dictionary<int, double> distribution)
        {
            if (distribution == null || distribution.Count == 0)
                throw new ArgumentException("Распределение пусто.");

            _distribution = new Dictionary<int, double>(distribution);
        }

        public void Normalize()
        {
            double sum = _distribution.Values.Sum();
            if (sum <= 0) throw new InvalidOperationException("Сумма вероятностей должна быть > 0.");

            var keys = _distribution.Keys.ToList();
            foreach (var k in keys) _distribution[k] /= sum;
        }

        public void Validate()
        {
            if (_distribution.Count == 0)
                throw new InvalidOperationException("Распределение пусто.");

            // **НОВОЕ**: Каждая вероятность 0 ≤ P ≤ 1
            foreach (var kvp in _distribution)
            {
                if (kvp.Value < 0 || kvp.Value > 1)
                    throw new InvalidOperationException(
                        $"Вероятность P(X={kvp.Key}) = {kvp.Value:F6} выходит за пределы [0, 1]. " +
                        $"Все вероятности должны быть между 0 и 1.");
            }

            double sum = _distribution.Values.Sum();
            if (Math.Abs(sum - 1.0) > 1e-6)
                throw new InvalidOperationException(
                    $"Сумма вероятностей должна быть 1, получено {sum:F6}. " +
                    $"Вероятности не нормализованы.");
        }

        public IEnumerable<int> GetSortedSupport() => _distribution.Keys.OrderBy(x => x);

        public (int Min, int Max) GetRange()
        {
            if (_distribution.Count == 0)
                throw new InvalidOperationException("Распределение пусто.");
            return (_distribution.Keys.Min(), _distribution.Keys.Max());
        }

        public double PMF(int x) =>
            _distribution.TryGetValue(x, out var p) ? p : 0.0;

        public double CDF(int x) =>
            _distribution.Where(kvp => kvp.Key <= x).Sum(kvp => kvp.Value);

        public double ProbabilityInterval(int a, int b)
        {
            if (a > b) (a, b) = (b, a);
            return _distribution.Where(kvp => kvp.Key >= a && kvp.Key <= b).Sum(kvp => kvp.Value);
        }

        public DiscreteRandomVariable Clone()
        {
            return new DiscreteRandomVariable
            {
                Name = Name,
                Description = Description,
                _distribution = new Dictionary<int, double>(_distribution)
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ДСВ {Name} ({Description})");
            foreach (var kv in _distribution.OrderBy(k => k.Key))
                sb.AppendLine($"P({Name}={kv.Key}) = {kv.Value:E6}");
            return sb.ToString();
        }

        public bool Equals(DiscreteRandomVariable? other) =>
            other != null && Name == other.Name && SupportSize == other.SupportSize;

        public override bool Equals(object? obj) => Equals(obj as DiscreteRandomVariable);

        public override int GetHashCode() => HashCode.Combine(Name, SupportSize);
    }
}
