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
        private static readonly Random _random = new Random();
        private static readonly object _randomLock = new object();

        public GenericRepository(FirebaseRepository firebaseRepo)
        {
            _firebaseRepo = firebaseRepo ?? throw new ArgumentNullException(nameof(firebaseRepo));
            _collectionName = typeof(T).Name.ToLower() + "s"; // users, bills, files ...
        }

        /// <summary>
        /// Генерирует уникальный ID на основе Unix timestamp в миллисекундах.
        /// Если ID уже существует, добавляет случайное число для избежания коллизий.
        /// </summary>
        private async Task<ulong> GenerateIdAsync()
        {
            // Unix timestamp в миллисекундах
            // Помещается в ulong до года ~584,942,417,355 (584 миллиарда лет)
            ulong baseId = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Проверяем, не существует ли уже запись с таким ID
            var existing = await GetByIdAsync(baseId);
            if (existing == null)
            {
                return baseId;
            }

            // Если ID уже существует (крайне редкий случай), добавляем случайное число
            // Используем lock для потокобезопасности Random
            int randomIncrement;
            lock (_randomLock)
            {
                randomIncrement = _random.Next(1, 10000); // От 1 до 9999
            }

            ulong newId = baseId + (ulong)randomIncrement;

            // Дополнительная проверка на коллизию (на практике не должна произойти)
            existing = await GetByIdAsync(newId);
            if (existing != null)
            {
                // Если и с инкрементом есть коллизия, используем timestamp + больший случайный номер
                lock (_randomLock)
                {
                    randomIncrement = _random.Next(10000, 99999);
                }
                newId = baseId + (ulong)randomIncrement;
            }

            return newId;
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Генерация ID для Firebase режима, если ID не установлен
            if (entity.Id == 0)
            {
                entity.Id = await GenerateIdAsync();
                Console.WriteLine($"[Firebase] Сгенерирован ID: {entity.Id} для {_collectionName}");
            }

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
                        var photo = JsonSerializer.Deserialize<PhotoFile>(result.RawJson, JsonOptions.Default);
                        if (photo != null)
                            return photo as T;
                    }
                    catch { /* Игнорируем — не подходит */ }

                    try
                    {
                        var product = JsonSerializer.Deserialize<ProductFile>(result.RawJson, JsonOptions.Default);
                        if (product != null)
                            return product as T;
                    }
                    catch { /* Игнорируем — не подходит */ }

                    Console.WriteLine("[Firebase] Не удалось определить подтип File.");
                    return null;
                }

                //Для всех остальных типов — стандартная десериализация
                return JsonSerializer.Deserialize<T>(result.RawJson, JsonOptions.Default);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Firebase десериализация] Ошибка: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var result = await _firebaseRepo.GetAsync<Dictionary<string, T>>(_collectionName, new ConsoleFirebaseListener());

            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.RawJson))
                return Enumerable.Empty<T>();

            var json = result.RawJson.Trim();

            if (json.Equals("null", StringComparison.OrdinalIgnoreCase))
                return Enumerable.Empty<T>();

            try
            {
                // Попробуем как Dictionary<string, T>
                var map = JsonSerializer.Deserialize<Dictionary<string, T>>(json, JsonOptions.Default);
                if (map != null)
                    return map.Values;
            }
            catch { }

            try
            {
                // Попробуем как массив
                var list = JsonSerializer.Deserialize<List<T>>(json, JsonOptions.Default);
                if (list != null)
                    return list;
            }
            catch { }

            try
            {
                // Попробуем как одиночный объект
                var single = JsonSerializer.Deserialize<T>(json, JsonOptions.Default);
                if (single != null)
                    return new List<T> { single };
            }
            catch { }

            return Enumerable.Empty<T>();
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
