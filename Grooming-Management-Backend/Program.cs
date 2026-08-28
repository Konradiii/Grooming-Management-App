using System.Text;
using Grooming_Management_App.BackgroundServices;
using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Exceptions;
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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Stripe;
using SubscriptionService = Grooming_Management_App.Services.SubscriptionServ.SubscriptionService;
using TokenService = Grooming_Management_App.Services.TokenServ.TokenService;
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

//JWT
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

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

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

builder.Services.AddScoped<IBreedReaderService, BreedService>();

builder.Services.AddScoped<ISalonService, SalonService>();

builder.Services.AddScoped<IGroomerReaderService, GroomerService>();
builder.Services.AddScoped<IGroomerWriterService, GroomerService>();


builder.Services.AddScoped<IDogOwnerWriterService, DogOwnerService>();
builder.Services.AddScoped<IDogOwnerReaderService, DogOwnerService>();


builder.Services.AddScoped<IDogWriterService, DogService>();
builder.Services.AddScoped<IDogReaderService, DogService>();

builder.Services.AddScoped<IServiceReaderService, ServiceService>();
builder.Services.AddScoped<IServiceWriterService, ServiceService>();


builder.Services.AddScoped<IServiceBreedWriterService, ServiceBreedService>();
builder.Services.AddScoped<IServiceBreedReaderService, ServiceBreedService>();


builder.Services.AddScoped<IVisitReaderService, VisitService>();
builder.Services.AddScoped<IVisitWriterService, VisitService>();


builder.Services.AddScoped<IEarningsReaderService, EarningsService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<ILoginService, AuthenticationService>();
builder.Services.AddScoped<IPasswordService, AuthenticationService>();
builder.Services.AddScoped<IRegistrationService, AuthenticationService>();
builder.Services.AddScoped<ITokenSessionService, AuthenticationService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<BlacklistService>();
builder.Services.AddScoped<IBlacklistService>(sp => sp.GetRequiredService<BlacklistService>());
builder.Services.AddScoped<IBlacklistCheckService>(sp => sp.GetRequiredService<BlacklistService>());

builder.Services.AddScoped<ISmsService, MockSmsService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddHostedService<ReminderScheduler>();
builder.Services.AddHostedService<SubscriptionScheduler>();

builder.Services.AddScoped<IGroomerScheduleReaderService, GroomerScheduleService>();
builder.Services.AddScoped<IGroomerScheduleWriterService, GroomerScheduleService>();


builder.Services.AddScoped<IGroomerTimeOffWriterService, GroomerTimeOffService>();
builder.Services.AddScoped<IGroomerTimeOffReaderService, GroomerTimeOffService>();

builder.Services.AddScoped<IAvailabilityReaderService, AvailabilityService>();

builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

builder.Services.AddScoped<IWaitlistReaderService, WaitlistService>();
builder.Services.AddScoped<IWaitlistWriterService, WaitlistService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHostedService<TokenCleanupScheduler>();

builder.Services.AddDbContext<GroomingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"]
                             ?? throw new InvalidOperationException("Stripe SecretKey is not configured");

builder.Services.AddScoped<IStripeService, StripeService>();

// Notification celowo bez query filtra (czytany z ReminderScheduler bez HttpContext).
// Nigdy nie sięgamy z Notification do DogOwner przez nawigację, więc ostrzeżenie nieistotne.
builder.Services.AddDbContext<GroomingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(w => w.Ignore(
            Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

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