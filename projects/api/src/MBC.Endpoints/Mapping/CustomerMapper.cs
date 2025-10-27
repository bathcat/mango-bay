using MBC.Core.Entities;
using MBC.Endpoints.Dtos;

namespace MBC.Endpoints.Mapping;

public class CustomerMapper : IMapper<Customer, CustomerDto>
{
    public CustomerDto Map(Customer source)
    {
        return new CustomerDto
        {
            Id = source.Id,
            Nickname = source.Nickname
        };
    }
}

