namespace MBC.Core.Models;

public sealed class CostCalculationOptions
{
    public const string SectionName = "CostCalculation";

    public decimal BaseRate { get; set; } = 50.00m;
    public decimal DistanceRatePerUnit { get; set; } = 2.00m;
    public decimal WeightRatePerKg { get; set; } = 5.00m;
    public decimal RushFee { get; set; } = 25.00m;
    public int RushThresholdDays { get; set; } = 5;
}

