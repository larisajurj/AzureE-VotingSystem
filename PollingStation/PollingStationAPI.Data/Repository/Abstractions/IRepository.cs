using PollingStationAPI.Data.Models;
using System.Linq.Expressions;

namespace PollingStationAPI.Data.Repository.Abstractions;

public interface IRepository<TEntity, TId>
    where TEntity : class
{
    Task Add(TEntity entity);
    Task<TEntity?> GetById(TId entityId);
    Task<TEntity?> Update(TEntity entity);
    Task<bool> Delete(TId entityId);
    Task<IEnumerable<TEntity>> Filter(Expression<Func<TEntity, bool>> predicate);
}
