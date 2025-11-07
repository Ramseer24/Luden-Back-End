using System.Text.Json;
using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        private readonly FirebaseRepository _firebaseRepo;
        private readonly string _collectionName;

        public GenericRepository(FirebaseRepository firebaseRepo)
        {
            _firebaseRepo = firebaseRepo ?? throw new ArgumentNullException(nameof(firebaseRepo));
            _collectionName = typeof(T).Name.ToLower() + "s"; // users, bills, files ...
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Console.WriteLine($"[Firebase] Добавление {_collectionName}/{entity.Id}...");
            var result = await _firebaseRepo.SetAsync($"{_collectionName}/{entity.Id}", entity, new ConsoleFirebaseListener());

            if (result.IsSuccess)
                Console.WriteLine($"Успешно добавлено в Firebase: {_collectionName}/{entity.Id}");
            else
                Console.WriteLine($"Ошибка добавления: {result.Message}");
        }

        public async Task RemoveAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _firebaseRepo.DeleteAsync($"{_collectionName}/{entity.Id}", new ConsoleFirebaseListener());
            if (result.IsSuccess)
                Console.WriteLine($"Удалено: {_collectionName}/{entity.Id}");
            else
                Console.WriteLine($"Ошибка удаления: {result.Message}");
        }

        public async Task RemoveByIdAsync(ulong id)
        {
            var result = await _firebaseRepo.DeleteAsync($"{_collectionName}/{id}", new ConsoleFirebaseListener());
            if (result.IsSuccess)
                Console.WriteLine($"Удалено: {_collectionName}/{id}");
            else
                Console.WriteLine($"Ошибка удаления: {result.Message}");
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _firebaseRepo.SetAsync($"{_collectionName}/{entity.Id}", entity, new ConsoleFirebaseListener());
            if (result.IsSuccess)
                Console.WriteLine($"Обновлено: {_collectionName}/{entity.Id}");
            else
                Console.WriteLine($"Ошибка обновления: {result.Message}");
        }

        public async Task<T?> GetByIdAsync(ulong id)
        {
            var result = await _firebaseRepo.GetAsync<T>($"{_collectionName}/{id}", new ConsoleFirebaseListener());
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.RawJson))
                return null;

            if (result.RawJson.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                //Если тип File — пробуем определить подтип вручную
                if (typeof(T) == typeof(Entities.Models.File))
                {
                    try
                    {
                        var photo = JsonSerializer.Deserialize<PhotoFile>(result.RawJson);
                        if (photo != null)
                            return photo as T;
                    }
                    catch { /* Игнорируем — не подходит */ }

                    try
                    {
                        var product = JsonSerializer.Deserialize<ProductFile>(result.RawJson);
                        if (product != null)
                            return product as T;
                    }
                    catch { /* Игнорируем — не подходит */ }

                    Console.WriteLine("[Firebase] Не удалось определить подтип File.");
                    return null;
                }

                //Для всех остальных типов — стандартная десериализация
                return JsonSerializer.Deserialize<T>(result.RawJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Firebase десериализация] Ошибка: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var result = await _firebaseRepo.GetAsync<Dictionary<string, T>>($"{_collectionName}", new ConsoleFirebaseListener());
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.RawJson))
                return Enumerable.Empty<T>();

            if (result.RawJson.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                return Enumerable.Empty<T>();

            var map = JsonSerializer.Deserialize<Dictionary<string, T>>(result.RawJson)
                      ?? new Dictionary<string, T>();

            return map.Values;
        }
    }

    // Простой listener для наглядных логов
    internal class ConsoleFirebaseListener : IFirebaseListener
    {
        public void OnSuccess(string message) => Console.WriteLine($"[Firebase норм] {message}");
        public void OnError(string reason) => Console.WriteLine($"[Firebase не норм] {reason}");
        public void OnDataSnapshot<T>(T data) { /* без вывода */ }
    }
}
