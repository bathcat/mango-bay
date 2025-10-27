using System;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MBC.Endpoints.Endpoints;

public static class PilotEndpoints
{
    public static void MapPilotEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var pilotsGroup = app.MapGroup(ApiRoutes.Pilots)
            .WithTags("Pilots");

        pilotsGroup.MapGet("/", GetPilots)
            .WithName("GetPilots")
            .Produces<Page<PilotDto>>(StatusCodes.Status200OK)
            .WithDescription("Retrieves a paginated list of pilots.");

        pilotsGroup.MapGet("/{id}", GetPilot)
            .WithName("GetPilot")
            .Produces<PilotDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a specific pilot by their ID.");
    }

    public static async Task<Ok<Page<PilotDto>>> GetPilots(
        IPilotStore pilotStore,
        IMapper<Pilot, PilotDto> pilotMapper,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10)
    {
        var pilots = await pilotStore.GetPage(skip, take);
        var page = PageMapper.Map(pilots, pilotMapper);
        return TypedResults.Ok(page);
    }

    public static async Task<Results<Ok<PilotDto>, NotFound>> GetPilot(
        IPilotStore pilotStore,
        IMapper<Pilot, PilotDto> pilotMapper,
        Guid id)
    {
        var pilot = await pilotStore.GetById(id);
        if (pilot == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(pilotMapper.Map(pilot));
    }
}

