using developers.Models;

namespace developers.Data{
    public interface IDataRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> Save();
        Task<Developer> GetDeveloperByUserIdAsync(int userId);
        Task<T> GetByEmailAsync(string email);
        DatabaseContext GetContext(); 
        Task<T> GetByIdsAsync(params object[] keyValues);



}

    }


