// LudenWebAPI/Controllers/PaymentController.cs
using Application.Abstractions.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LudenWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(IStripeService stripeService) : ControllerBase
    {
        [Authorize]
        [HttpPost("stripe/create")]
        public async Task<IActionResult> CreateStripePayment([FromBody] int billId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("Id")!.Value);
                var paymentIntentId = await stripeService.CreatePaymentIntentAsync(userId, billId);
                return Ok(new { success = true, paymentIntentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("stripe/complete")]
        public async Task<IActionResult> CompleteStripePayment([FromBody] string paymentIntentId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("Id")!.Value);
                var paymentOrder = await stripeService.CapturePaymentAsync(paymentIntentId, userId);
                return Ok(new { success = true, paymentId = paymentOrder.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}