using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Interfaces;
using System.Security.Claims;
using PokemonReviewApp.Data;

namespace PokemonReviewApp.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BahodirAkaAttribute : Attribute, IAuthorizationFilter
    {
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var _tokenService = context.HttpContext.RequestServices.GetService(typeof(ITokenService)) as ITokenService;
            var _dataContext = context.HttpContext.RequestServices.GetService(typeof(DataContext)) as DataContext;
            var token = Convert.ToString(context.HttpContext.Request.Headers.Authorization).Trim().Split(" ");
            if (token.Length > 1)
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(token[1]);
                var username = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _dataContext.Users.FirstOrDefault(u => u.Username == username);
                if (user != null && user.AccessToken == token[1])
                {
                    return;
                }
            }

            context.Result = new JsonResult(new { message = "Invalid token" }) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}