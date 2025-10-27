using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.ValueObjects;

namespace MBC.Core.Services;

public interface IReviewService
{
    Task<DeliveryReview> CreateReview(Guid customerId, Guid deliveryId, Rating rating, string notes);

    Task<DeliveryReview> UpdateReview(Guid reviewId, Rating rating, string notes);

    Task DeleteReview(Guid reviewId);

    Task<DeliveryReview?> GetReviewById(Guid reviewId);

    Task<DeliveryReview?> GetReviewByDeliveryId(Guid deliveryId);

    Task<Page<DeliveryReview>> GetByPilotId(Guid pilotId, int skip, int take);
}

