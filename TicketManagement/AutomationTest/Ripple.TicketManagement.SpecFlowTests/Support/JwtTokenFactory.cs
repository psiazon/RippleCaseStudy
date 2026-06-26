using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ripple.TicketManagement.SpecFlowTests.Support;

public static class JwtTokenFactory
{
    public const string Issuer = "SpecFlowTests";
    public const string Audience = "SpecFlowClients";
    public const string SigningKey = "SPEC_FLOW_TEST_SIGNING_KEY_64_CHARS_LONG_FOR_TICKET_API_123456789";

    public static string CreateToken(string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "specflow-user"),
            new Claim(ClaimTypes.Name, "SpecFlow User"),
            new Claim(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);

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
