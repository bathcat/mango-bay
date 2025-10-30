using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints;
using MBC.Endpoints.Mapping;
using Moq;
using Xunit;

namespace MBC.Endpoints.Tests;

public class DeliveryEndpointsTests
{
    private static Delivery CreateDelivery()
    {
        return new Delivery
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            PilotId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            Details = new JobDetails
            {
                OriginId = Guid.NewGuid(),
                DestinationId = Guid.NewGuid(),
                CargoDescription = "Test cargo",
                CargoWeightKg = 42,
                ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.Date)
            },
            CompletedOn = null,
            Status = DeliveryStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static DeliveryDto CreateDeliveryDto(Guid? id = null)
    {
        var deliveryId = id ?? Guid.NewGuid();
        return new DeliveryDto
        {
            Id = deliveryId,
            CustomerId = Guid.NewGuid(),
            PilotId = Guid.NewGuid(),
            OriginId = Guid.NewGuid(),
            DestinationId = Guid.NewGuid(),
            ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            CompletedOn = null,
            Status = DeliveryStatus.Confirmed,
            CargoDescription = "Test cargo",
            CargoWeightKg = 42,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PaymentId = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task BookDelivery_Success_ReturnsCreated()
    {
        var delivery = CreateDelivery();
        var deliveryDto = CreateDeliveryDto(delivery.Id);

        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.Book(It.IsAny<DeliveryRequest>())).ReturnsAsync(delivery);

        var mockDeliveryMapper = new Mock<IMapper<Delivery, DeliveryDto>>();
        mockDeliveryMapper.Setup(x => x.Map(delivery)).Returns(deliveryDto);

        var mockRequestMapper = new Mock<IMapper<DeliveryRequestDto, DeliveryRequest>>();
        mockRequestMapper.Setup(x => x.Map(It.IsAny<DeliveryRequestDto>())).Returns(new DeliveryRequest());

        var requestDto = new DeliveryRequestDto
        {
            PilotId = Guid.NewGuid(),
            Details = new JobDetails(),
            CreditCard = new CreditCardInfo { CardNumber = "1211111111111111", Cvc = "123", CardholderName = "Test", Expiration = DateOnly.FromDateTime(DateTime.UtcNow.Date) }
        };

        var result = await DeliveryEndpoints.BookDelivery(
            mockService.Object,
            mockDeliveryMapper.Object,
            mockRequestMapper.Object,
            requestDto);

        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(deliveryDto, result.Value);
    }

    [Fact]
    public async Task GetDelivery_WhenFound_ReturnsOkWithDto()
    {
        var id = Guid.NewGuid();
        var delivery = CreateDelivery();
        delivery.Id = id;
        var dto = CreateDeliveryDto(id);

        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.GetById(id)).ReturnsAsync(delivery);

        var mockMapper = new Mock<IMapper<Delivery, DeliveryDto>>();
        mockMapper.Setup(x => x.Map(delivery)).Returns(dto);

        var result = await DeliveryEndpoints.GetDelivery(
            mockService.Object,
            mockMapper.Object,
            id);

        var ok = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<DeliveryDto>>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task GetDelivery_WhenNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.GetById(id)).ReturnsAsync((Delivery?)null);

        var mockMapper = new Mock<IMapper<Delivery, DeliveryDto>>();

