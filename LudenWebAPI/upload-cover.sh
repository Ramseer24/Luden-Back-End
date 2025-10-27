#!/bin/bash

# Скрипт для загрузки обложки Cyberpunk через API
BASE_URL="http://localhost:5195"
FILE_PATH="./LudenWebAPI/wwwroot/uploads/products/cyberpunk-cover.jpg"

echo "=== Upload Cyberpunk Cover ==="
echo ""

# Шаг 1: Получить Product ID
read -p "Enter Cyberpunk Product ID: " PRODUCT_ID

if [ -z "$PRODUCT_ID" ]; then
    echo "Error: Product ID is required!"
    exit 1
fi

# Шаг 2: Авторизация
echo ""
read -p "Admin Email: " EMAIL
read -sp "Admin Password: " PASSWORD
echo ""

echo "Logging in..."
TOKEN=$(curl -s -X POST "$BASE_URL/api/Authorization/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}" | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
    echo "Error: Failed to get authorization token"
    exit 1
fi

echo "Token obtained successfully"

# Шаг 3: Загрузить файл
echo ""
echo "Uploading file..."

RESPONSE=$(curl -s -X POST "$BASE_URL/api/File/product/$PRODUCT_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@$FILE_PATH" \
  -F "fileType=cover" \
  -F "displayOrder=0")

echo ""
echo "Response:"
echo "$RESPONSE" | python -m json.tool 2>/dev/null || echo "$RESPONSE"

echo ""
echo "Done!"
