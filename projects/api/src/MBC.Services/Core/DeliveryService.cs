using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Authorization;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Models.Payment;
using MBC.Core.Persistence;
using MBC.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MBC.Services.Core;

public class DeliveryService : IDeliveryService
{
    private const int MaxDailyBookingsPerPilot = 5;

    private readonly IDeliveryStore _deliveryStore;
    private readonly IPaymentStore _paymentStore;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly IPilotStore _pilotStore;
    private readonly ISiteStore _siteStore;
    private readonly IMbcAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeliveryService> _logger;
    private readonly IOptions<CostCalculationOptions> _costOptions;

    public DeliveryService(
        IDeliveryStore deliveryStore,
        IPaymentStore paymentStore,
        IPaymentProcessor paymentProcessor,
        IPilotStore pilotStore,
        ISiteStore siteStore,
        IMbcAuthorizationService authorizationService,
        ICurrentUser currentUser,
        ILogger<DeliveryService> logger,
        IOptions<CostCalculationOptions> costOptions)
    {
        _deliveryStore = deliveryStore;
        _paymentStore = paymentStore;
        _paymentProcessor = paymentProcessor;
        _pilotStore = pilotStore;
        _siteStore = siteStore;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
        _logger = logger;
        _costOptions = costOptions;
    }

    /// <summary></summary>
    /// <remarks>
    /// Hybrid authorization approach: role-based check at service layer ensures only Customers and
    /// Administrators can book, then validates customers can only book for themselves (admins can
    /// book for anyone).
    ///
    /// If we end up doing this multiple times, we should build some notion of delegation into
    /// IMbcAuthorizationService.
    /// </remarks>
    public async Task<Delivery> Book(DeliveryRequest request)
    {
        _authorizationService.ThrowIfUnauthorized([UserRoles.Customer, UserRoles.Administrator]);

        if (!_currentUser.User.IsInRole(UserRoles.Administrator) &&
           _currentUser.CustomerId != request.CustomerId)
        {
            throw new UnauthorizedAccessException("Customers can only book deliveries for themselves.");
        }

        _logger.LogInformation(
            "Booking delivery for customer {CustomerId} with pilot {PilotId} scheduled for {ScheduledFor}",
            request.CustomerId,
            request.PilotId,
            request.Details.ScheduledFor);

        var pilot = await _pilotStore.GetById(request.PilotId);
        if (pilot == null)
        {
            _logger.LogWarning("Pilot {PilotId} not found", request.PilotId);
            throw new InvalidOperationException($"Pilot with ID {request.PilotId} not found.");
        }

        var isFullyBooked = await IsFullyBooked(request.PilotId, request.Details.ScheduledFor);
        if (isFullyBooked)
        {
            _logger.LogWarning(
                "Pilot {PilotId} is fully booked on {ScheduledFor}",
                request.PilotId,
                request.Details.ScheduledFor);
            throw new InvalidOperationException($"Pilot {request.PilotId} is fully booked on {request.Details.ScheduledFor}.");
        }

        var costEstimate = await CalculateCost(request.Details);
        _logger.LogInformation(
            "Calculated cost for delivery: {TotalCost} (Base: {BaseRate}, Distance: {DistanceCost}, Weight: {WeightCost}, Rush: {RushFee})",
            costEstimate.TotalCost,
            costEstimate.BaseRate,
            costEstimate.DistanceCost,
            costEstimate.WeightCost,
            costEstimate.RushFee);

        var paymentRequest = new PaymentRequest
        {
            MerchantReference = Guid.NewGuid().ToString(),
            Amount = costEstimate.TotalCost,
            CreditCard = request.CreditCard
        };

        _logger.LogDebug("Processing payment of {Amount} for customer {CustomerId} with merchant reference {MerchantReference}", costEstimate.TotalCost, request.CustomerId, paymentRequest.MerchantReference);
        var paymentResult = await _paymentProcessor.ProcessPayment(paymentRequest);
        if (!paymentResult.Success)
        {
            _logger.LogWarning(
                "Payment failed for customer {CustomerId}: {ErrorMessage}",
                request.CustomerId,
                paymentResult.ErrorMessage);
            throw new InvalidOperationException($"Payment failed: {paymentResult.ErrorMessage}");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            DeliveryId = Guid.Empty,
            Amount = costEstimate.TotalCost,
            Status = PaymentStatus.Completed,
            MerchantReference = paymentResult.MerchantReference,
            TransactionId = paymentResult.TransactionId,
            CreditCard = request.CreditCard,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var delivery = new Delivery
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            PilotId = request.PilotId,
            Details = request.Details,
            Status = DeliveryStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PaymentId = payment.Id
        };

        var createdDelivery = await _deliveryStore.Create(delivery);

        payment.DeliveryId = createdDelivery.Id;
        await _paymentStore.Create(payment);

        _logger.LogInformation(
            "Successfully booked delivery {DeliveryId} for customer {CustomerId} with transaction {TransactionId}",
            createdDelivery.Id,
            request.CustomerId,
            paymentResult.TransactionId);

        return createdDelivery;
    }

