using Application.Abstractions.Interfaces.Services;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LudenWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillController(IBillService billService) : ControllerBase
    {
        // GET: api/Bill
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bill>>> GetBills()
        {
            var bills = await billService.GetAllAsync();
            return Ok(bills);
        }

        // GET: api/Bill/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bill>> GetBill(int id)
        {
            try
            {
                var bill = await billService.GetByIdAsync(id);
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
                var bill = await billService.CreateBillAsync(
                    billDto.UserId,
                    billDto.TotalAmount,
                    billDto.Status
                );
                return CreatedAtAction(nameof(GetBill), new { id = bill.Id }, bill);
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
        public async Task<IActionResult> DeleteBill(int id)
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
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "pending";
    }
}
