namespace PokeBuilder.Server.Models.DTOs.Auth;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public static UserResponse FromUser(Models.User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Username = user.Username
    };
}
