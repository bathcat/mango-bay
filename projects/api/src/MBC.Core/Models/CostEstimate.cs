namespace MBC.Core.Models;

public sealed class CostEstimate
{
    public decimal TotalCost { get; set; }
    public decimal BaseRate { get; set; }
    public decimal DistanceCost { get; set; }
    public decimal WeightCost { get; set; }
    public decimal RushFee { get; set; }
    public decimal Distance { get; set; }
}

