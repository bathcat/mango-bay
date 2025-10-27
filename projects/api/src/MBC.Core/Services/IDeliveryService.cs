using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;

namespace MBC.Core.Services;

public interface IDeliveryService
{
    Task<Delivery> Book(DeliveryRequest request);

    Task Cancel(Guid deliveryId);

    Task UpdateStatus(Guid deliveryId, DeliveryStatus newStatus);

    Task<Delivery?> GetById(Guid deliveryId);

    Task<Page<Delivery>> GetByCurrentCustomer(int skip, int take);

    Task<Page<Delivery>> GetByCurrentPilot(int skip, int take);

    Task<Page<Delivery>> GetByCustomer(Guid customerId, int skip, int take);

    Task<Page<Delivery>> GetByPilot(Guid pilotId, int skip, int take);

    Task<bool> IsFullyBooked(Guid pilotId, DateOnly date);

    ValueTask<CostEstimate> CalculateCost(JobDetails details);

    Task<Page<Delivery>> SearchByCargoDescription(string searchTerm, int skip, int take);
}


