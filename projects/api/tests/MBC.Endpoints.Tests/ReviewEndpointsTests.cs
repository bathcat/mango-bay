using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Services;
using MBC.Core.ValueObjects;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints;
using MBC.Endpoints.Mapping;
using Moq;
using Xunit;

namespace MBC.Endpoints.Tests;

public class ReviewEndpointsTests
{
    private static DeliveryReview CreateDeliveryReview()
    {
        return new DeliveryReview
        {
            Id = Guid.NewGuid(),
            PilotId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            DeliveryId = Guid.NewGuid(),
            Rating = Rating.From(4),
            Notes = "Great pilot!",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static DeliveryReviewDto CreateDeliveryReviewDto()
    {
        return new DeliveryReviewDto
        {
            Id = Guid.NewGuid(),
            PilotId = Guid.NewGuid(),
            Rating = Rating.From(4),
            Notes = "Great pilot!",
            CreatedAt = DateTime.UtcNow
        };
    }



    [Fact]
    public async Task GetReview_WhenNotFound_ReturnsNotFound()
    {
        var reviewId = Guid.NewGuid();

        var mockReviewService = new Mock<IReviewService>();
        mockReviewService.Setup(x => x.GetReviewById(reviewId)).ReturnsAsync((DeliveryReview?)null);

        var mockMapper = new Mock<IMapper<DeliveryReview, DeliveryReviewDto>>();

        var result = await ReviewEndpoints.GetReview(
            mockReviewService.Object,
            mockMapper.Object,
            reviewId);

        var notFoundResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}

