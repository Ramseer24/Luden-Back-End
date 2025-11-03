using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PaymentRepository(LudenDbContext dbContext) : GenericRepository<PaymentOrder>(dbContext), IPaymentRepository
    {
        public async Task<bool> ExistsByTransactionIdAsync(string transactionId)
        {
            return await dbContext.PaymentOrders
                .AnyAsync(p => p.ProviderTransactionId == transactionId);
        }
    }

}
