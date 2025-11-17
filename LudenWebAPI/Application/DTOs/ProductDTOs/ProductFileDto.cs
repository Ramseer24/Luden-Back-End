namespace Application.DTOs.ProductDTOs
{
    public class ProductFileDto
    {
        public ulong Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public ulong? UserId { get; set; }
        public ulong? ProductId { get; set; }
        public string? Url { get; set; }
    }
}
