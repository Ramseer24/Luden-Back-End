using Entities.Enums;

namespace Application.DTOs.BillDTOs
{
    public class CreateBillDto
    {
        public ulong UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public BillStatus Status { get; set; }
        public string? Currency { get; set; }
        public int BonusPointsUsed { get; set; }
        public List<BillItemCreateDto>? Items { get; set; }
    }

    public class BillItemCreateDto
    {
        public ulong ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
