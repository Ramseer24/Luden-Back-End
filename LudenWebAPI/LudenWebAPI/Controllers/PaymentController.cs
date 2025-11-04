using Application.Abstractions.Interfaces.Services;
using Infrastructure.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LudenWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(IPayPalService payPalService) : ControllerBase
    {
        [Authorize]
        [HttpPost("paypal/create")]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] int billId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("Id")!.Value);
                var orderId = await payPalService.CreatePayPalOrderAsync(userId.ToUlong(), billId.ToUlong());
                return Ok(new { success = true, orderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("paypal/complete")]
        public async Task<IActionResult> CompletePayPalPayment([FromBody] string payPalOrderId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("Id")!.Value);
                var paymentOrder = await payPalService.CapturePaymentAsync(payPalOrderId, userId.ToUlong());
                return Ok(new { success = true, paymentId = paymentOrder.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}