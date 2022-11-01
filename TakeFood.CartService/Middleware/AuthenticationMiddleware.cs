using TakeFood.CartService.Service;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using StoreService.Service;

namespace CartService.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            
            await _next(context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

}
