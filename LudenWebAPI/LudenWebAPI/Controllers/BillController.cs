using Application.Abstractions.Interfaces.Services;
using Entities.Enums;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LudenWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BillController(IBillService billService) : ControllerBase
    {
        // GET: api/Bill
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bill>>> GetBills()
        {
            var bills = await billService.GetAllAsync();
            return Ok(bills);
        }

        // GET: api/Bill/user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<Bill>>> GetUserBills()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("Invalid user ID format");
                }

                var bills = await billService.GetBillsByUserIdAsync(userId);
                return Ok(bills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving user bills: {ex.Message}");
            }
        }

        // GET: api/Bill/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bill>> GetBill(int id)
        {
            try
            {
                var bill = await billService.GetByIdAsync((ulong)id);
                if (bill == null)
                {
                    return NotFound();
                }
                return Ok(bill);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the bill.");
            }
        }

        // POST: api/Bill
        [HttpPost]
        public async Task<ActionResult<Bill>> PostBill([FromBody] BillCreateDto billDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Bill bill = await billService.CreateBillAsync(
                    billDto.UserId,
                    billDto.TotalAmount,
                    billDto.Status
                );
                return Ok(bill);//CreatedAtAction(nameof(GetBill), new { id = bill.Id }, bill);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while creating the bill.");
            }
        }

        // PUT: api/Bill/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBill(int id, [FromBody] Bill bill)
        {
            if ((ulong)id != bill.Id)
            {
                return BadRequest("Bill ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingBill = await billService.GetByIdAsync((ulong)id);
                if (existingBill == null)
                {
                    return NotFound();
                }

                await billService.UpdateAsync(bill);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the bill.");
            }
        }

        // DELETE: api/Bill/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBill(int id)
        {
            try
            {
                var bill = await billService.GetByIdAsync((ulong)id);
                if (bill == null)
                {
                    return NotFound();
                }

                await billService.DeleteAsync((ulong)id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the bill.");
            }
        }
    }

    public class BillCreateDto
    {
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public BillStatus Status { get; set; }
    }
}
