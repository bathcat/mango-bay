using System;

namespace MBC.Core.Authorization;

public interface ICustomerOwned
{
    Guid CustomerId { get; }
}

