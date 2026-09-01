using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace VibeCheck.Api.Services;

// Bygger den JWT som frontend får vid inloggning och sen skickar med i varje anrop.
// Vi sparar inget på servern om vem som är inloggad, all info finns i token.
//
// Obs: innehållet i en token går att läsa för vem som helst (testa på jwt.io).
// Det är signaturen som är skyddet, inte innehållet. Så inget känsligt här.
public class TokenService
{
    private readonly IConfiguration _config;

    // IConfiguration behövs för att komma åt den hemliga nyckeln i user-secrets.
    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    // Anropas efter lyckad registrering eller inloggning.
    //
    // Tar strängar istället för ett ApplicationUser, så att den här filen inte
    // behöver veta något om Identity eller databasen. Att ta upp i gruppen när
    // modellerna är klara, men jag tycker vi kan låta det vara så här.
    public string CreateToken(string userId, string userName, IEnumerable<string> roles)
    {
        // Claims = det vi vill kunna veta om användaren utan att fråga databasen.
        var claims = new List<Claim>
        {
            // Det här id:t läser vi ut i controllers för att veta vems data vi rör.
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName)
        };

        // Rollerna behövs för att [Authorize(Roles = "Admin")] ska funka.
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Många exempel på nätet använder "sub" och "role" istället för ClaimTypes.
        // Det kräver tre extra inställningar, och missar man dem får man 403 på allt
        // utan felmeddelande. ClaimTypes blir fulare i token men funkar direkt.

        // Samma nyckel används här och i Program.cs. Är den inte identisk godkänns
        // ingen token.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Fyra timmar är godtyckligt valt, men känns lagom. Går att ändra.
        // UtcNow och inte Now, all tid ska vara UTC hos oss.
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
