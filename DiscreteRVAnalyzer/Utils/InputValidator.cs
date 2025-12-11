using System;
using System.Globalization;

namespace DiscreteRVAnalyzer.Utils
{
    /// <summary>
    /// Валідація вхідних параметрів
    /// </summary>
    public static class InputValidator
    {
        public const int MAX_N = 100000;
        public const int MIN_N = 1;
        public const double MIN_PROBABILITY = 0.0;
        public const double MAX_PROBABILITY = 1.0;
        public const double MIN_LAMBDA = 0.01;
        public const double MAX_LAMBDA = 1000.0;
        public const double EPSILON = 1e-9;

        public static bool TryParseInt(string input, int minValue, int maxValue, out int result, out string errorMessage)
        {
            result = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Поле не може бути порожнім";
                return false;
            }

            if (!int.TryParse(input.Trim(), out result))
            {
                errorMessage = $"'{input}' не є цілим числом";
                return false;
            }

            if (result < minValue)
            {
                errorMessage = $"Значення має бути не менше {minValue}";
                return false;
            }

            if (result > maxValue)
            {
                errorMessage = $"Значення не повинно перевищувати {maxValue}";
                return false;
            }

            return true;
        }

        public static bool TryParseProbability(string input, out double result, out string errorMessage)
        {
            result = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Поле не може бути порожнім";
                return false;
            }

            string normalizedInput = input.Trim().Replace(',', '.');

            if (!double.TryParse(normalizedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                errorMessage = $"'{input}' не є числом";
                return false;
            }

            if (result < MIN_PROBABILITY)
            {
                errorMessage = $"Ймовірність не може бути менше {MIN_PROBABILITY}";
                return false;
            }

            if (result > MAX_PROBABILITY)
            {
                errorMessage = $"Ймовірність не може бути більше {MAX_PROBABILITY}";
                return false;
            }

            return true;
        }

        public static bool TryParsePositiveDouble(string input, double minValue, double maxValue, out double result, out string errorMessage)
        {
            result = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Поле не може бути порожнім";
                return false;
            }

            string normalizedInput = input.Trim().Replace(',', '.');

            if (!double.TryParse(normalizedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                errorMessage = $"'{input}' не є числом";
                return false;
            }

            if (result < minValue)
            {
                errorMessage = $"Значення має бути не менше {minValue}";
                return false;
            }

            if (result > maxValue)
            {
                errorMessage = $"Значення має бути не більше {maxValue}";
                return false;
            }

            return true;
        }

        public static string ValidateBinomial(int n, double p)
        {
            if (n < MIN_N) return $"N повинно бути не менше {MIN_N}";
            if (n > MAX_N) return $"N не повинно перевищувати {MAX_N}";
            if (p < MIN_PROBABILITY || p > MAX_PROBABILITY) return "P повинна бути в діапазоні [0, 1]";
            if (Math.Abs(p) < EPSILON) return "P не може бути нулем для осмисленого розподілу";
            return null;
        }

        public static string ValidatePoisson(double lambda)
        {
            if (lambda < MIN_LAMBDA) return $"λ повинна бути не менше {MIN_LAMBDA}";
            if (lambda > MAX_LAMBDA) return $"λ не повинна перевищувати {MAX_LAMBDA} (може викликати overflow)";
            return null;
        }

        public static string ValidateGeometric(double p)
        {
            if (p <= 0 || p > 1) return "P повинна бути в діапазоні (0, 1]";
            if (Math.Abs(p - 1.0) < EPSILON) return "P не може бути одиницею для осмисленого розподілу";
            return null;
        }

        public static string ValidateHypergeometric(int N, int K, int n)
        {
            if (N < 1) return "N (розмір сукупності) повинно бути не менше 1";
            if (K < 0) return "K (число сприятливих) не може бути від'ємним";
            if (K > N) return "K не може бути більше N";
            if (n < 1) return "n (розмір вибірки) повинно бути не менше 1";
            if (n > N) return "n не може бути більше N";
            return null;
        }
    }
}
