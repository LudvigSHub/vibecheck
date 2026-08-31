using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VibeCheck.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ---------------------------------------------------------------------------
// JWT
// ---------------------------------------------------------------------------
// Två skilda saker: att skapa en token gör vi i TokenService, att kontrollera
// inkommande tokens gör .NET åt oss. Det som ställs in här är kontrollen.

// Nyckeln ligger i user-secrets och inte i appsettings, eftersom appsettings
// hamnar på GitHub. Var och en sätter sin egen en gång:
//
//   dotnet user-secrets set "Jwt:Key" "<minst 32 tecken>" --project backend/VibeCheck.Api
//
// Kollen nedan finns för att felet annars dyker upp först vid inloggning, och då
// är det betydligt svårare att förstå vad som är fel.
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    // 32 tecken är ett krav från algoritmen, inget vi hittat på.
    throw new InvalidOperationException(
        "Jwt:Key saknas eller är kortare än 32 tecken. Kör detta en gång:\n" +
        "  dotnet user-secrets set \"Jwt:Key\" \"<minst 32 tecken>\" --project backend/VibeCheck.Api");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Kollar att token är signerad med vår nyckel och inte påhittad.
            // Det här är hela poängen med JWT.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            // Kollar att token inte gått ut (fyra timmar, sätts i TokenService).
            ValidateLifetime = true,

            // Avstängda med flit. Issuer och audience används för att hålla isär
            // tokens från olika system som delar nyckel. Vi har ett api och en
            // frontend, så det finns inget att blanda ihop. Kan slås på senare.
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Behövs för att [Authorize] och rollerna ska funka.
builder.Services.AddAuthorization();

builder.Services.AddScoped<TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Ordningen spelar roll. UseAuthentication svarar på "vem är du" och fyller i
// uppgifterna om användaren, UseAuthorization svarar på "får du". Man måste veta
// vem någon är innan man kan avgöra vad hen får göra.
//
// Mallen vi fick hade bara UseAuthorization. Saknas raden nedanför får man 401 på
// allt, även med en helt korrekt token, och felmeddelandet avslöjar inte varför.
//
// Till den som gör CORS-kortet: app.UseCors() ska in precis här ovanför.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
