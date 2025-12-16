// Application/Services/StripeService.cs (исправленная версия)
using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Config;
using Entities.Enums;
using Entities.Models;
using Stripe;

namespace Application.Services
{
    public class StripeService(
        IPaymentRepository repository,
        IUserService userService,
        IBillService billService,
        IGenericRepository<Entities.Models.Product> productRepository,
        IGenericRepository<Entities.Models.License> licenseRepository,
        IGenericRepository<BillItem> billItemRepository,
        Config config)
        : GenericService<PaymentOrder>(repository), IStripeService
    {
        private readonly PaymentIntentService _paymentIntentService = new();

        private static readonly Dictionary<string, decimal> ExchangeRates = new()
        {
            { "UAH", 1m },
            { "USD", 0.024m },
            { "PLN", 0.095m },
            { "EUR", 0.022m },
            { "BGN", 0.043m },
            { "CZK", 0.55m },
            { "RON", 0.11m }
        };

        public async Task<string> CreatePaymentIntentAsync(ulong userId, ulong billId)
        {
            StripeConfiguration.ApiKey = config.StripeOptions.Secret;
            var user = await userService.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var bill = await billService.GetByIdAsync(billId);
            if (bill == null)
            {
                throw new Exception($"Bill not found. ID: {billId}");
            }
            if (bill.UserId != userId)
            {
                throw new Exception($"Bill does not belong to the user. Bill UserID: {bill.UserId}, Request UserID: {userId}");
            }

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
            return paymentIntent.ClientSecret;
        }
        public async Task<PaymentOrder?> UpdatePaymentStatusAsync(string paymentIntentId, string paymentMethod, string action)
        {
            StripeConfiguration.ApiKey = config.StripeOptions.Secret;

            var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId);
            if (paymentIntent == null)
                throw new Exception("Payment intent not found.");

            // 1️⃣ Привязка метода оплаты (если указан)
            if (!string.IsNullOrWhiteSpace(paymentMethod))
            {
                var updateOptions = new PaymentIntentUpdateOptions
                {
                    PaymentMethod = paymentMethod
                };
                paymentIntent = await _paymentIntentService.UpdateAsync(paymentIntentId, updateOptions);
            }

            // 2️⃣ Выполнение действия (confirm/cancel)
            if (action?.ToLower() == "confirm")
            {
                var confirmOptions = new PaymentIntentConfirmOptions();
                paymentIntent = await _paymentIntentService.ConfirmAsync(paymentIntentId, confirmOptions);
            }
            else if (action?.ToLower() == "cancel")
            {
                paymentIntent = await _paymentIntentService.CancelAsync(paymentIntentId);
            }

            // 3️⃣ Обновляем данные в локальной БД, если нужно
            if (paymentIntent.Status == "succeeded")
            {
                var chargeService = new ChargeService();
                var charges = chargeService.List(new ChargeListOptions { PaymentIntent = paymentIntent.Id });
                var charge = charges.FirstOrDefault();

                if (charge != null && !await repository.ExistsByTransactionIdAsync(charge.Id))
                {
                    var paymentOrder = new PaymentOrder
                    {
                        ProviderTransactionId = charge.Id,
                        Provider = "Stripe",
                        Success = true,
                        AmountInMinorUnits = charge.Amount,
                        Currency = charge.Currency.ToUpper(),
                        CreatedAt = charge.Created.ToUniversalTime(),
                        DeliveredAt = DateTime.UtcNow
                    };

                    await repository.AddAsync(paymentOrder);
                    return paymentOrder;
                }
            }

            return null;
        }

        public async Task<PaymentOrder> CapturePaymentAsync(string paymentIntentId, ulong userId)
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


            var user = await userService.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            if (await repository.ExistsByTransactionIdAsync(transactionId))
                throw new Exception("Payment already processed.");

            // Получаем Bill ID из metadata
            if (paymentIntent.Metadata != null && paymentIntent.Metadata.TryGetValue("bill_id", out var billIdStr))
            {
                if (ulong.TryParse(billIdStr, out var billId))
                {
                    var bill = await billService.GetByIdAsync(billId);
                    if (bill != null && bill.Status == BillStatus.Pending)
                    {
                        bill.Status = BillStatus.Paid;
                        bill.UpdatedAt = DateTime.UtcNow;
                        await billService.UpdateAsync(bill);

                        var allBillItems = await billItemRepository.GetAllAsync();
                        var billItems = allBillItems.Where(bi => bi.BillId == billId).ToList();

                        // Load all licenses to check for duplicates (idempotency)
                        var allLicenses = await licenseRepository.GetAllAsync();

                        foreach (var item in billItems)
                        {
                            var product = await productRepository.GetByIdAsync(item.ProductId);
                            if (product != null)
                            {
                                // Check how many licenses already exist for this bill item
                                int existingLicensesCount = allLicenses.Count(l => l.BillItemId == item.Id);
                                int licensesToCreate = item.Quantity - existingLicensesCount;

                                if (licensesToCreate > 0)
                                {
                                    product.SalesCount += licensesToCreate;
                                    await productRepository.UpdateAsync(product);

                                    for (int i = 0; i < licensesToCreate; i++)
                                    {
                                        var license = new License
                                        {
                                            ProductId = product.Id,
                                            BillItemId = item.Id,
                                            LicenseKey = $"{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                                            Status = "Active",
                                            CreatedAt = DateTime.UtcNow,
                                            UpdatedAt = DateTime.UtcNow
                                        };
                                        await licenseRepository.AddAsync(license);
                                    }
                                }
                            }
                        }

                        // Начисляем бонусные баллы пользователю (10% от суммы покупки в UAH)
                        decimal amountInUah = bill.TotalAmount;
                        if (!string.IsNullOrEmpty(bill.Currency) && ExchangeRates.TryGetValue(bill.Currency.ToUpper(), out var rate) && rate != 0)
                        {
                            amountInUah = bill.TotalAmount / rate;
                        }

                        var bonusPoints = (int)Math.Floor(amountInUah * 0.1m);
                        user.BonusPoints += bonusPoints;
                        await userService.UpdateAsync(user);
                    }
                }
            }

            var paymentOrder = new PaymentOrder
            {
                ProviderTransactionId = transactionId,
                Provider = "Stripe",
                Success = true,
                AmountInMinorUnits = charge.Amount,
                Currency = currency,
                CreatedAt = createTime,
                TokensAmount = 0,
                DeliveredAt = DateTime.UtcNow,
                UserId = user.Id,
                User = user
            };

            await repository.AddAsync(paymentOrder);
            return paymentOrder;
        }
    }
}