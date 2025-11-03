using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IPaymentRepository : IGenericRepository<PaymentOrder>
    {
        Task<bool> ExistsByTransactionIdAsync(string transactionId);
    }

}
