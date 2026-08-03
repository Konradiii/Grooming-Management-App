

using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Services.PasswordHasherServ;
using Grooming_Management_App.Services.TokenServ;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddDbContext<GroomingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GroomingDbContext>();
    
    await DbSeeder.SeedAsync(context);
}

app.UseHttpsRedirection();


app.Run();