    public async Task Cancel(Guid deliveryId)
    {
        _logger.LogInformation("Cancelling delivery {DeliveryId}", deliveryId);

        var delivery = await _deliveryStore.GetById(deliveryId);
        if (delivery == null)
        {
            _logger.LogWarning("Delivery {DeliveryId} not found", deliveryId);
            throw new InvalidOperationException($"Delivery with ID {deliveryId} not found.");
        }

        _authorizationService.ThrowIfUnauthorized(DeliveryOperations.Update, delivery);

        if (delivery.Status == DeliveryStatus.Cancelled)
        {
            _logger.LogWarning("Delivery {DeliveryId} is already cancelled", deliveryId);
            throw new InvalidOperationException($"Delivery {deliveryId} is already cancelled.");
        }

        if (delivery.Status == DeliveryStatus.Delivered)
        {
            _logger.LogWarning("Attempted to cancel delivered shipment {DeliveryId}", deliveryId);
            throw new InvalidOperationException($"Cannot cancel a delivered shipment.");
        }

        var payment = await _paymentStore.GetByDeliveryId(deliveryId);
        if (payment != null && payment.Status == PaymentStatus.Completed)
        {
            _logger.LogDebug(
                "Processing refund for delivery {DeliveryId}, transaction {TransactionId}",
                deliveryId,
                payment.TransactionId);

            var refundResult = await _paymentProcessor.RefundPayment(payment.TransactionId, payment.Amount);
            if (!refundResult.Success)
            {
                _logger.LogWarning(
                    "Refund failed for delivery {DeliveryId}: {ErrorMessage}",
                    deliveryId,
                    refundResult.ErrorMessage);
                throw new InvalidOperationException($"Refund failed: {refundResult.ErrorMessage}");
            }

            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentStore.Update(payment);
        }

        delivery.Status = DeliveryStatus.Cancelled;
        delivery.UpdatedAt = DateTime.UtcNow;
        await _deliveryStore.Update(delivery);

        _logger.LogInformation("Successfully cancelled delivery {DeliveryId}", deliveryId);
    }

    public async Task UpdateStatus(Guid deliveryId, DeliveryStatus newStatus)
    {
        _logger.LogInformation("Updating delivery {DeliveryId} status to {NewStatus}", deliveryId, newStatus);

        var delivery = await _deliveryStore.GetById(deliveryId);
        if (delivery == null)
        {
            _logger.LogWarning("Delivery {DeliveryId} not found", deliveryId);
            throw new InvalidOperationException($"Delivery with ID {deliveryId} not found.");
        }

        _authorizationService.ThrowIfUnauthorized(DeliveryOperations.Update, delivery);

        if (delivery.Status == DeliveryStatus.Cancelled)
        {
            _logger.LogWarning("Attempted to update status of cancelled delivery {DeliveryId}", deliveryId);
            throw new InvalidOperationException($"Cannot update status of a cancelled delivery.");
        }

        delivery.Status = newStatus;
        delivery.UpdatedAt = DateTime.UtcNow;

        if (newStatus == DeliveryStatus.Delivered)
        {
            delivery.CompletedOn = DateTime.UtcNow;
            _logger.LogInformation("Delivery {DeliveryId} marked as delivered", deliveryId);
        }

        await _deliveryStore.Update(delivery);
    }

    public async Task<Delivery?> GetById(Guid deliveryId)
    {
        var delivery = await _deliveryStore.GetById(deliveryId);
        if (delivery != null)
        {
            _authorizationService.ThrowIfUnauthorized(DeliveryOperations.Read, delivery);
        }
        return delivery;
    }

    public async Task<Page<Delivery>> GetByCurrentCustomer(int skip, int take)
    {
        _authorizationService.ThrowIfUnauthorized([UserRoles.Customer]);

        var customerId = _currentUser.CustomerId
            ?? throw new InvalidOperationException("Customer ID not found for current user");

        return await _deliveryStore.GetByCustomerId(customerId, skip, take);
    }

