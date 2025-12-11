namespace DiscreteRVAnalyzer.Distributions
{
    public interface IDistribution
    {
        string Name { get; }
        double CalculateProbability(int k); // P(X=k)
        double Mean { get; }               // M(X) - Математичне сподівання
        double Variance { get; }           // D(X) - Дисперсія
        double StandardDeviation { get; }  // sigma(X) - Сер. кв. відхилення
    }
}