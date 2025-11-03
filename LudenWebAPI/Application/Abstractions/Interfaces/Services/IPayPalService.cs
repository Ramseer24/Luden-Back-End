using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IPayPalService : IGenericService<PaymentOrder>
    {
        Task<PaymentOrder> CapturePaymentAsync(string orderId, ulong userId);
    }

}
