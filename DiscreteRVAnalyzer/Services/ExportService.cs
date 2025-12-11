using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using DiscreteRVAnalyzer.Models;

namespace DiscreteRVAnalyzer.Services
{
    public static class ExportService
    {
        public static string ExportToJson(DiscreteRandomVariable rv)
        {
            var dto = new
            {
                rv.Name,
                rv.Description,
                Distribution = rv.Distribution
            };
            return JsonConvert.SerializeObject(dto, Formatting.Indented);
        }

        public static DiscreteRandomVariable ImportFromJson(string json)
        {
            dynamic? dto = JsonConvert.DeserializeObject<dynamic>(json)
                ?? throw new InvalidOperationException("Пустой JSON.");

            var rv = new DiscreteRandomVariable
            {
                Name = dto.Name ?? "X",
                Description = dto.Description ?? ""
            };

            var dict = new System.Collections.Generic.Dictionary<int, double>();
            foreach (var item in dto.Distribution)
            {
                int x = int.Parse(item.Name);
                double p = (double)item.Value;
                dict[x] = p;
            }

            rv.LoadDistribution(dict);
            rv.Normalize();
            return rv;
        }

        public static void ExportReportToFile(
            string path,
            DiscreteRandomVariable rv,
            StatisticalCharacteristics stats)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"X: {rv.Name}");
            sb.AppendLine(rv.Description);
            sb.AppendLine();
            sb.AppendLine("Распределение:");
            foreach (var kv in rv.Distribution)
                sb.AppendLine($"P(X={kv.Key}) = {kv.Value:E6}");
            sb.AppendLine();
            sb.AppendLine(stats.GetFormattedReport());
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        public static void ExportToCsv(string path, DiscreteRandomVariable rv)
        {
            var sb = new StringBuilder();
            sb.AppendLine("X,P,F");
            double c = 0;
            foreach (var x in rv.GetSortedSupport())
            {
                double p = rv.PMF(x);
                c += p;
                sb.AppendLine($"{x},{p:E6},{c:E6}");
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
