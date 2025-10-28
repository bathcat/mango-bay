using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

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

        var imageData = await file.ToMemory();
        var proof = await proofService.UploadProofOfDelivery(deliveryId, imageData, file.FileName);

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


}

