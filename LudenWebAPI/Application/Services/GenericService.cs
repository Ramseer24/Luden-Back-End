using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Models;

namespace Application.Services;

public class GenericService<T>(IGenericRepository<T> repository) : IGenericService<T> where T : IEntity
{
    public async Task CreateAsync(T entity)
    {
        await repository.AddAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await repository.RemoveAsync(await GetByIdAsync(id));
    }

    public async Task DeleteAsync(T entity)
    {
        await repository.RemoveAsync(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task UpdateAsync(T entity)
    {
        await repository.UpdateAsync(entity);
    }
}
