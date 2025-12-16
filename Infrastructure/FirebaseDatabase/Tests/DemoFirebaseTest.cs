using Entities.Models;

namespace Infrastructure.FirebaseDatabase.Tests;

/// <summary>
/// ДЕМОНСТРАЦИЯ РАБОТЫ С СЕРВИСОМ FIREBASE
/// Ниже подробно представлено, как юзать этот сервис
/// </summary>
public class DemoFirebaseTest : IFirebaseListener
{
    private readonly FirebaseRepository _repo;

    public DemoFirebaseTest()
    {
        var service = new FirebaseService();
        _repo = new FirebaseRepository(service);
    }

    public async Task RunDemoAsync()
    {
        Console.WriteLine("=== Firebase User Demo ===");

        //тестовый юзер подопытный
        var user = new User
        {
            Id = 1,
            Username = "Mamut_rahal",
            Email = "MRahal@fbi.com",
            PasswordHash = "hashed_pass_123",
            Role = Entities.Enums.UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        //ЗДЕСЬ ВЫЗОВ АСИНХРОННЫХ МЕТОДОВ

        //добавление юзера (или перезапись) - работает ваще с любым энтити, не только юзером
        await _repo.SetAsync
        (
            $"users/{user.Id}", //директория в бд - отдел юзерс и сам юзер
            user,                    //передача самого юзера
            this              //просто листенер, везде одинаково
        );

        //чтение юзера
        await _repo.GetAsync<User>($"users/{user.Id}", this);

        //удаление юзера
        //await _repo.DeleteAsync($"users/{user.Id}", this);

        Console.WriteLine("=== Demo finished ===");
    }

    //реализация IFirebaseListener - 3 метода: успех, неудача, полученные данные
    public void OnSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"### Success: {message}");
        Console.ResetColor();
    }

    public void OnError(string reason)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"### Error: {reason}");
        Console.ResetColor();
    }

    //получение данных тут
    public void OnDataSnapshot<T>(T data)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("### Received snapshot:");
        Console.ResetColor();

        if (data == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Snapshot is empty (null)");
            Console.ResetColor();
            return;
        }

        //проверка энтити юзера
        if (data is User receivedUser)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Data recognized as User class.");

            //демонстрация что типо можно на изи получить энтити из данных
            var user = new User
            {
                Id = receivedUser.Id,
                Username = receivedUser.Username,
                Email = receivedUser.Email,
                Role = receivedUser.Role,
                CreatedAt = receivedUser.CreatedAt
            };

            Console.WriteLine("Вот полученный объект User:");
            Console.WriteLine($"  ID: {user.Id}");
            Console.WriteLine($"  Username: {user.Username}");
            Console.WriteLine($"  Email: {user.Email}");
            Console.WriteLine($"  Role: {user.Role}");
            Console.WriteLine($"  CreatedAt: {user.CreatedAt}");

            //демонстрация работы с полученным юзером
            if (user.Email.Contains("@"))
                Console.WriteLine("Email норм");
            else
                Console.WriteLine("Email говно");

            Console.ResetColor();
        }
        else
        {
            //на случай если другой энтити
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                data,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
            Console.ResetColor();
        }
    }
}