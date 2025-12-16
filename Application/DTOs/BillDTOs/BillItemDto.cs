using Application.DTOs.ProductDTOs;

namespace Application.DTOs.BillDTOs
{
    public class BillItemDto
    {
        public ulong Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public ProductDto Product { get; set; } 
        public List<string> LicenseKeys { get; set; } = new List<string>();
    }
}
