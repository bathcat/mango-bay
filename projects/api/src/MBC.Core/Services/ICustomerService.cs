using System;
using System.Threading.Tasks;
using MBC.Core.Entities;

namespace MBC.Core.Services;

public interface ICustomerService
{
    Task<Customer> UpdateCustomer(Guid customerId, string nickname);

    Task<Customer?> GetCustomerById(Guid customerId);

    Task<Customer?> GetCustomerByUserId(Guid userId);
}

