using MBC.Core.Entities;
using MBC.Endpoints.Dtos;
using Microsoft.AspNetCore.Http;

namespace MBC.Endpoints.Mapping;

public class PilotMapper : IMapper<Pilot, PilotDto>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PilotMapper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public PilotDto Map(Pilot source)
    {
        string? avatarUrl = null;
        if (!string.IsNullOrEmpty(source.AvatarUrl))
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var request = httpContext.Request;
                avatarUrl = $"{request.Scheme}://{request.Host}/uploads/{source.AvatarUrl}";
            }
        }

        return new PilotDto
        {
            Id = source.Id,
            FullName = source.FullName,
            ShortName = source.ShortName,
            AvatarUrl = avatarUrl,
            Bio = source.Bio
        };
    }
}

