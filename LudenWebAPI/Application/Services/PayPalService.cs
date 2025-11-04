using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Config;
using Entities.Models;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PayPalService(IPaymentRepository repository, IUserService userService, IBillService billService, Config config)
        : GenericService<PaymentOrder>(repository), IPayPalService
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
            var user = await userService.GetByIdAsync(userId);
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

        public async Task<string> CreatePayPalOrderAsync(ulong userId, ulong billId)
        {
            var user = await userService.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var bill = await billService.GetByIdAsync(billId);
            if (bill == null || bill.User.Id != userId)
                throw new Exception("Bill not found or does not belong to the user.");

            if (bill.Status != Entities.Enums.BillStatus.Pending)
                throw new Exception("Bill is not in a payable state.");

            var request = new OrdersCreateRequest();
            request.Headers.Add("prefer", "return=representation");
            request.RequestBody(BuildOrderRequestBody(bill.TotalAmount, bill.Currency ?? "USD"));

            var response = await _client.Execute(request);
            if (response.StatusCode != HttpStatusCode.Created)
                throw new Exception("Failed to create PayPal order.");

            var order = response.Result<Order>();
            return order.Id;
        }

        private static OrderRequest BuildOrderRequestBody(decimal amount, string currency)
        {
            return new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>()
                {
                    new PurchaseUnitRequest()
                    {
                        AmountWithBreakdown = new AmountWithBreakdown()
                        {
                            CurrencyCode = currency,
                            Value = amount.ToString("0.00")
                        }
                    }
                }
            };
        }
    }
}