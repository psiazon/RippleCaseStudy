using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ripple.Ticketing.EventManagement.Api.Services
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Retrieve the "Jwt" configuration section
            var jwtSection = configuration.GetSection("Jwt");
            Console.WriteLine($"Jwt:Issuer = {jwtSection["Issuer"]}");

            var token = GenerateJwtToken(configuration, "alice", new[] { "Admin" });
            Console.WriteLine(token);
        }

        // Generates a JWT compatible with the authentication setup in Program.cs
        public static string GenerateJwtToken(IConfiguration configuration, string userName, IEnumerable<string> roles, TimeSpan? validFor = null)
        {
            var jwt = configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey missing")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                // Optional: include sub and unique jti
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims so policy.RequireRole(...) works
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var lifetime = validFor ?? TimeSpan.FromHours(1000);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(lifetime),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}