using Microsoft.EntityFrameworkCore;
using VibeCheck.Data.Data;
using Microsoft.AspNetCore.Identity;
using VibeCheck.Data.Models;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<VibeCheckDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services
    .AddIdentityCore<User>()
    .AddEntityFrameworkStores<VibeCheckDbContext>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<VibeCheckDbContext>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<User>>();

    await DbInitializer.InitializeAsync(context, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
