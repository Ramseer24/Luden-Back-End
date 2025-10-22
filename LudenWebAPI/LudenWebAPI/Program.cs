using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.Services;
using Entities.Config;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace LudenWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Config config = new();
            builder.Configuration.Bind(config);
            builder.Services.AddSingleton(config);

            builder.Services.AddDbContext<LudenDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LudenDbContext") ??
                      throw new InvalidOperationException("Connection string 'LudenDbContext' not found.")));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ArtemPetrenko", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    // .AllowCredentials();
                });
            });
            // Configure JWT authentication
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
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IBillRepository, BillRepository>();
            builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
            builder.Services.AddScoped<ITokenService, BaseTokenService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
            builder.Services.AddScoped<IBillService, BillService>();

            builder.Services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();

            WebApplication app = builder.Build();



            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("ArtemRomanovich");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}