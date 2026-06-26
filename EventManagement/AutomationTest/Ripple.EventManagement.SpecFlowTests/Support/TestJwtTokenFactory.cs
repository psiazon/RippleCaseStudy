using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ripple.EventManagement.SpecFlowTests.Support;

public static class TestJwtTokenFactory
{
    public const string Issuer = "Ripple.EventManagement.SpecFlowTests";
    public const string Audience = "Ripple.EventManagement.Api";
    public const string SigningKey = "SpecFlow_event_management_test_signing_key_1234567890";

    public static string CreateToken(string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "specflow-user"),
            new Claim(ClaimTypes.Name, "SpecFlow User"),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-5),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
