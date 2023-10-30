using PokemonReviewApp.Dto;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Interfaces
{
    public interface IUserRepository
    {
        Task<Tokens?> Login(UserLoginDto model);
        Task<User?> Register(UserRegisterDto model);
        Task<Tokens?> Refresh(Tokens tokens);
    }
}