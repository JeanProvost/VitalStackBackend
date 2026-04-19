namespace Backend.Core.Interfaces.IRepository;

public interface IBaseRepository<T> where T : class
{
    Task<T> Create(T model);
    Task<T> Update(T model);
    Task Delete(T model);
}
