using System.Text;
using System.Text.Json.Serialization;
using Grooming_Management_App.BackgroundServices;
using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Extensions;
using Grooming_Management_App.Middleware;
using Grooming_Management_App.Services.AuthServ;
using Grooming_Management_App.Services.AvailabilityServ;
using Grooming_Management_App.Services.BlacklistServ;
using Grooming_Management_App.Services.Breed;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.DogOwner;
using Grooming_Management_App.Services.DogServ;
using Grooming_Management_App.Services.EarningServ;
using Grooming_Management_App.Services.GroomerScheduleServ;
using Grooming_Management_App.Services.GroomerServ;
using Grooming_Management_App.Services.GroomerTimeOffServ;
using Grooming_Management_App.Services.NotificationServ;
using Grooming_Management_App.Services.PasswordHasherServ;
using Grooming_Management_App.Services.SalonServ;
using Grooming_Management_App.Services.ServiceBreedServ;
using Grooming_Management_App.Services.ServiceServ;
using Grooming_Management_App.Services.StripeServ;
using Grooming_Management_App.Services.SubscriptionServ;
using Grooming_Management_App.Services.TokenServ;
using Grooming_Management_App.Services.VisitServ;
using Grooming_Management_App.Services.WaitlistServ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using SubscriptionService = Grooming_Management_App.Services.SubscriptionServ.SubscriptionService;
using TokenService = Grooming_Management_App.Services.TokenServ.TokenService;


var builder = WebApplication.CreateBuilder(args);

// ---------- JWT ----------
var secretKey = builder.Configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

// ---------- MVC + JSON ----------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

// ---------- Swagger ----------
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Wklej sam token JWT (bez słowa Bearer)."
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ---------- Baza ----------
// Notification celowo bez query filtra (czytany z ReminderScheduler bez HttpContext).
// Nigdy nie sięgamy z Notification do DogOwner przez nawigację, więc ostrzeżenie nieistotne.
builder.Services.AddDbContext<GroomingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(w => w.Ignore(
            CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

// ---------- Serwisy domenowe ----------
builder.Services.AddScoped<IBreedReaderService, BreedService>();
builder.Services.AddScoped<ISalonService, SalonService>();
builder.Services.AddScoped<IEarningsReaderService, EarningsService>();
builder.Services.AddScoped<IAvailabilityReaderService, AvailabilityService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScopedWithInterfaces<GroomerService>(
    typeof(IGroomerReaderService), typeof(IGroomerWriterService));

builder.Services.AddScopedWithInterfaces<DogOwnerService>(
    typeof(IDogOwnerReaderService), typeof(IDogOwnerWriterService));

builder.Services.AddScopedWithInterfaces<DogService>(
    typeof(IDogReaderService), typeof(IDogWriterService));

builder.Services.AddScopedWithInterfaces<ServiceService>(
    typeof(IServiceReaderService), typeof(IServiceWriterService));

builder.Services.AddScopedWithInterfaces<ServiceBreedService>(
    typeof(IServiceBreedReaderService), typeof(IServiceBreedWriterService));

builder.Services.AddScopedWithInterfaces<VisitService>(
    typeof(IVisitReaderService), typeof(IVisitWriterService));

builder.Services.AddScopedWithInterfaces<GroomerScheduleService>(
    typeof(IGroomerScheduleReaderService), typeof(IGroomerScheduleWriterService));

builder.Services.AddScopedWithInterfaces<GroomerTimeOffService>(
    typeof(IGroomerTimeOffReaderService), typeof(IGroomerTimeOffWriterService));

builder.Services.AddScopedWithInterfaces<WaitlistService>(
    typeof(IWaitlistReaderService), typeof(IWaitlistWriterService));

builder.Services.AddScopedWithInterfaces<BlacklistService>(
    typeof(IBlacklistService), typeof(IBlacklistCheckService));

builder.Services.AddScopedWithInterfaces<AuthenticationService>(
    typeof(ILoginService), typeof(IPasswordService),
    typeof(IRegistrationService), typeof(ITokenSessionService));

// ---------- Serwisy infrastrukturalne ----------
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISmsService, MockSmsService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ---------- Zadania w tle ----------
builder.Services.AddHostedService<ReminderScheduler>();
builder.Services.AddHostedService<SubscriptionScheduler>();
builder.Services.AddHostedService<TokenCleanupScheduler>();

// ---------- Obsługa błędów ----------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---------- Stripe ----------
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"]
                             ?? throw new InvalidOperationException("Stripe SecretKey is not configured");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GroomingDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(context, passwordHasher);
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<SubscriptionMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();