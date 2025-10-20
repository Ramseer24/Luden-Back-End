using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BillRepository(LudenDbContext context) : GenericRepository<Bill>(context), IBillRepository
    {
    }
}
