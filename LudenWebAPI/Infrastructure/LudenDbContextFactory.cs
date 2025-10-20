using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure
{
    public class LudenDbContextFactory : IDesignTimeDbContextFactory<LudenDbContext>
    {
        public LudenDbContext CreateDbContext(string[] args)
        {
            // Получаем текущую директорию (...\Infrastructure)
            var currentDirectory = Directory.GetCurrentDirectory();
            // Формируем правильный путь к проекту API (...\LudenWebAPI)
            var apiProjectPath = Path.Combine(currentDirectory, "..", "LudenWebAPI");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                // --- ИСПРАВЛЕННЫЙ ПУТЬ ---
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LudenDbContext>();
            var connectionString = configuration.GetConnectionString("LudenDbContext");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Строка подключения 'DefaultConnection' не была найдена. Проверьте, что путь '{apiProjectPath}' к appsettings.json указан верно.");
            }

            optionsBuilder.UseSqlite(connectionString);

            return new LudenDbContext(optionsBuilder.Options);
        }
    }
}