using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SentinelMES.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginDto request)
    {
        // Şimdilik veritabanı yerine kod içinde statik bir kontrol yapıyoruz. 
        // İleride bunu PostgreSQL'deki Users tablosuna bağlayabilirsin!
        if (request.Username == "admin" && request.Password == "123456")
        {
            // Kullanıcı doğru! Ona bir bilet (Token) yazalım.
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin") // Rolünü Admin yaptık!
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(2), // 2 saat geçerli
                signingCredentials: credentials);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        return Unauthorized("Hatalı kullanıcı adı veya şifre!");
    }
}

public class UserLoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}