// Application/Services/StripeService.cs (исправленная версия)
using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Config;
using Entities.Enums;
using Entities.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StripeService(
        IPaymentRepository repository,
        IUserService userService,
        IBillService billService,
        Config config)
        : GenericService<PaymentOrder>(repository), IStripeService
    {
        private readonly PaymentIntentService _paymentIntentService = new();

        public async Task<string> CreatePaymentIntentAsync(int userId, int billId)
        {
            StripeConfiguration.ApiKey = config.StripeOptions.Secret;
            var user = await userService.GetByIdAsync((ulong)userId);
            if (user == null)
                throw new Exception("User not found.");

            var bill = await billService.GetByIdAsync((ulong)billId);
            if (bill == null || bill.User.Id != (ulong)userId)
                throw new Exception("Bill not found or does not belong to the user.");

            if (bill.Status != BillStatus.Pending)
                throw new Exception("Bill is not in a payable state.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(bill.TotalAmount * 100),
                Currency = bill.Currency?.ToLower() ?? "usd",
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId.ToString() },
                    { "bill_id", billId.ToString() }
                }
            };

            var paymentIntent = await _paymentIntentService.CreateAsync(options);
            return paymentIntent.Id;
        }

        public async Task<PaymentOrder> CapturePaymentAsync(string paymentIntentId, int userId)
        {
            StripeConfiguration.ApiKey = config.StripeOptions.Secret;
            var paymentIntent = await _paymentIntentService.GetAsync(
                paymentIntentId,
                new PaymentIntentGetOptions { Expand = new List<string> { "charges" } });

            if (paymentIntent == null)
                throw new Exception("Payment intent not found.");

            if (paymentIntent.Status != "succeeded")
                throw new Exception($"Payment not succeeded. Status: {paymentIntent.Status}");

            var chargeService = new ChargeService();
            var charges = chargeService.List(new ChargeListOptions
            {
                PaymentIntent = paymentIntent.Id
            });
            var charge = charges.FirstOrDefault();
            if (charge == null)
                throw new Exception("No charge information found.");

            var transactionId = charge.Id;
            var amount = charge.Amount / 100m;
            var currency = charge.Currency.ToUpper();
            var createTime = charge.Created.ToUniversalTime();


            var user = await userService.GetByIdAsync((ulong)userId);
            if (user == null)
                throw new Exception("User not found.");

            if (await repository.ExistsByTransactionIdAsync(transactionId))
                throw new Exception("Payment already processed.");

            var paymentOrder = new PaymentOrder
            {
                ProviderTransactionId = transactionId,
                Provider = "Stripe",
                Success = true,
                AmountInMinorUnits = charge.Amount,
                Currency = currency,
                CreatedAt = createTime,
                TokensAmount = 0,
                DeliveredAt = DateTime.MinValue,
                UserId = user.Id,
                User = user
            };

            await repository.AddAsync(paymentOrder);
            return paymentOrder;
        }
    }
}