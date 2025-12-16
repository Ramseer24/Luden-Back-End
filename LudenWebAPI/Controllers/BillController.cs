using Application.Abstractions.Interfaces.Services;
using Application.DTOs.BillDTOs;
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
        public async Task<ActionResult<IEnumerable<BillDto>>> GetBills()
        {
            try
            {
                var bills = await billService.GetAllBillDtosAsync();
                return Ok(bills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving bills: {ex.Message}");
            }
        }

        // GET: api/Bill/user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<BillDto>>> GetUserBills()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                if (!ulong.TryParse(userIdClaim.Value, out ulong userId))
                {
                    return BadRequest("Invalid user ID format");
                }

                var bills = await billService.GetBillDtosByUserIdAsync(userId);
                return Ok(bills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving user bills: {ex.Message}");
            }
        }

        // GET: api/Bill/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BillDto>> GetBill(ulong id)
        {
            try
            {
                var bill = await billService.GetBillDtoByIdAsync(id);
                return Ok(bill);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Bill with ID {id} not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the bill: {ex.Message}");
            }
        }

        // POST: api/Bill
        [HttpPost]
        public async Task<ActionResult<BillDto>> PostBill([FromBody] BillCreateDto billDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var items = billDto.Items?.Select(i => new Application.DTOs.BillDTOs.BillItemCreateDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();

                Bill bill = await billService.CreateBillAsync(
                    billDto.UserId,
                    billDto.TotalAmount,
                    billDto.Status,
                    billDto.Currency ?? "UAH",
                    billDto.BonusPointsUsed,
                    items
                );
                var billDtoResult = await billService.GetBillDtoByIdAsync(bill.Id);
                return CreatedAtAction(nameof(GetBill), new { id = bill.Id }, billDtoResult);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the bill: {ex.Message}");
            }
        }

        // PUT: api/Bill/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBill(ulong id, [FromBody] Bill bill)
        {
            if (id != bill.Id)
            {
                return BadRequest("Bill ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingBill = await billService.GetByIdAsync(id);
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
        public async Task<IActionResult> DeleteBill(ulong id)
        {
            try
            {
                var bill = await billService.GetByIdAsync(id);
                if (bill == null)
                {
                    return NotFound();
                }

                await billService.DeleteAsync(id);
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
        public ulong UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public BillStatus Status { get; set; }
        public string? Currency { get; set; }
        public int BonusPointsUsed { get; set; }
        public List<BillItemCreateDto>? Items { get; set; }
    }

    public class BillItemCreateDto
    {
        public ulong ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

