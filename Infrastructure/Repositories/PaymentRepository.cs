using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : GenericRepository<PaymentOrder>, IPaymentRepository
    {
        public PaymentRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
        }

        public async Task<bool> ExistsByTransactionIdAsync(string transactionId)
        {
            var payments = await GetAllAsync();
            return payments.Any(p => p.ProviderTransactionId == transactionId);
        }
    }
}