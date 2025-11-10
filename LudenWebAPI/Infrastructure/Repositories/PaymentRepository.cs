using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Infrastructure.FirebaseDatabase;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : GenericRepository<PaymentOrder>, IPaymentRepository
    {
        private readonly LudenDbContext? _context;
        private readonly bool _useFirebase;

        //Конструктор для SQLite
        public PaymentRepository(LudenDbContext dbContext) : base(null!)
        {
            _context = dbContext;
            _useFirebase = false;
        }

        //Конструктор для Firebase
        public PaymentRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
            _useFirebase = true;
        }

        public async Task<bool> ExistsByTransactionIdAsync(string transactionId)
        {
            if (_useFirebase)
            {
                //Firebase-режим
                var payments = await GetAllAsync();
                return payments.Any(p => p.ProviderTransactionId == transactionId);
            }
            else
            {
                //Старый SQLite-режим
                return await _context!.PaymentOrders
                    .AnyAsync(p => p.ProviderTransactionId == transactionId);
            }
        }
    }
}