namespace Application.DTOs.BillDTOs
{
    public class BillDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<BillItemDto> BillItems { get; set; }
    }
}
