using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IGenericService<T> where T : IEntity
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task UpdateAsync(T user);
        Task DeleteAsync(int id);
    }
}
