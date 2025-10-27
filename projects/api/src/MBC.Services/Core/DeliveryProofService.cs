using System;
using System.IO;
using System.Threading.Tasks;
using MBC.Core.Authorization;
using MBC.Core.Entities;
using MBC.Core.Persistence;
using MBC.Core.Services;
using Microsoft.Extensions.Logging;

namespace MBC.Services.Core;

public class DeliveryProofService : IDeliveryProofService
{
    private readonly IDeliveryProofStore _proofStore;
    private readonly IDeliveryStore _deliveryStore;
    private readonly IImageRepository _imageRepository;
    private readonly IMbcAuthorizationService _authorizationService;
    private readonly ILogger<DeliveryProofService> _logger;

    public DeliveryProofService(
        IDeliveryProofStore proofStore,
        IDeliveryStore deliveryStore,
        IImageRepository imageRepository,
        IMbcAuthorizationService authorizationService,
        ILogger<DeliveryProofService> logger)
    {
        _proofStore = proofStore;
        _deliveryStore = deliveryStore;
        _imageRepository = imageRepository;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<DeliveryProof> UploadProofOfDelivery(Guid deliveryId, Stream imageStream, string fileName)
    {
        _logger.LogInformation("Uploading proof of delivery for delivery {DeliveryId}", deliveryId);

        var delivery = await _deliveryStore.GetById(deliveryId);
        if (delivery == null)
        {
            _logger.LogWarning("Delivery {DeliveryId} not found", deliveryId);
            throw new InvalidOperationException($"Delivery with ID {deliveryId} not found.");
        }

        _authorizationService.ThrowIfUnauthorized(DeliveryProofOperations.Create, delivery);

        if (delivery.Status == DeliveryStatus.Cancelled)
        {
            _logger.LogWarning("Cannot upload proof of delivery for cancelled delivery {DeliveryId}", deliveryId);
            throw new InvalidOperationException("Cannot upload proof of delivery for a cancelled delivery.");
        }

        var existingProof = await _proofStore.GetByDeliveryId(deliveryId);
        if (existingProof != null)
        {
            _logger.LogWarning("Proof of delivery already exists for delivery {DeliveryId}", deliveryId);
            throw new InvalidOperationException($"Proof of delivery already exists for delivery {deliveryId}.");
        }

        var result = await _imageRepository.SaveProofOfDeliveryImage(deliveryId, imageStream, fileName);

        if (!result.Success || result.RelativePath == null)
        {
            _logger.LogWarning("Failed to save proof of delivery image for delivery {DeliveryId}: {Error}", deliveryId, result.ErrorMessage);
            throw new InvalidOperationException($"Failed to save proof of delivery image: {result.ErrorMessage}");
        }

        var proof = new DeliveryProof
        {
            Id = Guid.NewGuid(),
            DeliveryId = deliveryId,
            PilotId = delivery.PilotId,
            CustomerId = delivery.CustomerId,
            ImagePath = result.RelativePath,
            CreatedAt = DateTime.UtcNow
        };

        var createdProof = await _proofStore.Create(proof);

        delivery.Status = DeliveryStatus.Delivered;
        delivery.CompletedOn = DateTime.UtcNow;
        delivery.UpdatedAt = DateTime.UtcNow;
        await _deliveryStore.Update(delivery);

        _logger.LogInformation(
            "Successfully uploaded proof of delivery {ProofId} for delivery {DeliveryId}: {Path}",
            createdProof.Id,
            deliveryId,
            result.RelativePath);

        return createdProof;
    }

    public async Task<DeliveryProof?> GetProofByDeliveryId(Guid deliveryId)
    {
        var proof = await _proofStore.GetByDeliveryId(deliveryId);
        if (proof != null)
        {
            _authorizationService.ThrowIfUnauthorized(DeliveryProofOperations.Read, proof);
        }
        return proof;
    }

    public async Task<DeliveryProof?> GetProofById(Guid id)
    {
        var proof = await _proofStore.GetById(id);
        if (proof != null)
        {
            _authorizationService.ThrowIfUnauthorized(DeliveryProofOperations.Read, proof);
        }
        return proof;
    }
}

