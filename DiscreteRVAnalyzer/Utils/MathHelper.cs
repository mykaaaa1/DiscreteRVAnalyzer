using System;

namespace DiscreteRVAnalyzer.Utils
{
    public static class MathHelper
    {
        // Факторіал (n!)
        public static double Factorial(int n)
        {
            if (n < 0) return 0; // Або викинути помилку
            if (n == 0 || n == 1) return 1;

            double result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;

            return result;
        }

        // Біноміальний коефіцієнт (C з n по k)
        public static double Combinations(int n, int k)
        {
            if (k < 0 || k > n) return 0;
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }
    }
}