using System;
using System.Text;

namespace DiscreteRVAnalyzer.Models
{
    public class StatisticalCharacteristics
    {
        // начальные моменты v_k = M(X^k)
        public double Mean { get; set; }            // v1 = M(X)
        public double SecondMoment { get; set; }    // v2 = M(X^2)
        public double ThirdMoment { get; set; }     // v3 = M(X^3)
        public double FourthMoment { get; set; }    // v4 = M(X^4)

        // центральные моменты μ_k
        public double CentralSecondMoment { get; set; }  // μ2 = D(X)
        public double CentralThirdMoment { get; set; }   // μ3
        public double CentralFourthMoment { get; set; }  // μ4

        public double Variance { get; set; }        // D(X)
        public double StandardDeviation { get; set; } // σ(X)

        public double Skewness { get; set; }        // γ1 = μ3 / σ^3
        public double Kurtosis { get; set; }        // γ2 = μ4 / σ^4 - 3

        public int Mode { get; set; }
        public int Median { get; set; }
        public int QuantileQ1 { get; set; }
        public int QuantileQ3 { get; set; }
        public double InterquartileRange { get; set; }

        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int Range { get; set; }

        public double CoefficientOfVariation { get; set; }
        public double RelativeStandardDeviation { get; set; }

        public string GetFormattedReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine("╔════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║         ЧИСЛОВЫЕ ХАРАКТЕРИСТИКИ ДИСКРЕТНОЙ ЧВ             ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            // Начальные моменты
            sb.AppendLine("┌─ НАЧАЛЬНЫЕ МОМЕНТЫ (vₖ = M(Xᵏ)) ─────────────────────────┐");
            sb.AppendLine($"│  M(X)        = v₁ = {Mean,15:F8}                    │");
            sb.AppendLine($"│  M(X²)       = v₂ = {SecondMoment,15:F8}                    │");
            sb.AppendLine($"│  M(X³)       = v₃ = {ThirdMoment,15:F8}                    │");
            sb.AppendLine($"│  M(X⁴)       = v₄ = {FourthMoment,15:F8}                    │");
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
            sb.AppendLine();

            // Дисперсия и СКО
            sb.AppendLine("┌─ ДИСПЕРСИЯ И СРЕДНЕЕ КВАДРАТИЧНОЕ ОТКЛОНЕНИЕ ─────────────┐");
            sb.AppendLine($"│  D(X) = v₂ - v₁²  = {Variance,15:F8}                    │");
            sb.AppendLine($"│  σ(X) = √D(X)     = {StandardDeviation,15:F8}                    │");
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
            sb.AppendLine();

            // Центральные моменты
            sb.AppendLine("┌─ ЦЕНТРАЛЬНЫЕ МОМЕНТЫ (μₖ) ──────────────────────────────────┐");
            sb.AppendLine($"│  μ₂ = D(X)                 = {CentralSecondMoment,15:F8}                    │");
            sb.AppendLine($"│  μ₃ = M[(X-M(X))³]         = {CentralThirdMoment,15:F8}                    │");
            sb.AppendLine($"│  μ₄ = M[(X-M(X))⁴]         = {CentralFourthMoment,15:F8}                    │");
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
            sb.AppendLine();

            // Коэффициенты асимметрии и эксцесса
            sb.AppendLine("┌─ АСИММЕТРИЯ И ЭКСЦЕСС ───────────────────────────────────────┐");
            sb.AppendLine($"│  Skewness γ₁ = μ₃/σ³       = {Skewness,15:F8}                    │");
            sb.AppendLine($"│  Kurtosis γ₂ = μ₄/σ⁴ - 3   = {Kurtosis,15:F8}                    │");
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
            sb.AppendLine();

            // Мода и медиана
            sb.AppendLine("┌─ МОДА И МЕДИАНА ─────────────────────────────────────────────┐");
            sb.AppendLine($"│  Mo (мода)              = {Mode,15}                         │");
            sb.AppendLine($"│  Me (медиана)           = {Median,15}                         │");
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
            sb.AppendLine();

            // Квартили
            sb.AppendLine("┌─ КВАРТИЛИ И РАЗМАХ ──────────────────────────────────────────┐");
            sb.AppendLine($"│  Q₁ (нижний квартиль)   = {QuantileQ1,15}                         │");
            sb.AppendLine($"│  Q₃ (верхний квартиль)  = {QuantileQ3,15}                         │");
            sb.AppendLine($"│  IQR (межквартильный)   = {InterquartileRange,15:F8}                    │");
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
            sb.AppendLine();

            // Диапазон значений
            sb.AppendLine("┌─ ДИАПАЗОН ЗНАЧЕНИЙ ──────────────────────────────────────────┐");
            sb.AppendLine($"│  Min (X)                = {MinValue,15}                         │");
            sb.AppendLine($"│  Max (X)                = {MaxValue,15}                         │");
            sb.AppendLine($"│  Range = Max - Min      = {Range,15}                         │");
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
            sb.AppendLine();

            // Коэффициент вариации
            sb.AppendLine("┌─ ВАРИАТИВНОСТЬ ──────────────────────────────────────────────┐");
            sb.AppendLine($"│  v(X) = σ/|M(X)|·100%   = {RelativeStandardDeviation,15:F2}%                       │");
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");

            return sb.ToString();
        }

        public override string ToString() => GetFormattedReport();
    }
}
