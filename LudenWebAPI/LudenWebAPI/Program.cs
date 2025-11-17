using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.Services;
using Entities.Config;
using Entities.Models;
using FileSignatures;
using FileSignatures.Formats;
using Infrastructure.FileStorage;
using Infrastructure.FirebaseDatabase;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        var stripeOptions = new StripeOptions();
        builder.Configuration.Bind(config);
        StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeOptions")["SecretKey"];
        builder.Services.AddSingleton(config);

        // Firebase режим
        builder.Services.AddSingleton<FirebaseService>();      // Сервис для REST-запросов
        builder.Services.AddScoped<FirebaseRepository>();      // Универсальный репозиторий

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

        // =============================
        // Остальные сервисы
        // =============================
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

        // JWT
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

        // Сервисы бизнес-логики
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

        // File storage and validation services
        // Используем GitHub репозиторий для хранения файлов
        builder.Services.AddHttpClient();

        // Загружаем конфигурацию GitHub Storage
        var githubStorageConfig = new GitHubStorageConfig();
        builder.Configuration.GetSection("GitHubStorage").Bind(githubStorageConfig);

        // Регистрируем GitHubStorageService для обоих режимов
        builder.Services.AddSingleton<IFileStorageService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            return new GitHubStorageService(httpClient, githubStorageConfig);
        });

        builder.Services.AddSingleton<IFileFormatInspector>(new FileFormatInspector(
            [new Png(), new Jpeg(), new Gif()]));

        // =============================
        // Swagger/OpenAPI
        // =============================
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Luden Web API",
                Version = "v1",
                Description = "API для управления пользователями, продуктами, счетами и файлами",
                Contact = new OpenApiContact
                {
                    Name = "Luden Team"
                }
            });

            // Настройка для работы с файловыми загрузками (multipart/form-data)
            c.EnableAnnotations();
            c.CustomSchemaIds(type => type.FullName);

            // Добавляем JWT авторизацию в Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

        // =============================
        // Middleware
        // =============================
        WebApplication app = builder.Build();

        app.UseStaticFiles(); // wwwroot
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/uploads"
        });

        // Swagger/OpenAPI только в Development режиме
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Luden Web API v1");
                c.RoutePrefix = "swagger"; // Swagger будет доступен по /swagger
                c.DisplayRequestDuration();
            });

            // Редирект с корневого пути на Swagger
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        }

        // HTTPS редирект
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("ArtemPetrenko");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            ////асинхронный тест и блок потока до завершения
            //RunFirebaseRepoTestsAsync(services).GetAwaiter().GetResult();
        }

        app.Run();
    }

    //типо тесты для работы с файрбазе
    private static async Task RunFirebaseRepoTestsAsync(IServiceProvider services)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Firebase Repository Tests Start ===");
        Console.ResetColor();

        var users = services.GetRequiredService<IUserRepository>();
        var bills = services.GetRequiredService<IBillRepository>();
        var files = services.GetRequiredService<IFileRepository>();
        var payments = services.GetRequiredService<IPaymentRepository>();

        //UserRepository
        var testUser = new User
        {
            Id = 9999,
            Username = "TestUser",
            Email = "testuser@luden.com",
            PasswordHash = "hashed123",
            Role = Entities.Enums.UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
        await users.AddAsync(testUser);
        Console.WriteLine("UserRepository.AddAsync() работает.");

        var fetchedUser = await users.GetByIdAsync(testUser.Id);
        Console.WriteLine(fetchedUser != null ? "UserRepository.GetByIdAsync() работает." : "Не найден пользователь.");

        fetchedUser!.Username = "UpdatedTester";
        await users.UpdateAsync(fetchedUser);
        Console.WriteLine("UserRepository.UpdateAsync() работает.");

        //BillRepository
        var testBill = new Bill
        {
            Id = 8888,
            UserId = testUser.Id,
            CreatedAt = DateTime.UtcNow,
            Status = Entities.Enums.BillStatus.Paid
        };
        await bills.AddAsync(testBill);
        Console.WriteLine("BillRepository.AddAsync() работает.");

        var userBills = await bills.GetBillsByUserIdAsync(testUser.Id);
        Console.WriteLine(userBills.Any() ? "BillRepository.GetBillsByUserIdAsync() работает." : "Счета пользователя не найдены.");

        //FileRepository
        var photo = new ImageFile
        {
            Id = 7777,
            FileName = "test_avatar.png",
            UserId = testUser.Id,
            Width = 512,
            Height = 512,
            CreatedAt = DateTime.UtcNow
        };
        await files.AddAsync(photo);
        Console.WriteLine("FileRepository.AddAsync() работает.");

        var fetchedPhoto = await files.GetImageFileByIdAsync(photo.Id);
        Console.WriteLine(fetchedPhoto != null ? "FileRepository.GetByIdAsync() работает." : "Фото не найдено.");

        //PaymentRepository
        var testPayment = new PaymentOrder
        {
            Id = 6666,
            ProviderTransactionId = "TEST-TRANSACTION-XYZ",
            Provider = "Stripe",
            Success = true,
            AmountInMinorUnits = 1999, // $19.99 в центах
            Currency = "USD",
            CreatedAt = DateTime.UtcNow,
            DeliveredAt = DateTime.UtcNow,
            TokensAmount = 100,
            UserId = testUser.Id,
            UpdatedAt = null
        };
        await payments.AddAsync(testPayment);
        Console.WriteLine("PaymentRepository.AddAsync() работает.");

        var exists = await payments.ExistsByTransactionIdAsync("TEST-TRANSACTION-XYZ");
        Console.WriteLine(exists ? "PaymentRepository.ExistsByTransactionIdAsync() работает." : "Payment не найден.");

        //Очистка данных
        await files.RemoveAsync(photo);
        await bills.RemoveAsync(testBill);
        await payments.RemoveAsync(testPayment);
        await users.RemoveAsync(fetchedUser);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== Все Firebase Repository Tests завершены успешно ===");
        Console.ResetColor();
    }
}