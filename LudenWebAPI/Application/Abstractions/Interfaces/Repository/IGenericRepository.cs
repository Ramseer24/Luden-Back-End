using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IGenericRepository<T> where T : IEntity
    {
        Task AddAsync(T entity);                    // ✅ Реализовано
        Task RemoveAsync(T entity);                 // ✅ Реализовано  
        Task RemoveByIdAsync(int id);               // ✅ Реализовано
        Task UpdateAsync(T entity);                 // ✅ Реализовано
        Task<T> GetByIdAsync(int id);               // ✅ Реализовано
        Task<IEnumerable<T>> GetAllAsync();         // ✅ Реализовано
        Task SaveChangesAsync();
    }
}
