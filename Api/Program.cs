using System.Text;
using JobApplication.Api.Services;
using JobApplication.Application;
using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure;
using JobApplication.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Framework Services
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// MediatR & Application Services
builder.Services.AddApplicationServices();

// 2. Infrastructure Services (DbContext, Identity, Repositories, Services)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. JWT Authentication & Authorization
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TrackApplicationSecureJwtSigningKeyForDevelopment2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "JobApplicationApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "JobApplicationUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[JWT Auth Failed]: {context.Exception.Message}");
            Console.ResetColor();
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            if (!string.IsNullOrEmpty(context.Error) || !string.IsNullOrEmpty(context.ErrorDescription))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[JWT Challenge]: {context.Error} - {context.ErrorDescription}");
                Console.ResetColor();
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// 4. OpenAPI / Swagger Documentation with Bearer Security Scheme
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Track Application System API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token directly (without the 'Bearer ' prefix). Example: eyJhbGciOi...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

var app = builder.Build();

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Track Application System API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 6. Automatically apply pending database migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();

public partial class Program { }
