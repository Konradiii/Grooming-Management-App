

using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Services.AuthServ;
using Grooming_Management_App.Services.Breed;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.DogOwner;
using Grooming_Management_App.Services.DogServ;
using Grooming_Management_App.Services.EarningServ;
using Grooming_Management_App.Services.GroomerServ;
using Grooming_Management_App.Services.PasswordHasherServ;
using Grooming_Management_App.Services.SalonServ;
using Grooming_Management_App.Services.ServiceBreedServ;
using Grooming_Management_App.Services.ServiceServ;
using Grooming_Management_App.Services.TokenServ;
using Grooming_Management_App.Services.VisitServ;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IBreedService, BreedService>();
builder.Services.AddScoped<ISalonService, SalonService>();
builder.Services.AddScoped<IGroomerService, GroomerService>();
builder.Services.AddScoped<IDogOwnerService, DogOwnerService>();
builder.Services.AddScoped<IDogService, DogService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IServiceBreedService, ServiceBreedService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IEarningsService, EarningsService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddDbContext<GroomingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GroomingDbContext>();
    
    await DbSeeder.SeedAsync(context);
}
app.UseExceptionHandler();
app.UseHttpsRedirection();


app.Run();
