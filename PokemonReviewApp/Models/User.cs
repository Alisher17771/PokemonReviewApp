namespace PokemonReviewApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public DateTime AccessTokenExpiryTime { get; set; }
    }
}