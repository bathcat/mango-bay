using System;
using System.IO;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace MBC.Endpoints.Endpoints;

public static class DeliveryProofEndpoints
{
    public static void MapDeliveryProofEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var proofsGroup = app.MapGroup(ApiRoutes.Proofs)
            .WithTags("Delivery Proofs")
            .RequireAuthorization();

        proofsGroup.MapPost("/deliveries/{deliveryId}/upload", UploadProofOfDelivery)
            .WithName("UploadProofOfDelivery")
            .Produces<DeliveryProofDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .WithDescription("Uploads proof of delivery image for a delivery.");

        proofsGroup.MapGet("/deliveries/{deliveryId}", GetProofByDelivery)
            .WithName("GetProofByDelivery")
            .Produces<DeliveryProofDto?>(StatusCodes.Status200OK)
            .WithDescription("Retrieves proof of delivery for a specific delivery. Returns null if no proof has been uploaded yet.");

        proofsGroup.MapGet("/deliveries/{deliveryId}/image", GetProofImage)
            .WithName("GetProofImage")
            .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves the proof of delivery image for a specific delivery.");
    }

    public static async Task<Results<Created<DeliveryProofDto>, BadRequest<string>>> UploadProofOfDelivery(
        IDeliveryProofService proofService,
        IMapper<DeliveryProof, DeliveryProofDto> proofMapper,
        Guid deliveryId,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return TypedResults.BadRequest("No file uploaded");
        }

        using var stream = file.OpenReadStream();
        var proof = await proofService.UploadProofOfDelivery(deliveryId, stream, file.FileName);

        var proofDto = proofMapper.Map(proof);
        return TypedResults.Created($"{ApiRoutes.Proofs}/deliveries/{deliveryId}", proofDto);
    }

    public static async Task<Ok<DeliveryProofDto?>> GetProofByDelivery(
        IDeliveryProofService proofService,
        IMapper<DeliveryProof, DeliveryProofDto> proofMapper,
        Guid deliveryId)
    {
        var proof = await proofService.GetProofByDeliveryId(deliveryId);
        DeliveryProofDto? result = proofMapper.MapOptional(proof);
        return TypedResults.Ok<DeliveryProofDto?>(result);
    }

    public static async Task<Results<FileStreamHttpResult, NotFound>> GetProofImage(
        IDeliveryProofService proofService,
        IImageRepository imageRepository,
        ILogger<Program> logger,
        Guid deliveryId)
    {
        var proof = await proofService.GetProofByDeliveryId(deliveryId);
        if (proof == null || string.IsNullOrEmpty(proof.ImagePath))
        {
            return TypedResults.NotFound();
        }

        string physicalPath;
        try
        {
            physicalPath = imageRepository.GetPhysicalPath(proof.ImagePath);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid path for proof of delivery: {Path}", proof.ImagePath);
            return TypedResults.NotFound();
        }

        if (!File.Exists(physicalPath))
        {
            logger.LogWarning(
                "Proof of delivery file not found on disk for delivery {DeliveryId}: {Path}",
                deliveryId,
                physicalPath);
            return TypedResults.NotFound();
        }

        var extension = Path.GetExtension(physicalPath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        //TODO: Revisit this.
        var fileStream = File.OpenRead(physicalPath);
        return TypedResults.File(fileStream, contentType, enableRangeProcessing: true);
    }
}

