-- Сначала найдем ID продукта Cyberpunk
SELECT Id, Name FROM Products WHERE Name LIKE '%Cyberpunk%';

-- Добавим файл обложки для Cyberpunk (замените {PRODUCT_ID} на реальный ID из предыдущего запроса)
-- INSERT INTO Files (Path, FileName, MimeType, FileSize, CreatedAt, FileCategory, ProductId, FileType, DisplayOrder)
-- VALUES ('products\cyberpunk-cover.jpg', 'cyberpunk-cover.jpg', 'image/jpeg', 733184, datetime('now'), 'Product', {PRODUCT_ID}, 'cover', 0);
