using MBC.Core.Entities;
using MBC.Endpoints.Dtos;
using Microsoft.AspNetCore.Http;

namespace MBC.Endpoints.Mapping;

public class SiteMapper : IMapper<Site, SiteDto>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SiteMapper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public SiteDto Map(Site source)
    {
        string? imageUrl = null;
        if (!string.IsNullOrEmpty(source.ImageUrl))
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var request = httpContext.Request;
                imageUrl = $"{request.Scheme}://{request.Host}/uploads/{source.ImageUrl}";
            }
        }

        return new SiteDto
        {
            Id = source.Id,
            Name = source.Name,
            Notes = source.Notes,
            Island = source.Island,
            Address = source.Address,
            Location = source.Location,
            Status = source.Status,
            ImageUrl = imageUrl
        };
    }
}

