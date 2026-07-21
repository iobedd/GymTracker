using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using GymTracker.API.Middleware;
using GymTracker.Application.Interfaces;
using GymTracker.Infrastructure.Data;
using GymTracker.Infrastructure.Services;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Serilog ----------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ---------------- Servicii aplicatie ----------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserProvisioningService, UserProvisioningService>();
builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformation>();

// ---------------- DbContext ----------------
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------- Controllers ----------------
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();

// ---------------- Autentificare: Keycloak (OIDC resource server) ----------------
// Aplicatia NU emite si NU valideaza tokenuri cu cheie proprie - valideaza tokenuri
// emise de Keycloak, folosind cheile publice publicate la Authority/.well-known/openid-configuration.
var keycloakAuthority = builder.Configuration["Keycloak:Authority"]!;
var keycloakAudience = builder.Configuration["Keycloak:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = keycloakAuthority;
    options.Audience = keycloakAudience;
    options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = "preferred_username",
        RoleClaimType = System.Security.Claims.ClaimTypes.Role // populat de RoleClaimsTransformation
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("EmployeeOrAdmin", p => p.RequireRole("Employee", "Admin"));
});

// ---------------- CORS ----------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

// ---------------- Rate limiting ----------------
builder.Services.AddMemoryCache();
builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<AspNetCoreRateLimit.IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();

// ---------------- Swagger cu suport Bearer ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gym Tracker API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token-ul de acces obtinut de la Keycloak (fara cuvantul 'Bearer', Swagger il adauga singur)."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------------- Pipeline ----------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    await next();
});

app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseCors("AngularClient");

app.UseAuthentication();
app.UseMiddleware<UserProvisioningMiddleware>(); // JIT provisioning - dupa AuthN, inainte de AuthZ/controllere
app.UseAuthorization();

app.MapControllers();

app.Run();
