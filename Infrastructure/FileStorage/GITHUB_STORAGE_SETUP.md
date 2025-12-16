# Настройка GitHub Storage для хранения файлов

## Обзор

Система использует GitHub репозиторий для хранения файлов через GitHub Contents API. Файлы загружаются в публичный репозиторий и доступны через прямые ссылки `raw.githubusercontent.com` или через API endpoint.

## Преимущества

- ✅ **Бесплатно** - GitHub предоставляет бесплатное хранилище для публичных репозиториев
- ✅ **CDN** - GitHub использует CDN для быстрой доставки файлов
- ✅ **Версионирование** - все файлы автоматически версионируются через Git
- ✅ **Прямые ссылки** - файлы доступны напрямую через `raw.githubusercontent.com`
- ✅ **Простая настройка** - используется Personal Access Token вместо GitHub App

## Шаги настройки

### 1. Создание GitHub репозитория

1. Перейдите на [GitHub](https://github.com) и создайте новый публичный репозиторий
2. Назовите репозиторий (например: `luden-files`)
3. Выберите **Public** (публичный)
4. Инициализируйте репозиторий с README (опционально)
5. Запомните:
   - **Repository Owner** (ваш username или organization)
   - **Repository Name** (название репозитория)

### 2. Создание Personal Access Token (PAT)

1. Перейдите в **Settings** вашего GitHub аккаунта
2. Выберите **Developer settings** → **Personal access tokens** → **Tokens (classic)**
3. Нажмите **Generate new token** → **Generate new token (classic)**
4. Заполните форму:
   - **Note**: `Luden File Storage` (или любое другое описание)
   - **Expiration**: выберите срок действия (рекомендуется `No expiration` для продакшена)
   - **Select scopes**:
     - ✅ **repo** (Full control of private repositories)
       - ✅ **repo:status**
       - ✅ **repo_deployment**
       - ✅ **public_repo**
       - ✅ **repo:invite**
       - ✅ **security_events**
5. Нажмите **Generate token**
6. **⚠️ ВАЖНО**: Скопируйте токен сразу! Он больше не будет показан.

### 3. Настройка appsettings.json

Обновите секцию `GitHubStorage` в `appsettings.json`:

```json
{
  "GitHubStorage": {
    "RepositoryOwner": "your-github-username",
    "RepositoryName": "luden-files",
    "Branch": "main",
    "PersonalAccessToken": "ghp_your_token_here",
    "BaseUrl": null
  }
}
```

**Параметры:**
- `RepositoryOwner` - ваш GitHub username или organization name
- `RepositoryName` - название репозитория для хранения файлов
- `Branch` - ветка Git (обычно `main` или `master`)
- `PersonalAccessToken` - ваш Personal Access Token (начинается с `ghp_`)
- `BaseUrl` - опционально, для кастомного домена (если используется)

### 4. Безопасность токена

**⚠️ ВАЖНО**: 
- Никогда не коммитьте токен в репозиторий!
- Используйте User Secrets для разработки:
  ```bash
  dotnet user-secrets set "GitHubStorage:PersonalAccessToken" "ghp_your_token_here"
  ```
- Для продакшена используйте переменные окружения или Azure Key Vault

## Структура хранения файлов

Файлы сохраняются в следующей структуре:

```
uploads/
  ├── image/
  │   ├── 2024/
  │   │   ├── 12/
  │   │   │   └── {guid}.jpg
  │   │   └── 11/
  │   └── 2025/
  ├── audio/
  └── video/
```

## URL формат

### Прямая ссылка (рекомендуется для фронтенда):
```
https://raw.githubusercontent.com/{owner}/{repo}/{branch}/uploads/{path}
```

Пример:
```
https://raw.githubusercontent.com/username/luden-files/main/uploads/image/2024/12/abc123.jpg
```

### API проксирование:
```
GET /api/blob/{id}
GET /api/blob/{id}?redirect=true  # Редирект на прямую ссылку
GET /api/blob/{id}?dataUri=true  # Возвращает data URI
```

## Ограничения

- **Максимальный размер файла**: 100MB (рекомендуется до 50MB)
- **Rate limit**: 5000 запросов/час для аутентифицированных пользователей
- **Размер репозитория**: 
  - Бесплатный план: 1GB (рекомендуется)
  - Pro план: 50GB

## Troubleshooting

### Ошибка: "GitHub Personal Access Token is not configured"
- Проверьте, что токен указан в `appsettings.json` или User Secrets
- Убедитесь, что токен не истек
- Проверьте формат токена (должен начинаться с `ghp_`)

### Ошибка: "Failed to upload file to GitHub"
- Проверьте права доступа токена (должен быть scope `repo`)
- Убедитесь, что репозиторий существует и доступен
- Проверьте, что файл не превышает 100MB
- Проверьте, что токен имеет права на запись в репозиторий

### Ошибка: "File not found in GitHub repository"
- Убедитесь, что файл был успешно загружен
- Проверьте путь к файлу
- Проверьте, что используется правильная ветка

### Ошибка: "File size exceeds maximum"
- Для файлов >50MB рекомендуется использовать Git LFS
- Или используйте альтернативное хранилище для больших файлов

### Ошибка: "API rate limit exceeded"
- GitHub ограничивает количество запросов до 5000/час для аутентифицированных пользователей
- Подождите час или используйте несколько токенов

## Безопасность

1. **Personal Access Token**:
   - Храните в безопасном месте
   - Не коммитьте в репозиторий
   - Используйте User Secrets для разработки
   - Используйте переменные окружения для продакшена
   - Регулярно обновляйте токен

2. **Репозиторий**:
   - Для приватных файлов используйте приватный репозиторий
   - Настройте правила доступа через GitHub repository settings

3. **Токен права**:
   - Минимизируйте права доступа
   - Используйте только необходимые scopes (`repo`)

## Примеры использования

### На фронтенде (React/Vue/Angular):

```javascript
// Прямая ссылка
const imageUrl = `https://raw.githubusercontent.com/${owner}/${repo}/${branch}/${filePath}`;
<img src={imageUrl} alt="Image" />

// Или через API с редиректом
const imageUrl = `/api/blob/${fileId}?redirect=true`;
<img src={imageUrl} alt="Image" />

// Или через API проксирование
const imageUrl = `/api/blob/${fileId}`;
<img src={imageUrl} alt="Image" />
```

### Загрузка файла через API:

```bash
POST /api/File/product/{productId}
Content-Type: multipart/form-data

file: [binary data]
fileType: "screenshot"
```

## Использование User Secrets (для разработки)

Вместо хранения токена в `appsettings.json`, используйте User Secrets:

```bash
# Установите токен
dotnet user-secrets set "GitHubStorage:PersonalAccessToken" "ghp_your_token_here"

# Проверьте
dotnet user-secrets list
```

Токен будет автоматически загружен из User Secrets и переопределит значение из `appsettings.json`.

## Использование переменных окружения (для продакшена)

Для продакшена используйте переменные окружения:

```bash
# Linux/Mac
export GitHubStorage__PersonalAccessToken="ghp_your_token_here"

# Windows PowerShell
$env:GitHubStorage__PersonalAccessToken="ghp_your_token_here"

# Windows CMD
set GitHubStorage__PersonalAccessToken=ghp_your_token_here
```

Или в `appsettings.Production.json` (не коммитьте токен!):

```json
{
  "GitHubStorage": {
    "PersonalAccessToken": "ghp_your_token_here"
  }
}
```

## Дополнительные ресурсы

- [GitHub Personal Access Tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token)
- [GitHub Contents API](https://docs.github.com/en/rest/repos/contents)
- [GitHub Rate Limits](https://docs.github.com/en/rest/overview/resources-in-the-rest-api#rate-limiting)
- [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

