using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Services
{
    using Application.Services.Payment;
    using Entities;
    using Entities.Config;
    using Entities.Models;
    using global::Application.Abstractions.Interfaces.Repository;
    using global::Application.Abstractions.Interfaces.Services;
    using PayPalCheckoutSdk.Core;
    using PayPalCheckoutSdk.Orders;
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    namespace Application.Services.Payment
    {
        public class PayPalService(
            IPaymentRepository repository,
            IUserService userService,
            Config config
        ) : GenericService<PaymentOrder>(repository), IPayPalService
        {
            private readonly PayPalHttpClient _client = config.PayPalOptions.UseSandbox
                ? new PayPalHttpClient(new SandboxEnvironment(config.PayPalOptions.ClientId, config.PayPalOptions.Secret))
                : new PayPalHttpClient(new LiveEnvironment(config.PayPalOptions.ClientId, config.PayPalOptions.Secret));

            public async Task<PaymentOrder> CapturePaymentAsync(string orderId, ulong userId)
            {
                var orderRequest = new OrdersGetRequest(orderId);
                var response = await _client.Execute(orderRequest);

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("Failed to retrieve PayPal order.");

                var order = response.Result<Order>();
                if (order.Status != "COMPLETED")
                    throw new Exception($"PayPal order is not completed. Status: {order.Status}");

                var capture = order.PurchaseUnits.FirstOrDefault()?.Payments?.Captures?.FirstOrDefault();
                if (capture == null)
                    throw new Exception("No capture information found.");

                var transactionId = capture.Id;
                var amount = decimal.Parse(capture.Amount.Value);
                var currency = capture.Amount.CurrencyCode;
                var timeStr = capture.CreateTime ?? order.CreateTime;
                var createTime = DateTime.TryParse(timeStr, out var parsedTime) ? parsedTime : DateTime.UtcNow;

                var user = await userService.GetByIdAsync((int)userId);
                if (user == null)
                    throw new Exception("User not found.");

                if (await repository.ExistsByTransactionIdAsync(transactionId))
                    throw new Exception("Payment already processed.");

                var paymentOrder = new PaymentOrder
                {
                    ProviderTransactionId = transactionId,
                    Provider = "PayPal",
                    Success = true,
                    AmountInMinorUnits = amount * 100,
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
}
