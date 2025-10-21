using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.UserDTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public List<BillDto> Bills { get; set; } = new List<BillDto>();
    }
    public class BillDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public BillStatus Status { get; set; }
    }
}
