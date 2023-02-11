using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheerApp.Common.Models;

namespace CheerApp.Common.Interfaces
{
    public interface IFirestoreProvider
    {
        T Get<T>(string id) where T : ModelBase, new();
        Task<T> GetAsync<T>(string id) where T : ModelBase, new();

        IEnumerable<T> Get<T>(string[] ids) where T : ModelBase, new();
        Task<IEnumerable<T>> GetAsync<T>(string[] ids) where T : ModelBase, new();

        IEnumerable<T> GetAll<T>() where T : ModelBase, new();
        Task<IEnumerable<T>> GetAllAsync<T>() where T : ModelBase, new();

        void AddUpdate<T>(T obj) where T : ModelBase;
        Task AddUpdateAsync<T>(T obj) where T : ModelBase;

        void AddUpdate<T>(object docRef, T obj) where T : ModelBase;
        Task AddUpdateAsync<T>(object docRef, T obj) where T : ModelBase;

        void RegisterListener<T>(string id, Action<T> listenAction) where T : ModelBase, new();
    }
}