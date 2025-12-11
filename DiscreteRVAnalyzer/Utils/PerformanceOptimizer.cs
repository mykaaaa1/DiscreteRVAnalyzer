using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DiscreteRVAnalyzer.Utils
{
    /// <summary>
    /// Оптимизация производительности и кэширование
    /// </summary>
    public class PerformanceOptimizer
    {
        // Кэш результатов расчётов
        private static readonly Dictionary<string, (object result, DateTime expiry)> ResultCache =
            new Dictionary<string, (object, DateTime)>();

        private const int CacheExpirySeconds = 300; // 5 минут
        private const long OperationTimeoutMs = 5000; // 5 секунд

        public static (object result, TimeSpan elapsed) ExecuteWithTimeout<T>(
            Func<T> operation,
            string operationName = "Операция")
        {
            var sw = Stopwatch.StartNew();

            try
            {
                // Используем Task с timeout
                var task = System.Threading.Tasks.Task.Run(operation);
                if (task.Wait(TimeSpan.FromMilliseconds(OperationTimeoutMs)))
                {
                    sw.Stop();
                    return (task.Result, sw.Elapsed);
                }
                else
                {
                    sw.Stop();
                    throw new TimeoutException($"{operationName} превысила лимит времени ({OperationTimeoutMs}ms)");
                }
            }
            catch (AggregateException ae)
            {
                sw.Stop();
                throw ae.InnerException ?? ae;
            }
        }

        public static bool TryGetCachedResult<T>(string key, out T result)
        {
            result = default;

            if (!ResultCache.TryGetValue(key, out var cached)) return false;
            if (DateTime.Now > cached.expiry)
            {
                ResultCache.Remove(key);
                return false;
            }

            result = (T)cached.result;
            return true;
        }

        public static void CacheResult<T>(string key, T result)
        {
            var expiry = DateTime.Now.AddSeconds(CacheExpirySeconds);
            ResultCache[key] = (result, expiry);
        }

        public static void ClearCache()
        {
            ResultCache.Clear();
        }

        public static void ClearExpiredCache()
        {
            var now = DateTime.Now;
            var keysToRemove = new List<string>();

            foreach (var kvp in ResultCache)
            {
                if (now > kvp.Value.expiry)
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
                ResultCache.Remove(key);
        }
    }
}
