using MBC.Core.Entities;
using MBC.Endpoints.Dtos;

namespace MBC.Endpoints.Mapping;

public class PaymentMapper : IMapper<Payment, PaymentDto>
{
    public PaymentDto Map(Payment source)
    {
        return new PaymentDto
        {
            Id = source.Id,
            DeliveryId = source.DeliveryId,
            Amount = source.Amount,
            Status = source.Status,
            TransactionId = source.TransactionId,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }
}

