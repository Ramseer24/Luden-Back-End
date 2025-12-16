using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LudenWebAPI.Models.Requests
{
    public class UpdatePaymentRequest
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty; // pm_card_visa, pm_googlepay и т.д.
        public string Action { get; set; } = "confirm"; // confirm | cancel
    }
}
