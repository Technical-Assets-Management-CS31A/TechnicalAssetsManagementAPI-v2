using System.Linq.Expressions;
using BackendTechnicalEquipmentBorrowingSystem.Data;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BackendTechnicalEquipmentBorrowingSystem.Repository;

// Generic EF Core repository. Specialized repos inherit this and add entity-specific queries.
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public Task<T?> GetByIdAsync(int id) => Set.FindAsync(id).AsTask();
    public Task<List<T>> GetAllAsync() => Set.ToListAsync();
    public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) => Set.Where(predicate).ToListAsync();
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => Set.FirstOrDefaultAsync(predicate);
    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => Set.AnyAsync(predicate);
    public IQueryable<T> Query() => Set.AsQueryable();
    public async Task AddAsync(T entity) => await Set.AddAsync(entity);
    public void Update(T entity) => Set.Update(entity);
    public void Remove(T entity) => Set.Remove(entity);
    public Task<int> SaveChangesAsync() => Db.SaveChangesAsync();
}
