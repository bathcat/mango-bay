using System;

namespace MBC.Core.Models;

public sealed class JobDetails
{
    public Guid OriginId { get; set; }
    public Guid DestinationId { get; set; }
    public string CargoDescription { get; set; } = string.Empty;
    public decimal CargoWeightKg { get; set; }
    public DateOnly ScheduledFor { get; set; }
}

