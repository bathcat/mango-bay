using System.Threading.Tasks;

namespace MBC.Core.Services;

public interface IAuthService
{
    Task<AuthResult> SignUp(string email, string password, string nickname);

    Task<AuthResult> CreatePilot(string email, string password, string shortName, string fullName, string bio, string? avatarUrl);

    Task<AuthResult> CreateAdmin(string email, string password);

    Task<AuthResult> SignIn(string email, string password);

    Task<AuthResult> RefreshToken(string refreshToken);

    Task SignOut(string refreshToken);
}
