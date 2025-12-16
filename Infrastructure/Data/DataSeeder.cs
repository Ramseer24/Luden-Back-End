using Entities.Models;
using Application.Abstractions.Interfaces.Repository;

namespace Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedProductsAsync(IGenericRepository<Product> productRepository)
        {
            var products = await productRepository.GetAllAsync();
            if (products.Any())
            {
                return; // Data already exists
            }

            var sampleProducts = new List<Product>();
            var random = new Random();
            var developers = new[] { "Ubisoft", "EA", "Activision", "CD Projekt Red", "Bethesda", "Rockstar Games", "Nintendo", "Sony", "Microsoft", "Valve" };
            var publishers = new[] { "Ubisoft", "EA", "Activision Blizzard", "CD Projekt", "Bethesda Softworks", "Take-Two", "Nintendo", "Sony Interactive", "Xbox Game Studios", "Valve" };
            var categories = new[] { "Action", "RPG", "Shooter", "Strategy", "Adventure", "Simulation", "Sports", "Racing", "Puzzle", "Fighting" };
            
            // Generate 50 sample products
            for (int i = 1; i <= 50; i++)
            {
                var price = random.Next(10, 100) + 0.99m;
                var discount = random.Next(0, 5) == 0 ? random.Next(10, 80) : 0; // 20% chance of discount
                var dev = developers[random.Next(developers.Length)];
                var pub = publishers[random.Next(publishers.Length)];
                var cat = categories[random.Next(categories.Length)];
                var releaseDate = DateTime.UtcNow.AddDays(-random.Next(100, 3650)); // Released within last 10 years

                sampleProducts.Add(new Product
                {
                    Name = $"Game Title {i}",
                    Description = $"This is a description for Game Title {i}. It is a very interesting game in the {cat} genre developed by {dev}.",
                    Price = price,
                    Stock = random.Next(10, 1000),
                    RegionId = 1, // Assuming Region ID 1 exists (e.g. Global or similar)
                    DiscountPercentage = discount,
                    Developer = dev,
                    Publisher = pub,
                    ReleaseDate = releaseDate,
                    Category = cat,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    SalesCount = random.Next(0, 5000)
                });
            }

            foreach (var product in sampleProducts)
            {
                await productRepository.AddAsync(product);
            }
        }
    }
}

