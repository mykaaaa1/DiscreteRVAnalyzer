using System;
using System.Globalization;

namespace DiscreteRVAnalyzer.Utils
{
    /// <summary>
    /// Валидация входных параметров
    /// </summary>
    public static class InputValidator
    {
        // Константы для ограничений
        public const int MAX_N = 100000;  // Максимальное число испытаний
        public const int MIN_N = 1;       // Минимальное число испытаний
        public const double MIN_PROBABILITY = 0.0;
        public const double MAX_PROBABILITY = 1.0;
        public const double MIN_LAMBDA = 0.01;
        public const double MAX_LAMBDA = 1000.0;
        public const double EPSILON = 1e-9; // Для сравнения с нулём

        /// <summary>
        /// Парсит целое число с валидацией
        /// </summary>
        public static bool TryParseInt(string input, int minValue, int maxValue, out int result, out string errorMessage)
        {
            result = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Поле не может быть пустым";
                return false;
            }

            if (!int.TryParse(input.Trim(), out result))
            {
                errorMessage = $"'{input}' не является целым числом";
                return false;
            }

            if (result < minValue)
            {
                errorMessage = $"Значение должно быть не менее {minValue}";
                return false;
            }

            if (result > maxValue)
            {
                errorMessage = $"Значение не должно превышать {maxValue}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Парсит вероятность (0..1) с валидацией
        /// </summary>
        public static bool TryParseProbability(string input, out double result, out string errorMessage)
        {
            result = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Поле не может быть пустым";
                return false;
            }

            string normalizedInput = input.Trim().Replace(',', '.');

            if (!double.TryParse(normalizedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                errorMessage = $"'{input}' не является числом";
                return false;
            }

            if (result < MIN_PROBABILITY)
            {
                errorMessage = $"Вероятность не может быть меньше {MIN_PROBABILITY}";
                return false;
            }

            if (result > MAX_PROBABILITY)
            {
                errorMessage = $"Вероятность не может быть больше {MAX_PROBABILITY}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Парсит положительное вещественное число с валидацией
        /// </summary>
        public static bool TryParsePositiveDouble(string input, double minValue, double maxValue, out double result, out string errorMessage)
        {
            result = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                errorMessage = "Поле не может быть пустым";
                return false;
            }

            string normalizedInput = input.Trim().Replace(',', '.');

            if (!double.TryParse(normalizedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                errorMessage = $"'{input}' не является числом";
                return false;
            }

            if (result < minValue)
            {
                errorMessage = $"Значение должно быть не менее {minValue}";
                return false;
            }

            if (result > maxValue)
            {
                errorMessage = $"Значение должно быть не более {maxValue}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет совместимость параметров биномиального распределения
        /// </summary>
        public static string ValidateBinomial(int n, double p)
        {
            if (n < MIN_N) return $"N должно быть не менее {MIN_N}";
            if (n > MAX_N) return $"N не должно превышать {MAX_N}";
            if (p < MIN_PROBABILITY || p > MAX_PROBABILITY) return "P должна быть в диапазоне [0, 1]";
            if (Math.Abs(p) < EPSILON) return "P не может быть нулём для осмысленного распределения";
            return null; // Валидно
        }

        /// <summary>
        /// Проверяет совместимость параметров распределения Пуассона
        /// </summary>
        public static string ValidatePoisson(double lambda)
        {
            if (lambda < MIN_LAMBDA) return $"λ должна быть не менее {MIN_LAMBDA}";
            if (lambda > MAX_LAMBDA) return $"λ не должна превышать {MAX_LAMBDA} (может вызвать overflow)";
            return null; // Валидно
        }

        /// <summary>
        /// Проверяет совместимость параметров геометрического распределения
        /// </summary>
        public static string ValidateGeometric(double p)
        {
            if (p <= 0 || p > 1) return "P должна быть в диапазоне (0, 1]";
            if (Math.Abs(p - 1.0) < EPSILON) return "P не может быть единицей для осмысленного распределения";
            return null; // Валидно
        }

        /// <summary>
        /// Проверяет совместимость параметров гипергеометрического распределения
        /// </summary>
        public static string ValidateHypergeometric(int N, int K, int n)
        {
            if (N < 1) return "N (размер совокупности) должна быть не менее 1";
            if (K < 0) return "K (число благоприятных) не может быть отрицательной";
            if (K > N) return "K не может быть больше N";
            if (n < 1) return "n (размер выборки) должна быть не менее 1";
            if (n > N) return "n не может быть больше N";
            return null; // Валидно
        }
    }
}