    public async Task<Page<Delivery>> GetByCurrentPilot(int skip, int take)
    {
        _authorizationService.ThrowIfUnauthorized([UserRoles.Pilot]);

        var pilotId = _currentUser.PilotId
            ?? throw new InvalidOperationException("Pilot ID not found for current user");

        return await _deliveryStore.GetByPilotId(pilotId, skip, take);
    }

    public async Task<Page<Delivery>> GetByCustomer(Guid customerId, int skip, int take)
    {
        _authorizationService.ThrowIfUnauthorized([UserRoles.Administrator]);
        return await _deliveryStore.GetByCustomerId(customerId, skip, take);
    }

    public async Task<Page<Delivery>> GetByPilot(Guid pilotId, int skip, int take)
    {
        _authorizationService.ThrowIfUnauthorized([UserRoles.Administrator]);
        return await _deliveryStore.GetByPilotId(pilotId, skip, take);
    }

    public async Task<bool> IsFullyBooked(Guid pilotId, DateOnly date)
    {
        _authorizationService.ThrowIfUnauthorized([UserRoles.Customer, UserRoles.Administrator]);

        if (date < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            _logger.LogWarning("Attempted to check if pilot {PilotId} is fully booked for a past date {Date}", pilotId, date);
            return true;
        }

        var deliveries = await _deliveryStore.GetByPilotId(pilotId, 0, int.MaxValue);
        var bookingsOnDate = deliveries.Items.Count(d =>
            d.Details.ScheduledFor == date &&
            d.Status != DeliveryStatus.Cancelled);

        _logger.LogDebug(
            "Pilot {PilotId} has {BookingCount} bookings on {Date}",
            pilotId,
            bookingsOnDate,
            date);

        return bookingsOnDate >= MaxDailyBookingsPerPilot;
    }

    public async ValueTask<CostEstimate> CalculateCost(JobDetails details)
    {
        _authorizationService.ThrowIfUnauthorized([UserRoles.Customer, UserRoles.Administrator]);

        _logger.LogInformation(
            "Calculating cost for route {OriginId} -> {DestinationId}, weight {Weight}kg, scheduled for {ScheduledFor}",
            details.OriginId,
            details.DestinationId,
            details.CargoWeightKg,
            details.ScheduledFor);

        var origin = await _siteStore.GetById(details.OriginId);
        if (origin == null)
        {
            _logger.LogWarning("Origin site {OriginId} not found", details.OriginId);
            throw new InvalidOperationException($"Origin site with ID {details.OriginId} not found.");
        }

        var destination = await _siteStore.GetById(details.DestinationId);
        if (destination == null)
        {
            _logger.LogWarning("Destination site {DestinationId} not found", details.DestinationId);
            throw new InvalidOperationException($"Destination site with ID {details.DestinationId} not found.");
        }

        var options = _costOptions.Value;

        var deltaX = destination.Location.X - origin.Location.X;
        var deltaY = destination.Location.Y - origin.Location.Y;
        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        var distanceCost = (decimal)distance * options.DistanceRatePerUnit;
        var weightCost = details.CargoWeightKg * options.WeightRatePerKg;

        var daysUntilScheduled = details.ScheduledFor.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
        var rushFee = daysUntilScheduled < options.RushThresholdDays ? options.RushFee : 0m;

        var totalCost = options.BaseRate + distanceCost + weightCost + rushFee;

        var estimate = new CostEstimate
        {
            TotalCost = Math.Round(totalCost, 2),
            BaseRate = options.BaseRate,
            DistanceCost = Math.Round(distanceCost, 2),
            WeightCost = Math.Round(weightCost, 2),
            RushFee = rushFee,
            Distance = Math.Round((decimal)distance, 2)
        };

        _logger.LogInformation(
            "Cost estimate: Total={TotalCost}, Distance={Distance} leagues, Rush={IsRush}",
            estimate.TotalCost,
            estimate.Distance,
            rushFee > 0);

        return estimate;
    }

    public async Task<Page<Delivery>> SearchByCargoDescription(string searchTerm, int skip, int take)
    {
        _authorizationService.ThrowIfUnauthorized([UserRoles.Customer]);

        var customerId = _currentUser.CustomerId
            ?? throw new InvalidOperationException("Customer ID not found for current user");

        _logger.LogInformation(
            "Searching deliveries for customer {CustomerId} with search term: {SearchTerm}",
            customerId,
            searchTerm);

        return await _deliveryStore.SearchByCargoDescription(customerId, searchTerm, skip, take);
    }
}


