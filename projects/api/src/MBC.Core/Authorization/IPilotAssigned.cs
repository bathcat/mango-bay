using System;

namespace MBC.Core.Authorization;

public interface IPilotAssigned
{
    Guid PilotId { get; }
}

