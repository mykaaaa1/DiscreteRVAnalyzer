using System;
using System.Collections.Generic;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services
{
    public static class GraphService
    {
        /// <summary>
        /// Точки многоугольника распределения (PMF): (x, P(X=x)).
        /// </summary>
        public static List<(int X, double Y)> GetPolygonPoints(DiscreteRandomVariable rv)
        {
            var list = new List<(int X, double Y)>();
            foreach (var x in rv.GetSortedSupport())
            {
                double p = rv.PMF(x);
                list.Add((x, p));             // ВАЖНО: один аргумент – кортеж
            }
            return list;
        }

        /// <summary>
        /// Точки интегральной функции распределения (CDF): ступенчатый график.
        /// </summary>
        public static List<(double X, double Y)> GetCumulativePoints(DiscreteRandomVariable rv)
        {
            var list = new List<(double X, double Y)>();
            var (min, max) = rv.GetRange();

            // слева от минимума
            list.Add((min - 0.5, 0.0));

            double cumPrev = 0.0;
            foreach (var x in rv.GetSortedSupport())
            {
                double p = rv.PMF(x);
                double cum = cumPrev + p;

                // горизонтальный участок до скачка
                list.Add((x - 0.001, cumPrev));
                // скачок
                list.Add((x + 0.001, cum));

                cumPrev = cum;
            }

            // справа от максимума
            list.Add((max + 0.5, 1.0));

            return list;
        }
    }
}
