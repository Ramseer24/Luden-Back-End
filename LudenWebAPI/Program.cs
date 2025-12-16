using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.Services;
using Entities.Config;
using Entities.Models;
using FileSignatures;
using FileSignatures.Formats;
using Infrastructure;
using Infrastructure.FileStorage;
using Infrastructure.FirebaseDatabase;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using Product = Entities.Models.Product;
using ProductService = Application.Services.ProductService;

namespace LudenWebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Config config = new();
        builder.Configuration.Bind(config);
        StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeOptions")["SecretKey"];
        builder.Services.AddSingleton(config);

        builder.Services.AddSingleton<FirebaseService>();
        builder.Services.AddScoped<FirebaseRepository>();

        builder.Services.AddScoped<IUserRepository, UserRepository>(sp =>
            new UserRepository(sp.GetRequiredService<FirebaseRepository>()));

        builder.Services.AddScoped<IBillRepository, BillRepository>(sp =>
            new BillRepository(sp.GetRequiredService<FirebaseRepository>()));

        builder.Services.AddScoped<IFileRepository, FileRepository>(sp =>
            new FileRepository(sp.GetRequiredService<FirebaseRepository>()));

        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>(sp =>
            new PaymentRepository(sp.GetRequiredService<FirebaseRepository>()));

        builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>(sp =>
            new FavoriteRepository(sp.GetRequiredService<FirebaseRepository>()));

        builder.Services.AddScoped<IGenericRepository<Product>, GenericRepository<Product>>(sp =>
            new GenericRepository<Product>(sp.GetRequiredService<FirebaseRepository>()));

        builder.Services.AddScoped<IGenericRepository<License>, GenericRepository<License>>(sp =>
            new GenericRepository<License>(sp.GetRequiredService<FirebaseRepository>()));

        builder.Services.AddScoped<IGenericRepository<BillItem>, GenericRepository<BillItem>>(sp =>
            new GenericRepository<BillItem>(sp.GetRequiredService<FirebaseRepository>()));

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ArtemPetrenko", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });

        builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
        builder.Services.AddScoped<ITokenService, BaseTokenService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        builder.Services.AddScoped<IBillService, BillService>();
        builder.Services.AddScoped<IFileService, Application.Services.FileService>();
        builder.Services.AddScoped<IStripeService, StripeService>();
        builder.Services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IFavoriteService, FavoriteService>();

        builder.Services.AddHttpClient();

        var githubStorageConfig = new GitHubStorageConfig();
        builder.Configuration.GetSection("GitHubStorage").Bind(githubStorageConfig);

        builder.Services.AddSingleton<IFileStorageService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            return new GitHubStorageService(httpClient, githubStorageConfig);
        });

        builder.Services.AddSingleton<IFileFormatInspector>(new FileFormatInspector(
            [new Png(), new Jpeg(), new Gif()]));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Luden Web API",
                Version = "v1"
            });
            c.EnableAnnotations();
            c.CustomSchemaIds(type => type.FullName);
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        WebApplication app = builder.Build();

        // Seed Data
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var productRepo = services.GetRequiredService<IGenericRepository<Product>>();
                Infrastructure.Data.DataSeeder.SeedProductsAsync(productRepo).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred seeding the DB: {ex.Message}");
            }
        }

        app.UseStaticFiles();
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/uploads"
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Luden Web API v1");
                c.RoutePrefix = "swagger"; 
                c.DisplayRequestDuration();
            });

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("ArtemPetrenko");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}