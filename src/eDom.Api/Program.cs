using System.Text;
using eDom.Application;
using eDom.Api.Middleware;
using eDom.Core.Interfaces;
using eDom.Api.Services;
using eDom.Infrastructure;
using eDom.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console()
      .WriteTo.File("logs/edom-.txt", rollingInterval: RollingInterval.Day));

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── HttpContext / CurrentUser ─────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ── Application layer ────────────────────────────────────────────────────────
builder.Services.AddApplication();

// ── Infrastructure layer ─────────────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "eDom API", Version = "v1" });
    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header — Bearer {token}"
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSection["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret non configurato");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var hasAuthHeader = context.Request.Headers.TryGetValue("Authorization", out var authHeader);
                var authPreview = hasAuthHeader
                    ? authHeader.ToString()[..Math.Min(authHeader.ToString().Length, 24)]
                    : "<none>";

                Log.Information(
                    "JWT message received. Path={Path} HasAuthorizationHeader={HasAuthorizationHeader} AuthorizationPreview={AuthorizationPreview}",
                    context.HttpContext.Request.Path,
                    hasAuthHeader,
                    authPreview);

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Log.Warning(context.Exception,
                    "JWT authentication failed. Path={Path} Message={Message}",
                    context.HttpContext.Request.Path,
                    context.Exception.Message);
                return Task.CompletedTask;
            },

            OnTokenValidated = async context =>
            {
                var uidClaim = context.Principal?.FindFirst("uid")?.Value;
                if (!int.TryParse(uidClaim, out var userId))
                {
                    context.Fail("Token senza uid valido.");
                    return;
                }

                var iatClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
                if (!long.TryParse(iatClaim, out var iatUnix))
                {
                    context.Fail("Token senza iat valido.");
                    return;
                }

                var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnix).UtcDateTime;
                var db = context.HttpContext.RequestServices.GetRequiredService<HctDbContext>();
                var state = await db.UserTokenStates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserId == userId, context.HttpContext.RequestAborted);

                if (state?.InvalidBeforeUtc is DateTime invalidBefore)
                {
                    // `iat` is second-based while DB timestamps can include milliseconds and unspecified kind.
                    var invalidBeforeUtc = invalidBefore.Kind switch
                    {
                        DateTimeKind.Utc => invalidBefore,
                        DateTimeKind.Local => invalidBefore.ToUniversalTime(),
                        _ => DateTime.SpecifyKind(invalidBefore, DateTimeKind.Utc)
                    };

                    // `iat` has second precision. Normalize DB cutoff to second precision too,
                    // otherwise tokens issued in the same second as logout can be rejected.
                    var invalidBeforeSecond = new DateTime(
                        invalidBeforeUtc.Year,
                        invalidBeforeUtc.Month,
                        invalidBeforeUtc.Day,
                        invalidBeforeUtc.Hour,
                        invalidBeforeUtc.Minute,
                        invalidBeforeUtc.Second,
                        DateTimeKind.Utc);

                    // Reject only tokens issued before the normalized invalidation second.
                    if (issuedAt < invalidBeforeSecond)
                    {
                        context.Fail("Token invalidato da logout globale.");
                        return;
                    }
                }

                Log.Information(
                    "JWT token validated. Path={Path} UserId={UserId} IssuedAtUtc={IssuedAtUtc}",
                    context.HttpContext.Request.Path,
                    userId,
                    issuedAt);
            },

            OnChallenge = context =>
            {
                if (!string.IsNullOrWhiteSpace(context.ErrorDescription))
                {
                    Log.Warning(
                        "JWT challenge: Path={Path} Error={Error} Description={Description}",
                        context.HttpContext.Request.Path,
                        context.Error,
                        context.ErrorDescription);
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                      ?? ["http://localhost:4200"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// Apply pending EF Core migrations only when explicitly enabled.
// This avoids startup crashes when migration metadata is temporarily inconsistent.
var applyMigrationsOnStartup = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
if (app.Environment.IsDevelopment() && applyMigrationsOnStartup)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HctDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "EF Core automatic migrations failed at startup. Start continues because Database:ApplyMigrationsOnStartup is optional.");
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseSerilogRequestLogging();
app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "eDom API v1"));
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("AngularClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
