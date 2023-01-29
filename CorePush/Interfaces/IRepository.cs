using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorePush
{
    public interface IRepository<T>
    {
        T GetOne(string collectionName, string id);
        Task<T> GetOneAsync(string collectionName, string id);

        IEnumerable<T> GetAll(string collectionName);
        Task<IEnumerable<T>> GetAllAsync(string collectionName);

        void AddUpdateOne(string collectionName, string id, Dictionary<object, object> document);
        Task AddUpdateOneAsync(string collectionName, string id, Dictionary<object, object> document);

        void AddUpdateOne(object docRef, Dictionary<object, object> document);
        Task AddUpdateOneAsync(object docRef, Dictionary<object, object> document);
    }
}