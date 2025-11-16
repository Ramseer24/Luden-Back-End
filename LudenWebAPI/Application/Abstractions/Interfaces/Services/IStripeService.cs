using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;
using System.Threading.Tasks;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IStripeService
    {
        Task<string> CreatePaymentIntentAsync(ulong userId, ulong billId);
        Task<PaymentOrder> CapturePaymentAsync(string paymentIntentId, ulong userId);
        Task<PaymentOrder?> UpdatePaymentStatusAsync(string paymentIntentId, string paymentMethod, string action);

    }
}

