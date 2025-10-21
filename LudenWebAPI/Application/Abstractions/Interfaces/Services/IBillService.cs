using Entities.Enums;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IBillService : IGenericService<Bill>
    {
        Task<Bill> CreateBillAsync(int userId, decimal totalAmount, BillStatus status);
    }
}
