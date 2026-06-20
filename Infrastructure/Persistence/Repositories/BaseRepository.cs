using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Persistence.Repositories
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(int id) =>
            await _dbSet.FindAsync(id);

        public async Task<IEnumerable<TEntity>> GetAllAsync() => 
            await _dbSet
                .AsNoTracking()
                .ToListAsync();

        public async Task AddAsync(TEntity entity) => 
            await _dbSet.AddAsync(entity);

        public void Update(TEntity entity) =>
            _dbSet.Update(entity);

        public void Delete(TEntity entity) =>
            _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<TEntity> entity) =>
            _dbSet.RemoveRange(entity);
    }
}
