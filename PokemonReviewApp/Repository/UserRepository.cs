using Azure.Core;
using Microsoft.EntityFrameworkCore;
using PokemonReviewApp.Data;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCryptNet = BCrypt.Net.BCrypt;

namespace PokemonReviewApp.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public UserRepository(IConfiguration configuration, DataContext context, ITokenService tokenService)
        {
            _configuration = configuration;
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<Tokens?> Login(UserLoginDto model)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !BCryptNet.Verify(model.Password, user.PasswordHash))
            {
                return null;
            }

            var authClaims = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier, user.Username),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var accessToken = user.AccessToken;
            var refreshToken = user.RefreshToken;
            if (user.AccessToken == null || user.AccessTokenExpiryTime < DateTime.UtcNow)
            {
                accessToken = _tokenService.GenerateAccessToken(authClaims);
                refreshToken = _tokenService.GenerateRefreshToken();
                user.AccessToken = accessToken;
                user.RefreshToken = accessToken;
                user.AccessTokenExpiryTime = DateTime.UtcNow.AddDays(1);
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();
            }
            
            var tokenValidityInDays = int.Parse(_configuration["JWT:RefreshTokenValidityInDays"]!);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(tokenValidityInDays);
            await _context.SaveChangesAsync();

            return new Tokens()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<User?> Register(UserRegisterDto model)
        {
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                return null;
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                PasswordHash = BCryptNet.HashPassword(model.Password)
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<Tokens?> Refresh(Tokens tokens)
        {
            string refreshToken = tokens.RefreshToken!;
            var principal = _tokenService.GetPrincipalFromExpiredToken(tokens.AccessToken!);
            var username = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user is null
                || user.RefreshToken != refreshToken
                || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshValidityInDays = int.Parse(_configuration["JWT:RefreshTokenValidityInDays"]!);
            var tokenValidityInDays = int.Parse(_configuration["JWT:TokenValidityInDays"]!);
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshValidityInDays);
            user.AccessToken = newAccessToken;
            user.AccessTokenExpiryTime = DateTime.UtcNow.AddDays(tokenValidityInDays);
            await _context.SaveChangesAsync();
            return new Tokens()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}