        var result = await DeliveryEndpoints.GetDelivery(
            mockService.Object,
            mockMapper.Object,
            id);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
    }

    [Fact]
    public async Task GetDeliveriesForCurrentCustomer_PassesCorrectPaginationParameters()
    {
        var skip = 5;
        var take = 15;

        var page = Page.Create(
            items: new[] { CreateDelivery() },
            offset: 0,
            countRequested: take,
            totalCount: 1);

        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.GetByCurrentCustomer(skip, take)).ReturnsAsync(page);

        var mockMapper = new Mock<IMapper<Delivery, DeliveryDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<Delivery>())).Returns(CreateDeliveryDto());

        var result = await DeliveryEndpoints.GetDeliveriesForCurrentCustomer(
            mockService.Object,
            mockMapper.Object,
            skip,
            take);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        mockService.Verify(x => x.GetByCurrentCustomer(skip, take), Times.Once);
    }

    [Fact]
    public async Task GetDeliveriesForCurrentPilot_PassesCorrectPaginationParameters()
    {
        var skip = 2;
        var take = 20;

        var page = Page.Create(
            items: new[] { CreateDelivery(), CreateDelivery() },
            offset: 0,
            countRequested: take,
            totalCount: 2);

        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.GetByCurrentPilot(skip, take)).ReturnsAsync(page);

        var mockMapper = new Mock<IMapper<Delivery, DeliveryDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<Delivery>())).Returns(CreateDeliveryDto());

        var result = await DeliveryEndpoints.GetDeliveriesForCurrentPilot(
            mockService.Object,
            mockMapper.Object,
            skip,
            take);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        mockService.Verify(x => x.GetByCurrentPilot(skip, take), Times.Once);
    }

    [Fact]
    public async Task GetDeliveriesByPilot_PassesCorrectPaginationParameters()
    {
        var pilotId = Guid.NewGuid();
        var skip = 0;
        var take = 10;

        var page = Page.Create(
            items: new[] { CreateDelivery() },
            offset: 0,
            countRequested: take,
            totalCount: 1);

        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.GetByPilot(pilotId, skip, take)).ReturnsAsync(page);

        var mockMapper = new Mock<IMapper<Delivery, DeliveryDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<Delivery>())).Returns(CreateDeliveryDto());

        var result = await DeliveryEndpoints.GetDeliveriesByPilot(
            mockService.Object,
            mockMapper.Object,
            pilotId,
            skip,
            take);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        mockService.Verify(x => x.GetByPilot(pilotId, skip, take), Times.Once);
    }

    [Fact]
    public async Task CancelDelivery_Success_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.Cancel(id)).Returns(Task.CompletedTask);

        var result = await DeliveryEndpoints.CancelDelivery(
            mockService.Object,
            id);

        Assert.Equal(204, result.StatusCode);
        mockService.Verify(x => x.Cancel(id), Times.Once);
    }

    [Fact]
    public async Task UpdateDeliveryStatus_Success_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var status = DeliveryStatus.InTransit;
        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.UpdateStatus(id, status)).Returns(Task.CompletedTask);

        var result = await DeliveryEndpoints.UpdateDeliveryStatus(
            mockService.Object,
            id,
            new UpdateDeliveryStatusRequest { Status = status });

        Assert.Equal(204, result.StatusCode);
        mockService.Verify(x => x.UpdateStatus(id, status), Times.Once);
    }

    [Fact]
    public async Task CalculateCost_Success_ReturnsOk()
    {
        var estimate = new CostEstimate { TotalCost = 100, BaseRate = 10, Distance = 50 };
        var details = new JobDetails
        {
            OriginId = Guid.NewGuid(),
            DestinationId = Guid.NewGuid(),
            CargoDescription = "Widgets",
            CargoWeightKg = 10,
            ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.Date)
        };

        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.CalculateCost(details)).Returns(new ValueTask<CostEstimate>(estimate));

        var result = await DeliveryEndpoints.CalculateCost(
            mockService.Object,
            details);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(estimate, result.Value);
    }

    [Fact]
    public async Task SearchDeliveries_PassesCorrectParameters()
    {
        var searchTerm = "fragile";
        var skip = 3;
        var take = 7;

        var page = Page.Create(
            items: new[] { CreateDelivery() },
            offset: 0,
            countRequested: take,
            totalCount: 1);

        var mockService = new Mock<IDeliveryService>();
        mockService.Setup(x => x.SearchByCargoDescription(searchTerm, skip, take)).ReturnsAsync(page);

        var mockMapper = new Mock<IMapper<Delivery, DeliveryDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<Delivery>())).Returns(CreateDeliveryDto());

        var result = await DeliveryEndpoints.SearchDeliveries(
            mockService.Object,
            mockMapper.Object,
            searchTerm,
            skip,
            take);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        mockService.Verify(x => x.SearchByCargoDescription(searchTerm, skip, take), Times.Once);
    }
}


