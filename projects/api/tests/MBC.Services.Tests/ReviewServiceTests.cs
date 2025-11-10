using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Authorization;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Core.ValueObjects;
using MBC.Services.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MBC.Services.Tests;

public class ReviewServiceTests
{
    private readonly Mock<IDeliveryReviewStore> _mockReviewStore;
    private readonly Mock<IDeliveryStore> _mockDeliveryStore;
    private readonly Mock<IMbcAuthorizationService> _mockAuthService;
    private readonly ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _mockReviewStore = new Mock<IDeliveryReviewStore>();
        _mockDeliveryStore = new Mock<IDeliveryStore>();
        _mockAuthService = new Mock<IMbcAuthorizationService>();

        _reviewService = new ReviewService(
            _mockReviewStore.Object,
            _mockDeliveryStore.Object,
            _mockAuthService.Object,
            NullLogger<ReviewService>.Instance
        );
    }

    [Fact]
    public async Task CreateReview_WhenDeliveryIsDeliveredAndAuthorized_CreatesReviewSuccessfully()
    {
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var rating = Rating.From(5);
        var notes = "<p>Great service!</p>";

        var delivery = CreateDelivery(deliveryId, pilotId, customerId, DeliveryStatus.Delivered);

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync(delivery);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Create, delivery));
        _mockReviewStore.Setup(s => s.GetByDeliveryId(deliveryId)).ReturnsAsync((DeliveryReview?)null);
        _mockReviewStore.Setup(s => s.Create(It.IsAny<DeliveryReview>())).ReturnsAsync(
            (DeliveryReview r) => r
        );

        var result = await _reviewService.CreateReview(customerId, deliveryId, rating, notes);

        Assert.NotNull(result);
        Assert.Equal(pilotId, result.PilotId);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(deliveryId, result.DeliveryId);
        Assert.Equal(rating, result.Rating);
        Assert.Equal(notes, result.Notes);
        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(ReviewOperations.Create, delivery), Times.Once);
    }

    [Fact]
    public async Task CreateReview_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
    {
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var rating = Rating.From(4);
        var notes = "Good";

        var delivery = CreateDelivery(deliveryId, pilotId, customerId, DeliveryStatus.Delivered);

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync(delivery);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Create, delivery))
            .Throws<UnauthorizedAccessException>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _reviewService.CreateReview(customerId, deliveryId, rating, notes)
        );

        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(ReviewOperations.Create, delivery), Times.Once);
    }

    [Fact]
    public async Task CreateReview_WhenDeliveryDoesNotExist_ThrowsInvalidOperationException()
    {
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var rating = Rating.From(5);
        var notes = "Great";

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync((Delivery?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _reviewService.CreateReview(customerId, deliveryId, rating, notes)
        );

        Assert.Contains(deliveryId.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CreateReview_WhenDeliveryStatusIsPending_ThrowsInvalidOperationException()
    {
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var rating = Rating.From(5);
        var notes = "Great";

        var delivery = CreateDelivery(deliveryId, pilotId, customerId, DeliveryStatus.Pending);

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync(delivery);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Create, delivery));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _reviewService.CreateReview(customerId, deliveryId, rating, notes)
        );

        Assert.Contains("not yet delivered", exception.Message);
    }

    [Fact]
    public async Task CreateReview_WhenDeliveryStatusIsInTransit_ThrowsInvalidOperationException()
    {
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var rating = Rating.From(5);
        var notes = "Great";

        var delivery = CreateDelivery(deliveryId, pilotId, customerId, DeliveryStatus.InTransit);

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync(delivery);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Create, delivery));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _reviewService.CreateReview(customerId, deliveryId, rating, notes)
        );

        Assert.Contains("not yet delivered", exception.Message);
    }

    [Fact]
    public async Task CreateReview_WhenDeliveryAlreadyHasReview_ThrowsInvalidOperationException()
    {
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var rating = Rating.From(5);
        var notes = "Great";

        var delivery = CreateDelivery(deliveryId, pilotId, customerId, DeliveryStatus.Delivered);
        var existingReview = CreateReview(Guid.NewGuid(), pilotId, customerId, deliveryId);

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync(delivery);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Create, delivery));
        _mockReviewStore.Setup(s => s.GetByDeliveryId(deliveryId)).ReturnsAsync(existingReview);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _reviewService.CreateReview(customerId, deliveryId, rating, notes)
        );

        Assert.Contains("already been reviewed", exception.Message);
    }


    [Fact]
    public async Task UpdateReview_WhenAuthorized_UpdatesReviewSuccessfully()
    {
        var reviewId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var newRating = Rating.From(3);
        var newNotes = "<p>Updated notes</p>";
        var existingReview = CreateReview(reviewId, pilotId, customerId, deliveryId);

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync(existingReview);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Update, existingReview));
        _mockReviewStore.Setup(s => s.Update(It.IsAny<DeliveryReview>())).ReturnsAsync(
            (DeliveryReview r) => r
        );

        var result = await _reviewService.UpdateReview(reviewId, newRating, newNotes);

        Assert.NotNull(result);
        Assert.Equal(newRating, result.Rating);
        Assert.Equal(newNotes, result.Notes);
        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(ReviewOperations.Update, existingReview), Times.Once);
    }

    [Fact]
    public async Task UpdateReview_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
    {
        var reviewId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var newRating = Rating.From(3);
        var newNotes = "Updated";

        var existingReview = CreateReview(reviewId, pilotId, customerId, deliveryId);

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync(existingReview);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Update, existingReview))
            .Throws<UnauthorizedAccessException>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _reviewService.UpdateReview(reviewId, newRating, newNotes)
        );

        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(ReviewOperations.Update, existingReview), Times.Once);
    }

    [Fact]
    public async Task UpdateReview_WhenReviewDoesNotExist_ThrowsInvalidOperationException()
    {
        var reviewId = Guid.NewGuid();
        var newRating = Rating.From(3);
        var newNotes = "Updated";

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync((DeliveryReview?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _reviewService.UpdateReview(reviewId, newRating, newNotes)
        );

        Assert.Contains(reviewId.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task DeleteReview_WhenAuthorized_DeletesReviewSuccessfully()
    {
        var reviewId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var review = CreateReview(reviewId, pilotId, customerId, deliveryId);

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync(review);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Delete, review));
        _mockReviewStore.Setup(s => s.Delete(reviewId)).ReturnsAsync(true);

        await _reviewService.DeleteReview(reviewId);

        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(ReviewOperations.Delete, review), Times.Once);
        _mockReviewStore.Verify(s => s.Delete(reviewId), Times.Once);
    }

    [Fact]
    public async Task DeleteReview_WhenNotAuthorized_ThrowsUnauthorizedAccessException()
    {
        var reviewId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var review = CreateReview(reviewId, pilotId, customerId, deliveryId);

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync(review);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Delete, review))
            .Throws<UnauthorizedAccessException>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _reviewService.DeleteReview(reviewId)
        );

        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(ReviewOperations.Delete, review), Times.Once);
        _mockReviewStore.Verify(s => s.Delete(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteReview_WhenReviewDoesNotExist_ThrowsInvalidOperationException()
    {
        var reviewId = Guid.NewGuid();

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync((DeliveryReview?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _reviewService.DeleteReview(reviewId)
        );

        Assert.Contains(reviewId.ToString(), exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task DeleteReview_WhenDeleteOperationFails_ThrowsInvalidOperationException()
    {
        var reviewId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var review = CreateReview(reviewId, pilotId, customerId, deliveryId);

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync(review);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(ReviewOperations.Delete, review));
        _mockReviewStore.Setup(s => s.Delete(reviewId)).ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _reviewService.DeleteReview(reviewId)
        );

        Assert.Contains("Failed to delete", exception.Message);
    }

    [Fact]
    public async Task GetReviewByDeliveryId_WhenDeliveryExistsAndAuthorized_ReturnsReview()
    {
        var deliveryId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, pilotId, customerId, DeliveryStatus.Delivered);
        var review = CreateReview(reviewId, pilotId, customerId, deliveryId);

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync(delivery);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(DeliveryOperations.Read, delivery));
        _mockReviewStore.Setup(s => s.GetByDeliveryId(deliveryId)).ReturnsAsync(review);

        var result = await _reviewService.GetReviewByDeliveryId(deliveryId);

        Assert.NotNull(result);
        Assert.Equal(reviewId, result.Id);
        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(DeliveryOperations.Read, delivery), Times.Once);
    }

    [Fact]
    public async Task GetReviewByDeliveryId_WhenDeliveryDoesNotExist_ReturnsNull()
    {
        var deliveryId = Guid.NewGuid();

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync((Delivery?)null);

        var result = await _reviewService.GetReviewByDeliveryId(deliveryId);

        Assert.Null(result);
        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(It.IsAny<Microsoft.AspNetCore.Authorization.Infrastructure.OperationAuthorizationRequirement>(), It.IsAny<Delivery>()), Times.Never);
    }

    [Fact]
    public async Task GetReviewByDeliveryId_WhenNotAuthorized_ReturnsNullWithoutThrowingException()
    {
        var deliveryId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, pilotId, customerId, DeliveryStatus.Delivered);

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync(delivery);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(DeliveryOperations.Read, delivery))
            .Throws<UnauthorizedAccessException>();

        var result = await _reviewService.GetReviewByDeliveryId(deliveryId);

        Assert.Null(result);
        _mockAuthService.Verify(s => s.ThrowIfUnauthorized(DeliveryOperations.Read, delivery), Times.Once);
        _mockReviewStore.Verify(s => s.GetByDeliveryId(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetReviewByDeliveryId_WhenDeliveryExistsButNoReview_ReturnsNull()
    {
        var deliveryId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var delivery = CreateDelivery(deliveryId, pilotId, customerId, DeliveryStatus.Delivered);

        _mockDeliveryStore.Setup(s => s.GetById(deliveryId)).ReturnsAsync(delivery);
        _mockAuthService.Setup(s => s.ThrowIfUnauthorized(DeliveryOperations.Read, delivery));
        _mockReviewStore.Setup(s => s.GetByDeliveryId(deliveryId)).ReturnsAsync((DeliveryReview?)null);

        var result = await _reviewService.GetReviewByDeliveryId(deliveryId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPilotId_ReturnsPaginatedReviews()
    {
        var pilotId = Guid.NewGuid();
        var skip = 0;
        var take = 10;

        var reviews = new[]
        {
            CreateReview(Guid.NewGuid(), pilotId, Guid.NewGuid(), Guid.NewGuid()),
            CreateReview(Guid.NewGuid(), pilotId, Guid.NewGuid(), Guid.NewGuid())
        };

        var page = Page.Create(reviews, skip, take, 2);

        _mockReviewStore.Setup(s => s.GetByPilotId(pilotId, skip, take)).ReturnsAsync(page);

        var result = await _reviewService.GetByPilotId(pilotId, skip, take);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        _mockReviewStore.Verify(s => s.GetByPilotId(pilotId, skip, take), Times.Once);
    }

    [Fact]
    public async Task GetReviewById_WhenReviewExists_ReturnsReview()
    {
        var reviewId = Guid.NewGuid();
        var pilotId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var review = CreateReview(reviewId, pilotId, customerId, deliveryId);

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync(review);

        var result = await _reviewService.GetReviewById(reviewId);

        Assert.NotNull(result);
        Assert.Equal(reviewId, result.Id);
    }

    [Fact]
    public async Task GetReviewById_WhenReviewDoesNotExist_ReturnsNull()
    {
        var reviewId = Guid.NewGuid();

        _mockReviewStore.Setup(s => s.GetById(reviewId)).ReturnsAsync((DeliveryReview?)null);

        var result = await _reviewService.GetReviewById(reviewId);

        Assert.Null(result);
    }

    private static Delivery CreateDelivery(Guid id, Guid pilotId, Guid customerId, DeliveryStatus status)
    {
        return new Delivery
        {
            Id = id,
            PilotId = pilotId,
            CustomerId = customerId,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static DeliveryReview CreateReview(Guid id, Guid pilotId, Guid customerId, Guid deliveryId)
    {
        return new DeliveryReview
        {
            Id = id,
            PilotId = pilotId,
            CustomerId = customerId,
            DeliveryId = deliveryId,
            Rating = Rating.From(5),
            Notes = "Great service!",
            CreatedAt = DateTime.UtcNow
        };
    }
}
