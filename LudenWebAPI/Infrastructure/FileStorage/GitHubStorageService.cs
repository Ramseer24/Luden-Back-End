using Application.Abstractions.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace Infrastructure.FileStorage
{
    /// <summary>
    /// Сервис для хранения файлов в GitHub репозитории через GitHub Contents API
    /// Использует GitHub Personal Access Token для аутентификации
    /// </summary>
    public class GitHubStorageService : IFileStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly GitHubStorageConfig _config;
        private const long MaxFileSizeForContentsApi = 50 * 1024 * 1024; // 50MB

        public GitHubStorageService(HttpClient httpClient, GitHubStorageConfig config)
        {
            _httpClient = httpClient;
            _config = config;
            
            // Настройка HttpClient для GitHub API
            _httpClient.BaseAddress = new Uri("https://api.github.com");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LudenWebAPI");
            
            // Добавляем Personal Access Token в заголовки
            if (!string.IsNullOrEmpty(_config.PersonalAccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.PersonalAccessToken);
            }
        }

        public async Task<string> SaveAudioFileAsync(IFormFile file)
        {
            return await SaveFileAsync(file, "audio");
        }

        public async Task<string> SaveImageFileAsync(IFormFile file)
        {
            return await SaveFileAsync(file, "image");
        }

        public async Task<string> SaveImageFileAsync(Stream fileStream, string fileName, string category = "image")
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream), "File stream not found");

            // Читаем файл в байты
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            // Проверка размера
            if (fileBytes.Length > MaxFileSizeForContentsApi)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSizeForContentsApi / 1024 / 1024}MB for GitHub Contents API. Use Git LFS for larger files.");
            }

            // Генерируем путь для файла
            DateTime now = DateTime.UtcNow;
            string year = now.Year.ToString();
            string month = now.Month.ToString();
            string fileExtension = Path.GetExtension(fileName);
            string fileNameGuid = $"{Guid.NewGuid()}{fileExtension}";
            string filePath = $"uploads/{category}/{year}/{month}/{fileNameGuid}";

            // Конвертируем в base64 для GitHub API
            string base64Content = Convert.ToBase64String(fileBytes);

            // Проверяем наличие токена
            if (string.IsNullOrEmpty(_config.PersonalAccessToken))
            {
                throw new Exception("GitHub Personal Access Token is not configured");
            }

            // Создаем запрос на загрузку файла
            var requestBody = new
            {
                message = $"Upload {category} file: {fileNameGuid}",
                content = base64Content,
                branch = _config.Branch
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, 
                $"repos/{_config.RepositoryOwner}/{_config.RepositoryName}/contents/{filePath}")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to upload file to GitHub: {response.StatusCode} - {responseContent}");
            }

            // Возвращаем путь к файлу (будет использоваться для формирования URL)
            return filePath;
        }

        public async Task<string> SaveVideoFileAsync(IFormFile file)
        {
            return await SaveFileAsync(file, "video");
        }

        private async Task<string> SaveFileAsync(IFormFile file, string category)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file), "File not found");

            using var stream = file.OpenReadStream();
            return await SaveImageFileAsync(stream, file.FileName, category);
        }

        public async Task<byte[]> GetFile(string filePath)
        {
            try
            {
                // Проверяем наличие токена
                if (string.IsNullOrEmpty(_config.PersonalAccessToken))
                {
                    throw new Exception("GitHub Personal Access Token is not configured");
                }

                // Получаем информацию о файле через GitHub Contents API
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"repos/{_config.RepositoryOwner}/{_config.RepositoryName}/contents/{filePath}?ref={_config.Branch}");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new FileNotFoundException($"File not found in GitHub repository: '{filePath}'");
                }

                // Десериализуем ответ
                var fileInfo = JsonSerializer.Deserialize<GitHubFileResponse>(responseContent);
                if (fileInfo == null || string.IsNullOrEmpty(fileInfo.content))
                {
                    throw new FileNotFoundException($"File content not found for path '{filePath}'");
                }

                // Декодируем base64
                return Convert.FromBase64String(fileInfo.content);
            }
            catch (FormatException ex)
            {
                throw new Exception($"Invalid base64 data for file path '{filePath}'", ex);
            }
        }

        public async Task<bool> DeleteFile(string filePath)
        {
            try
            {
                // Проверяем наличие токена
                if (string.IsNullOrEmpty(_config.PersonalAccessToken))
                {
                    throw new Exception("GitHub Personal Access Token is not configured");
                }

                // Сначала получаем SHA файла (требуется для удаления)
                var getRequest = new HttpRequestMessage(HttpMethod.Get,
                    $"repos/{_config.RepositoryOwner}/{_config.RepositoryName}/contents/{filePath}?ref={_config.Branch}");

                var getResponse = await _httpClient.SendAsync(getRequest);
                if (!getResponse.IsSuccessStatusCode)
                {
                    return false; // Файл уже не существует
                }

                var fileInfo = JsonSerializer.Deserialize<GitHubFileResponse>(await getResponse.Content.ReadAsStringAsync());
                if (fileInfo == null || string.IsNullOrEmpty(fileInfo.sha))
                {
                    return false;
                }

                // Удаляем файл
                var deleteBody = new
                {
                    message = $"Delete file: {filePath}",
                    sha = fileInfo.sha,
                    branch = _config.Branch
                };

                var json = JsonSerializer.Serialize(deleteBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var deleteRequest = new HttpRequestMessage(HttpMethod.Delete,
                    $"repos/{_config.RepositoryOwner}/{_config.RepositoryName}/contents/{filePath}")
                {
                    Content = content
                };

                var deleteResponse = await _httpClient.SendAsync(deleteRequest);
                return deleteResponse.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public string GetPublicUrl(string filePath)
        {
            // Возвращаем прямую ссылку на raw.githubusercontent.com
            // Также можно использовать API endpoint для проксирования
            return $"https://raw.githubusercontent.com/{_config.RepositoryOwner}/{_config.RepositoryName}/{_config.Branch}/{filePath}";
        }


        private static string GetContentType(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                _ => "application/octet-stream"
            };
        }

        private class GitHubFileResponse
        {
            public string content { get; set; } = string.Empty;
            public string sha { get; set; } = string.Empty;
            public string encoding { get; set; } = string.Empty;
        }
    }

    public class GitHubStorageConfig
    {
        public string RepositoryOwner { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
        public string Branch { get; set; } = "main";
        public string PersonalAccessToken { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
    }
}

