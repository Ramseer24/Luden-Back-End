# Скрипт для загрузки обложки Cyberpunk через API
$baseUrl = "http://localhost:5195"
$filePath = "E:\Dodikuser\ХРЮНЕШНИК\LudenProject\Luden\LudenWebAPI\LudenWebAPI\wwwroot\uploads\products\cyberpunk-cover.jpg"

Write-Host "Uploading Cyberpunk cover via API..." -ForegroundColor Green

# Шаг 1: Указать ID продукта
Write-Host "`nStep 1: Enter Product ID" -ForegroundColor Yellow
$productId = Read-Host "Enter Cyberpunk Product ID"

if ([string]::IsNullOrWhiteSpace($productId)) {
    Write-Host "Error: Product ID is required!" -ForegroundColor Red
    exit 1
}

# Шаг 2: Получить токен авторизации (нужен админ или модератор)
Write-Host "`nStep 2: Getting authorization token..." -ForegroundColor Yellow
Write-Host "Please provide admin credentials:" -ForegroundColor Cyan
$email = Read-Host "Email"
$password = Read-Host "Password" -AsSecureString
$passwordText = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

try {
    $loginBody = @{
        email = $email
        password = $passwordText
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Authorization/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token

    if ($null -eq $token) {
        Write-Host "Error: Failed to get authorization token" -ForegroundColor Red
        exit 1
    }

    Write-Host "Token obtained successfully" -ForegroundColor Green
} catch {
    Write-Host "Error: Login failed" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Шаг 3: Загрузить файл
Write-Host "`nStep 3: Uploading file..." -ForegroundColor Yellow

if (!(Test-Path $filePath)) {
    Write-Host "Error: File not found at $filePath" -ForegroundColor Red
    exit 1
}

try {
    $headers = @{
        Authorization = "Bearer $token"
    }

    $fileBytes = [System.IO.File]::ReadAllBytes($filePath)
    $fileName = [System.IO.Path]::GetFileName($filePath)

    $boundary = [System.Guid]::NewGuid().ToString()
    $LF = "`r`n"

    $bodyLines = (
        "--$boundary",
        "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"",
        "Content-Type: image/jpeg$LF",
        [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($fileBytes),
        "--$boundary",
        "Content-Disposition: form-data; name=`"fileType`"$LF",
        "cover",
        "--$boundary",
        "Content-Disposition: form-data; name=`"displayOrder`"$LF",
        "0",
        "--$boundary--$LF"
    ) -join $LF

    $response = Invoke-RestMethod -Uri "$baseUrl/api/File/product/$productId" -Method Post -Headers $headers -ContentType "multipart/form-data; boundary=$boundary" -Body $bodyLines

    Write-Host "`nSuccess! File uploaded:" -ForegroundColor Green
    Write-Host "  File ID: $($response.id)" -ForegroundColor Cyan
    Write-Host "  File Name: $($response.fileName)" -ForegroundColor Cyan
    Write-Host "  File Type: $($response.fileType)" -ForegroundColor Cyan
    Write-Host "  URL: $($response.url)" -ForegroundColor Cyan
    Write-Host "  Size: $([math]::Round($response.fileSize / 1024, 2)) KB" -ForegroundColor Cyan

} catch {
    Write-Host "Error: Failed to upload file" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "`nDone!" -ForegroundColor Green
