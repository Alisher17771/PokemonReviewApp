using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repository;
using PokemonReviewApp.Services;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            var tokens = await _userRepository.Login(model);
            return tokens != null ? Ok(tokens) : BadRequest("Please check user credentials and try again.");
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Login(UserRegisterDto model)
        {
            var user = await _userRepository.Register(model);
            return user != null
                ? Ok("User created successfully!")
                : BadRequest("User creation failed! Please check user details and try again.");
        }

        [HttpPost]
        [Route("Refresh")]
        public async Task<IActionResult> Refresh(Tokens tokens)
        {
            var newTokens = await _userRepository.Refresh(tokens);
            return newTokens != null ? Ok(newTokens) : BadRequest();
        }
    }
}