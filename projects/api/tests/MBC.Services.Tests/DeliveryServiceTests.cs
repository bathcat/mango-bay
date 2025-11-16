using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Authorization;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Models.Payment;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Services.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MBC.Services.Tests;


public class DeliveryServiceTests
{
    [Fact]
    public async Task Book_WhenCustomerBooksForThemselves_CreatesDeliverySuccessfully()
    {
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();

        var request = CreateDeliveryRequest(customerId, pilotId, originId, destinationId);
        var pilot = CreatePilot(pilotId);
        var origin = CreateSite(originId, 10, 20);
        var destination = CreateSite(destinationId, 50, 60);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, customerId, null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }));
        mockPilotStore.Setup(x => x.GetById(pilotId)).ReturnsAsync(pilot);
        mockDeliveryStore.Setup(x => x.GetByPilotId(pilotId, 0, int.MaxValue))
            .ReturnsAsync(Page.Create(Array.Empty<Delivery>(), 0, int.MaxValue, 0));
        mockSiteStore.Setup(x => x.GetById(originId)).ReturnsAsync(origin);
        mockSiteStore.Setup(x => x.GetById(destinationId)).ReturnsAsync(destination);
        mockPaymentProcessor.Setup(x => x.ProcessPayment(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "TXN123", MerchantReference = "MR123" });
        mockDeliveryStore.Setup(x => x.Create(It.IsAny<Delivery>()))
            .ReturnsAsync((Delivery d) => d);
        mockPaymentStore.Setup(x => x.Create(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.Book(request);

        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(pilotId, result.PilotId);
        Assert.Equal(DeliveryStatus.Confirmed, result.Status);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Exactly(3));
    }

    [Fact]
    public async Task Book_WhenCustomerTriesToBookForDifferentCustomer_ThrowsUnauthorizedAccessException()
    {
        var currentCustomerId = Guid.NewGuid();
        var differentCustomerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();

        var request = CreateDeliveryRequest(differentCustomerId, pilotId, originId, destinationId);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, currentCustomerId, null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }));

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.Book(request));

        Assert.Contains("Customers can only book deliveries for themselves", exception.Message);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task Book_WhenAdministratorBooksForAnyCustomer_CreatesDeliverySuccessfully()
    {
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();

        var request = CreateDeliveryRequest(customerId, pilotId, originId, destinationId);
        var pilot = CreatePilot(pilotId);
        var origin = CreateSite(originId, 10, 20);
        var destination = CreateSite(destinationId, 50, 60);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Administrator, null, null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }));
        mockPilotStore.Setup(x => x.GetById(pilotId)).ReturnsAsync(pilot);
        mockDeliveryStore.Setup(x => x.GetByPilotId(pilotId, 0, int.MaxValue))
            .ReturnsAsync(Page.Create(Array.Empty<Delivery>(), 0, int.MaxValue, 0));
        mockSiteStore.Setup(x => x.GetById(originId)).ReturnsAsync(origin);
        mockSiteStore.Setup(x => x.GetById(destinationId)).ReturnsAsync(destination);
        mockPaymentProcessor.Setup(x => x.ProcessPayment(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "TXN123", MerchantReference = "MR123" });
        mockDeliveryStore.Setup(x => x.Create(It.IsAny<Delivery>()))
            .ReturnsAsync((Delivery d) => d);
        mockPaymentStore.Setup(x => x.Create(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.Book(request);

        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(DeliveryStatus.Confirmed, result.Status);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Exactly(3));
    }

    [Fact]
    public async Task Book_WhenPilotTriesToBook_ThrowsUnauthorizedAccessException()
    {
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();

        var request = CreateDeliveryRequest(customerId, pilotId, originId, destinationId);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Pilot, null, pilotId);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.Book(request));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task Cancel_WhenAuthorized_CancelsDeliveryAndRefundsPayment()
    {
        var deliveryId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, customerId, pilotId, DeliveryStatus.Confirmed);
        var payment = CreatePayment(deliveryId, 100m, PaymentStatus.Completed, "TXN123");

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, customerId, null);
        var costOptions = CreateCostOptions();

        mockDeliveryStore.Setup(x => x.GetById(deliveryId)).ReturnsAsync(delivery);
        mockAuthService.Setup(x => x.ThrowIfUnauthorized(DeliveryOperations.Update, delivery));
        mockPaymentStore.Setup(x => x.GetByDeliveryId(deliveryId)).ReturnsAsync(payment);
        mockPaymentProcessor.Setup(x => x.RefundPayment("TXN123", 100m))
            .ReturnsAsync(new RefundResult { Success = true });
        mockPaymentStore.Setup(x => x.Update(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);
        mockDeliveryStore.Setup(x => x.Update(It.IsAny<Delivery>()))
            .ReturnsAsync((Delivery d) => d);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await service.Cancel(deliveryId);

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(DeliveryOperations.Update, delivery), Times.Once);
        mockPaymentProcessor.Verify(x => x.RefundPayment("TXN123", 100m), Times.Once);
        mockDeliveryStore.Verify(x => x.Update(It.Is<Delivery>(d => d.Status == DeliveryStatus.Cancelled)), Times.Once);
    }

    [Fact]
    public async Task Cancel_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
    {
        var deliveryId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, customerId, pilotId, DeliveryStatus.Confirmed);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, Guid.NewGuid(), null);
        var costOptions = CreateCostOptions();

        mockDeliveryStore.Setup(x => x.GetById(deliveryId)).ReturnsAsync(delivery);
        mockAuthService.Setup(x => x.ThrowIfUnauthorized(DeliveryOperations.Update, delivery))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.Cancel(deliveryId));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(DeliveryOperations.Update, delivery), Times.Once);
        mockPaymentProcessor.Verify(x => x.RefundPayment(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_WhenAuthorized_UpdatesDeliveryStatus()
    {
        var deliveryId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, customerId, pilotId, DeliveryStatus.Confirmed);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Pilot, null, pilotId);
        var costOptions = CreateCostOptions();

        mockDeliveryStore.Setup(x => x.GetById(deliveryId)).ReturnsAsync(delivery);
        mockAuthService.Setup(x => x.ThrowIfUnauthorized(DeliveryOperations.Update, delivery));
        mockDeliveryStore.Setup(x => x.Update(It.IsAny<Delivery>()))
            .ReturnsAsync((Delivery d) => d);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await service.UpdateStatus(deliveryId, DeliveryStatus.InTransit);

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(DeliveryOperations.Update, delivery), Times.Once);
        mockDeliveryStore.Verify(x => x.Update(It.Is<Delivery>(d => d.Status == DeliveryStatus.InTransit)), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
    {
        var deliveryId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, customerId, pilotId, DeliveryStatus.Confirmed);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, Guid.NewGuid(), null);
        var costOptions = CreateCostOptions();

        mockDeliveryStore.Setup(x => x.GetById(deliveryId)).ReturnsAsync(delivery);
        mockAuthService.Setup(x => x.ThrowIfUnauthorized(DeliveryOperations.Update, delivery))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateStatus(deliveryId, DeliveryStatus.InTransit));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(DeliveryOperations.Update, delivery), Times.Once);
        mockDeliveryStore.Verify(x => x.Update(It.IsAny<Delivery>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WhenAuthorized_ReturnsDelivery()
    {
        var deliveryId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, customerId, pilotId, DeliveryStatus.Confirmed);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, customerId, null);
        var costOptions = CreateCostOptions();

        mockDeliveryStore.Setup(x => x.GetById(deliveryId)).ReturnsAsync(delivery);
        mockAuthService.Setup(x => x.ThrowIfUnauthorized(DeliveryOperations.Read, delivery));

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.GetById(deliveryId);

        Assert.NotNull(result);
        Assert.Equal(deliveryId, result.Id);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(DeliveryOperations.Read, delivery), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
    {
        var deliveryId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, customerId, pilotId, DeliveryStatus.Confirmed);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, Guid.NewGuid(), null);
        var costOptions = CreateCostOptions();

        mockDeliveryStore.Setup(x => x.GetById(deliveryId)).ReturnsAsync(delivery);
        mockAuthService.Setup(x => x.ThrowIfUnauthorized(DeliveryOperations.Read, delivery))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetById(deliveryId));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(DeliveryOperations.Read, delivery), Times.Once);
    }

    [Fact]
    public async Task GetByCurrentCustomer_WhenCustomerRole_ReturnsDeliveries()
    {
        var customerId = Guid.NewGuid();
        var deliveries = new[]
        {
            CreateDelivery(Guid.NewGuid(), customerId, Guid.NewGuid(), DeliveryStatus.Confirmed),
            CreateDelivery(Guid.NewGuid(), customerId, Guid.NewGuid(), DeliveryStatus.Delivered)
        };
        var page = Page.Create(deliveries, 0, 10, 2);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, customerId, null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer }));
        mockDeliveryStore.Setup(x => x.GetByCustomerId(customerId, 0, 10)).ReturnsAsync(page);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.GetByCurrentCustomer(0, 10);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer }), Times.Once);
    }

    [Fact]
    public async Task GetByCurrentCustomer_WhenNotCustomerRole_ThrowsUnauthorizedAccessException()
    {
        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Pilot, null, Guid.NewGuid());
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer }))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetByCurrentCustomer(0, 10));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer }), Times.Once);
    }

    [Fact]
    public async Task GetByCurrentPilot_WhenPilotRole_ReturnsDeliveries()
    {
        var pilotId = Guid.NewGuid();
        var deliveries = new[]
        {
            CreateDelivery(Guid.NewGuid(), Guid.NewGuid(), pilotId, DeliveryStatus.Confirmed),
            CreateDelivery(Guid.NewGuid(), Guid.NewGuid(), pilotId, DeliveryStatus.InTransit)
        };
        var page = Page.Create(deliveries, 0, 10, 2);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Pilot, null, pilotId);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Pilot }));
        mockDeliveryStore.Setup(x => x.GetByPilotId(pilotId, 0, 10)).ReturnsAsync(page);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.GetByCurrentPilot(0, 10);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Pilot }), Times.Once);
    }

    [Fact]
    public async Task GetByCurrentPilot_WhenNotPilotRole_ThrowsUnauthorizedAccessException()
    {
        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, Guid.NewGuid(), null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Pilot }))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetByCurrentPilot(0, 10));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Pilot }), Times.Once);
    }

    [Fact]
    public async Task GetByCustomer_WhenAdministratorRole_ReturnsDeliveries()
    {
        var customerId = Guid.NewGuid();
        var deliveries = new[]
        {
            CreateDelivery(Guid.NewGuid(), customerId, Guid.NewGuid(), DeliveryStatus.Confirmed)
        };
        var page = Page.Create(deliveries, 0, 10, 1);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Administrator, null, null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Administrator }));
        mockDeliveryStore.Setup(x => x.GetByCustomerId(customerId, 0, 10)).ReturnsAsync(page);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.GetByCustomer(customerId, 0, 10);

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task GetByCustomer_WhenNotAdministratorRole_ThrowsUnauthorizedAccessException()
    {
        var customerId = Guid.NewGuid();

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, Guid.NewGuid(), null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Administrator }))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetByCustomer(customerId, 0, 10));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task GetByPilot_WhenAdministratorRole_ReturnsDeliveries()
    {
        var pilotId = Guid.NewGuid();
        var deliveries = new[]
        {
            CreateDelivery(Guid.NewGuid(), Guid.NewGuid(), pilotId, DeliveryStatus.Confirmed)
        };
        var page = Page.Create(deliveries, 0, 10, 1);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Administrator, null, null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Administrator }));
        mockDeliveryStore.Setup(x => x.GetByPilotId(pilotId, 0, 10)).ReturnsAsync(page);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.GetByPilot(pilotId, 0, 10);

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task IsFullyBooked_WhenLessThanMaxBookings_ReturnsFalse()
    {
        var pilotId = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var deliveries = new[]
        {
            CreateDeliveryWithScheduledDate(Guid.NewGuid(), Guid.NewGuid(), pilotId, DeliveryStatus.Confirmed, futureDate),
            CreateDeliveryWithScheduledDate(Guid.NewGuid(), Guid.NewGuid(), pilotId, DeliveryStatus.Confirmed, futureDate)
        };
        var page = Page.Create(deliveries, 0, int.MaxValue, 2);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, Guid.NewGuid(), null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }));
        mockDeliveryStore.Setup(x => x.GetByPilotId(pilotId, 0, int.MaxValue)).ReturnsAsync(page);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.IsFullyBooked(pilotId, futureDate);

        Assert.False(result);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task IsFullyBooked_WhenPastDate_ReturnsTrue()
    {
        var pilotId = Guid.NewGuid();
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, Guid.NewGuid(), null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }));

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.IsFullyBooked(pilotId, pastDate);

        Assert.True(result);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task IsFullyBooked_WhenUnauthorizedRole_ThrowsUnauthorizedAccessException()
    {
        var pilotId = Guid.NewGuid();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Pilot, null, pilotId);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.IsFullyBooked(pilotId, futureDate));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task CalculateCost_WhenAuthorized_ReturnsCostEstimate()
    {
        var originId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();
        var origin = CreateSite(originId, 10, 20);
        var destination = CreateSite(destinationId, 50, 60);

        var jobDetails = new JobDetails
        {
            OriginId = originId,
            DestinationId = destinationId,
            CargoWeightKg = 100m,
            ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, Guid.NewGuid(), null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }));
        mockSiteStore.Setup(x => x.GetById(originId)).ReturnsAsync(origin);
        mockSiteStore.Setup(x => x.GetById(destinationId)).ReturnsAsync(destination);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.CalculateCost(jobDetails);

        Assert.NotNull(result);
        Assert.True(result.TotalCost > 0);
        Assert.Equal(50.00m, result.BaseRate);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task CalculateCost_WhenUnauthorizedRole_ThrowsUnauthorizedAccessException()
    {
        var originId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();

        var jobDetails = new JobDetails
        {
            OriginId = originId,
            DestinationId = destinationId,
            CargoWeightKg = 100m,
            ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Pilot, null, Guid.NewGuid());
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await service.CalculateCost(jobDetails));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer, UserRoles.Administrator }), Times.Once);
    }

    [Fact]
    public async Task SearchByCargoDescription_WhenCustomerRole_ReturnsMatchingDeliveries()
    {
        var customerId = Guid.NewGuid();
        var deliveries = new[]
        {
            CreateDelivery(Guid.NewGuid(), customerId, Guid.NewGuid(), DeliveryStatus.Confirmed)
        };
        var page = Page.Create(deliveries, 0, 10, 1);

        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, customerId, null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer }));
        mockDeliveryStore.Setup(x => x.SearchByCargoDescription(customerId, "test", 0, 10)).ReturnsAsync(page);

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        var result = await service.SearchByCargoDescription("test", 0, 10);

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer }), Times.Once);
    }

    [Fact]
    public async Task SearchByCargoDescription_WhenNotCustomerRole_ThrowsUnauthorizedAccessException()
    {
        var mockDeliveryStore = new Mock<IDeliveryStore>();
        var mockPaymentStore = new Mock<IPaymentStore>();
        var mockPaymentProcessor = new Mock<IPaymentProcessor>();
        var mockPilotStore = new Mock<IPilotStore>();
        var mockSiteStore = new Mock<ISiteStore>();
        var mockAuthService = new Mock<IMbcAuthorizationService>();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Administrator, null, null);
        var costOptions = CreateCostOptions();

        mockAuthService.Setup(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer }))
            .Throws<UnauthorizedAccessException>();

        var service = CreateDeliveryService(
            mockDeliveryStore.Object,
            mockPaymentStore.Object,
            mockPaymentProcessor.Object,
            mockPilotStore.Object,
            mockSiteStore.Object,
            mockAuthService.Object,
            mockCurrentUser.Object,
            costOptions);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SearchByCargoDescription("test", 0, 10));

        mockAuthService.Verify(x => x.ThrowIfUnauthorized(new[] { UserRoles.Customer }), Times.Once);
    }

    private static DeliveryService CreateDeliveryService(
        IDeliveryStore deliveryStore,
        IPaymentStore paymentStore,
        IPaymentProcessor paymentProcessor,
        IPilotStore pilotStore,
        ISiteStore siteStore,
        IMbcAuthorizationService authorizationService,
        ICurrentUser currentUser,
        IOptions<CostCalculationOptions> costOptions)
    {
        return new DeliveryService(
            deliveryStore,
            paymentStore,
            paymentProcessor,
            pilotStore,
            siteStore,
            authorizationService,
            currentUser,
            NullLogger<DeliveryService>.Instance,
            costOptions);
    }

    private static Mock<ICurrentUser> CreateMockCurrentUser(string role, Guid? customerId, Guid? pilotId)
    {
        var claims = new[] { new Claim(ClaimTypes.Role, role) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var mockCurrentUser = new Mock<ICurrentUser>();
        mockCurrentUser.Setup(x => x.User).Returns(principal);
        mockCurrentUser.Setup(x => x.CustomerId).Returns(customerId);
        mockCurrentUser.Setup(x => x.PilotId).Returns(pilotId);
        return mockCurrentUser;
    }

    private static IOptions<CostCalculationOptions> CreateCostOptions()
    {
        return Options.Create(new CostCalculationOptions
        {
            BaseRate = 50.00m,
            DistanceRatePerUnit = 2.00m,
            WeightRatePerKg = 5.00m,
            RushFee = 25.00m,
            RushThresholdDays = 5
        });
    }

    private static DeliveryRequest CreateDeliveryRequest(Guid customerId, Guid pilotId, Guid originId, Guid destinationId)
    {
        return new DeliveryRequest
        {
            CustomerId = customerId,
            PilotId = pilotId,
            Details = new JobDetails
            {
                OriginId = originId,
                DestinationId = destinationId,
                CargoDescription = "Test cargo",
                CargoWeightKg = 100m,
                ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
            },
            CreditCard = new CreditCardInfo
            {
                CardNumber = "4111111111111111",
                Expiration = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
                Cvc = "123",
                CardholderName = "Test User"
            }
        };
    }

    private static Pilot CreatePilot(Guid id)
    {
        return new Pilot
        {
            Id = id,
            FullName = "Test Pilot",
            ShortName = "TP",
            UserId = Guid.NewGuid()
        };
    }

    private static Site CreateSite(Guid id, byte x, byte y)
    {
        return new Site
        {
            Id = id,
            Name = "Test Site",
            Location = new Location { X = x, Y = y }
        };
    }

    private static Delivery CreateDelivery(Guid id, Guid customerId, Guid pilotId, DeliveryStatus status)
    {
        return new Delivery
        {
            Id = id,
            CustomerId = customerId,
            PilotId = pilotId,
            Status = status,
            Details = new JobDetails
            {
                OriginId = Guid.NewGuid(),
                DestinationId = Guid.NewGuid(),
                CargoDescription = "Test cargo",
                CargoWeightKg = 100m,
                ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Delivery CreateDeliveryWithScheduledDate(Guid id, Guid customerId, Guid pilotId, DeliveryStatus status, DateOnly scheduledDate)
    {
        return new Delivery
        {
            Id = id,
            CustomerId = customerId,
            PilotId = pilotId,
            Status = status,
            Details = new JobDetails
            {
                OriginId = Guid.NewGuid(),
                DestinationId = Guid.NewGuid(),
                CargoDescription = "Test cargo",
                CargoWeightKg = 100m,
                ScheduledFor = scheduledDate
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Payment CreatePayment(Guid deliveryId, decimal amount, PaymentStatus status, string transactionId)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            DeliveryId = deliveryId,
            Amount = amount,
            Status = status,
            TransactionId = transactionId,
            MerchantReference = "MR123",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

