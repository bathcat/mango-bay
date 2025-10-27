using MBC.Core.Services;
using MBC.Endpoints.Dtos;

namespace MBC.Endpoints.Mapping;

public class AuthResponseMapper : IMapper<AuthResult, AuthResponse>
{
    public AuthResponse Map(AuthResult source)
    {
        return new AuthResponse
        {
            AccessToken = source.AccessToken!,
            RefreshToken = source.RefreshToken!,
            User = new UserDto
            {
                Id = source.UserId!.Value,
                Email = source.Email!,
                Nickname = source.Nickname,
                Role = source.Role!,
                CustomerId = source.CustomerId,
                PilotId = source.PilotId
            }
        };
    }
}

