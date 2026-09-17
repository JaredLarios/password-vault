using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PasswordVault.Auth;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using PasswordVault.Common.Database;
using PasswordVault.Common.Interfaces;
using PasswordVault.Common.Repositories;
using PasswordVault.Users;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

string dbConnectionString = builder.Configuration["DB_URI"]
    ?? throw new InvalidOperationException("JWT_SECRET_KEY is not configured.");

string cryptoKey = builder.Configuration["CRYPTO_DB_SECRET_KEY"]
    ?? throw new InvalidOperationException(
        "CRYPTO_DB_SECRET_KEY is not configured.");

string middlewareKey = builder.Configuration["CRYPTO_MIDDLEWARE_SECRET_KEY"]
    ?? throw new InvalidOperationException(
        "CRYPTO_MIDDLEWARE_SECRET_KEY is not configured.");

IConfigurationSection jwtSettings = builder.Configuration.GetSection("JWT");

// Add services to the container.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(
        policy =>
        {
            policy
                .WithOrigins(jwtSettings["Audience"])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        })
);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConnectionString)
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    string secretKey = builder.Configuration["JWT_SECRET_KEY"]
        ?? throw new InvalidOperationException("JWT_SECRET_KEY is not configured.");


    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("auth_token", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddKeyedSingleton<IHash>(
    "sha256",
    new Sha256Repository());

builder.Services.AddKeyedSingleton<IHash>(
    "sha1",
    new Sha1Repository());

builder.Services.AddKeyedSingleton<IHash>(
    "argon2",
    new Argon2Repository());

builder.Services.AddKeyedSingleton<ICrypto>(
    "middleware",
    new FernetRepository(middlewareKey));

builder.Services.AddKeyedSingleton<ICrypto>(
    "services",
    new FernetRepository(cryptoKey));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "docs";
        options.SwaggerEndpoint("/openapi/v1.json", "Password Vault API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
