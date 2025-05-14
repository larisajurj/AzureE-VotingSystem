namespace PollingStationAPI.Data.Repository.Abstractions;

public interface IRepository<T, U>
    where T : class
{
    Task Add(T entity);
    Task<T?> GetById(U entityId);
    Task<T?> Update(T entity);
    Task<bool> Delete(U entityId);
}
