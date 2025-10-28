using System;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Persistence;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MBC.Endpoints.Endpoints;

public static class SiteEndpoints
{
    public static void MapSiteEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var sitesGroup = app.MapGroup(ApiRoutes.Sites)
            .WithTags("Sites");

        sitesGroup.MapGet("/", GetSites)
            .WithName("GetSites")
            .Produces<Page<SiteDto>>(StatusCodes.Status200OK)
            .WithDescription("Retrieves a paginated list of sites.");

        sitesGroup.MapGet("/{id}", GetSite)
            .WithName("GetSite")
            .Produces<SiteDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a specific site by its ID.");

        sitesGroup.MapPost("/", CreateSite)
            .WithName("CreateSite")
            .Produces<SiteDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Creates a new site.")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator));

        sitesGroup.MapPut("/{id}", UpdateSite)
            .WithName("UpdateSite")
            .Produces<SiteDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Updates a site's information.")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator));

        sitesGroup.MapDelete("/{id}", DeleteSite)
            .WithName("DeleteSite")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Deletes a site or sets it to Inactive if it has associated deliveries.")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator));

        sitesGroup.MapPost("/{id}/image", UploadSiteImage)
            .WithName("UploadSiteImage")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .WithDescription("Uploads an image for a site.")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Administrator));
    }

    public static async Task<Ok<Page<SiteDto>>> GetSites(
        ISiteStore siteStore,
        IMapper<Site, SiteDto> siteMapper,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10)
    {
        var sites = await siteStore.GetPage(skip, take);
        var page = PageMapper.Map(sites, siteMapper);
        return TypedResults.Ok(page);
    }

    public static async Task<Results<Ok<SiteDto>, NotFound>> GetSite(
        ISiteStore siteStore,
        IMapper<Site, SiteDto> siteMapper,
        Guid id)
    {
        var site = await siteStore.GetById(id);
        if (site == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(siteMapper.Map(site));
    }

    public static async Task<Created<SiteDto>> CreateSite(
        ISiteService siteService,
        IMapper<Site, SiteDto> siteMapper,
        Dtos.CreateOrUpdateSiteRequest request)
    {
        var coreRequest = new MBC.Core.Models.CreateOrUpdateSiteRequest
        {
            Name = request.Name,
            Notes = request.Notes,
            Island = request.Island,
            Address = request.Address,
            Location = request.Location,
            Status = request.Status
        };

        var site = await siteService.CreateSite(coreRequest);
        var siteDto = siteMapper.Map(site);
        return TypedResults.Created($"{ApiRoutes.Sites}/{siteDto.Id}", siteDto);
    }

    public static async Task<Results<Ok<SiteDto>, NotFound>> UpdateSite(
        ISiteService siteService,
        IMapper<Site, SiteDto> siteMapper,
        Guid id,
        Dtos.CreateOrUpdateSiteRequest request)
    {
        var coreRequest = new MBC.Core.Models.CreateOrUpdateSiteRequest
        {
            Name = request.Name,
            Notes = request.Notes,
            Island = request.Island,
            Address = request.Address,
            Location = request.Location,
            Status = request.Status
        };

        var site = await siteService.UpdateSite(id, coreRequest);
        var siteDto = siteMapper.Map(site);
        return TypedResults.Ok(siteDto);
    }

    public static async Task<NoContent> DeleteSite(
        ISiteService siteService,
        Guid id)
    {
        await siteService.DeleteSite(id);
        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> UploadSiteImage(
        ISiteService siteService,
        Guid id,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return TypedResults.BadRequest("No file uploaded");
        }

        var imageData = await file.ToMemory();
        var result = await siteService.UploadSiteImage(id, imageData, file.FileName);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.ErrorMessage ?? "Upload failed");
        }

        if (string.IsNullOrEmpty(result.RelativePath))
        {
            throw new InvalidOperationException("Upload succeeded but no path was returned");
        }

        return TypedResults.Ok(result.RelativePath);
    }

}

