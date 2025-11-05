using System.Text;
using System.Text.Json;

namespace Infrastructure.FirebaseDatabase;

//класс для общения с бдшкой напрямую
public class FirebaseService
{
    private readonly string _databaseUrl;
    private readonly string _secretToken;
    private readonly HttpClient _httpClient;

    public FirebaseService()
    {
        //здесь типо получение токена для базы данных из джсона
        var configPath = Path.Combine(AppContext.BaseDirectory, "FirebaseDatabase", "firebase.config.json");
        var json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<FirebaseConfig>(json);

        //получение ссылки из джсона
        _databaseUrl = config?.DatabaseUrl?.TrimEnd('/') ?? throw new Exception("Missing databaseUrl in config");
        _secretToken = config?.SecretToken ?? throw new Exception("Missing secretToken in config");

        _httpClient = new HttpClient();
    }

    //главный метод для контакта с бдшкой
    public async Task<FirebaseResult> SendAsync(string path, HttpMethod method, object? data = null)
    {
        //var url = $"{_databaseUrl}/{path}.json";
        var url = $"{_databaseUrl}/{path}.json?auth={_secretToken}"; //формируем юрл для запроса на бд с токеном
        var body = data != null ? JsonSerializer.Serialize(data) : "{}"; //формируем джсон на отправку
        var request = new HttpRequestMessage(method, url) //формируем пакет с готовой джсон + с юрл
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await _httpClient.SendAsync(request); //отправляет готовый пакет
            var json = await response.Content.ReadAsStringAsync(); //ждем результатов

            return new FirebaseResult //возвращаем результат куда надо
            {
                IsSuccess = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode ? "Success" : $"Error {response.StatusCode}",
                RawJson = json
            };
        }
        catch (Exception ex)
        {
            return new FirebaseResult { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<FirebaseResult> GetAsync(string path)
    {
        return await SendAsync(path, HttpMethod.Get);
    }

    public async Task<FirebaseResult> PostAsync(string path, object data)
    {
        return await SendAsync(path, HttpMethod.Post, data);
    }

    public async Task<FirebaseResult> PutAsync(string path, object data)
    {
        return await SendAsync(path, HttpMethod.Put, data);
    }

    public async Task<FirebaseResult> DeleteAsync(string path)
    {
        return await SendAsync(path, HttpMethod.Delete);
    }

    private class FirebaseConfig
    {
        public string DatabaseUrl { get; set; }
        public string SecretToken { get; set; }
    }
}