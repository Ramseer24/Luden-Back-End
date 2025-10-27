using Application.DTOs.ProductDTOs;

namespace Application.DTOs.BillDTOs
{
    public class BillItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public ProductDto Product { get; set; } 
    }
}
