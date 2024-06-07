using developers.Models;
using Microsoft.EntityFrameworkCore;

namespace developers.Data{
    public class DataRepository<T> : IDataRepository<T> where T : class
    {
        private readonly DatabaseContext _db;
        private readonly DbSet<T> table;

        public DataRepository(DatabaseContext db)
        {
            _db = db;
            table = _db.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await table.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await table.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await table.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(T entity)
        {
            table.Remove(entity);
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }



    public async Task<Developer> GetDeveloperByUserIdAsync(int userId)
    {
        return await _db.Set<Developer>().FirstOrDefaultAsync(d => d.UserID == userId);
    }


    public async Task<T> GetByEmailAsync(string email)
        {
            if (typeof(T) == typeof(User))
            {
                return await table.Cast<User>().FirstOrDefaultAsync(u => u.Email == email) as T;
            }
            else
            {
                return null;
            }
        }

        public async Task<T> GetByIdsAsync(params object[] keyValues)
        {
            return await table.FindAsync(keyValues);
        }


        


        public DatabaseContext GetContext()
        {
            return _db;
        }










    }
}


