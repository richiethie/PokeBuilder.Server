using PokeBuilder.Server.Models.Common;
using PokeBuilder.Server.Models.DTOs.Auth;
using PokeBuilder.Server.Models.DTOs.Users;

namespace PokeBuilder.Server.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<AuthResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<ServiceResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<ServiceResult> DeleteAccountAsync(Guid userId);
}
