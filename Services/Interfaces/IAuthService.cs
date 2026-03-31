using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Auth;

namespace PokeBuilder.Server.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
}
