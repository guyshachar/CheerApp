using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorePush.Interfaces
{
    public interface IRepository<T>
    {
        string CollectionName { get; }

        T Get(string id);
        Task<T> GetAsync(string id);

        T[] Get(string[] ids);
        Task<T[]> GetAsync(string[] ids);

        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();

        void AddUpdate(string id, T obj);
        Task AddUpdateAsync(string id, T obj);
    
        void AddUpdate(object docRef, T obj);
        Task AddUpdateAsync(object docRef, T obj);
    }
}