namespace CinemaApp.Model;

public static class UserMapping
{
    public static UserResponseDTO ToResponse(this User user) =>
        new UserResponseDTO
        {
            Id = user.Id,
            Email = user.Email,
            UserType = user.UserType,
            CreatedAt = user.CreatedAt
        };
    
    public static List<UserResponseDTO> ToResponse(this IEnumerable<User> users) =>
        users.Select(u => u.ToResponse()).ToList();
}