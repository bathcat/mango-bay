using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints;
using MBC.Endpoints.Mapping;
using Moq;
using Xunit;

namespace MBC.Endpoints.Tests;

public class PilotEndpointsTests
{
    private static Pilot CreatePilot()
    {
        return new Pilot
        {
            Id = Guid.NewGuid(),
            FullName = "Test Pilot",
            ShortName = "Test",
            Bio = "Test bio",
            UserId = Guid.NewGuid()
        };
    }

    private static PilotDto CreatePilotDto()
    {
        return new PilotDto
        {
            Id = Guid.NewGuid(),
            FullName = "Test Pilot",
            ShortName = "Test",
            Bio = "Test bio"
        };
    }

    [Fact]
    public async Task GetPilots_PassesCorrectPaginationParameters()
    {
        var skip = 10;
        var take = 20;

        var pilots = Page.Create(
            items: [CreatePilot(), CreatePilot()],
            offset: 0,
            countRequested: 20,
            totalCount: 2
        );

        var mockPilotStore = new Mock<IPilotStore>();
        mockPilotStore.Setup(x => x.GetPage(skip, take)).ReturnsAsync(pilots);

        var mockMapper = new Mock<IMapper<Pilot, PilotDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<Pilot>())).Returns(CreatePilotDto());

        var result = await PilotEndpoints.GetPilots(
            mockPilotStore.Object,
            mockMapper.Object,
            skip,
            take);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        mockPilotStore.Verify(x => x.GetPage(skip, take), Times.Once);
    }

    [Fact]
    public async Task GetPilots_UsesDefaultPaginationWhenNotProvided()
    {
        var pilots = Page.Create(
            items: [CreatePilot()],
            offset: 0,
            countRequested: 10,
            totalCount: 1
        );

        var mockPilotStore = new Mock<IPilotStore>();
        mockPilotStore.Setup(x => x.GetPage(0, 10)).ReturnsAsync(pilots);

        var mockMapper = new Mock<IMapper<Pilot, PilotDto>>();
        mockMapper.Setup(x => x.Map(It.IsAny<Pilot>())).Returns(CreatePilotDto());

        var result = await PilotEndpoints.GetPilots(
            mockPilotStore.Object,
            mockMapper.Object);

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        mockPilotStore.Verify(x => x.GetPage(0, 10), Times.Once);
    }

    [Fact]
    public async Task GetPilot_WhenFound_ReturnsOkWithDto()
    {
        var pilotId = Guid.NewGuid();
        var pilot = CreatePilot();
        pilot.Id = pilotId;
        var pilotDto = CreatePilotDto();
        pilotDto = pilotDto with { Id = pilotId };

        var mockPilotStore = new Mock<IPilotStore>();
        mockPilotStore.Setup(x => x.GetById(pilotId)).ReturnsAsync(pilot);

        var mockMapper = new Mock<IMapper<Pilot, PilotDto>>();
        mockMapper.Setup(x => x.Map(pilot)).Returns(pilotDto);

        var result = await PilotEndpoints.GetPilot(
            mockPilotStore.Object,
            mockMapper.Object,
            pilotId);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<PilotDto>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(pilotDto, okResult.Value);
    }
}

