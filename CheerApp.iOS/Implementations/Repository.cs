using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheerApp.iOS.Extensions;
using CorePush.Interfaces;
using Firebase.CloudFirestore;

namespace CheerApp.iOS.Implementations
{
    public abstract class Repository<T> : IRepository<T> where T : class, IModel<T>
    {
        public string CollectionName { get; }

        public Repository(string collectionName)
        {
            this.CollectionName = collectionName;
        }

        public IEnumerable<T> GetAll()
        {
            return this.GetAllAsync().Result;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await FirestoreService.Instance
                                   .GetCollection(CollectionName)
                                   .GetCollectionAsync<T>();
        }

        public T Get(string id)
        {
            return this.GetAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<T> GetAsync(string id)
        {
            var obj = Activator.CreateInstance<T>();
            var docRef = FirestoreService.Instance
                                   .GetCollection(CollectionName)
                                   .GetDocument(id);
            var found = await obj.Doc2ObjAsync(docRef);

            System.Diagnostics.Debug.WriteLine($"{nameof(T)} {nameof(GetAsync)} id={id} found={found} {obj}");

            return found ? obj : null;
        }

        public T[] Get(string[] ids)
        {
            return GetAsync(ids).Result;
        }

        public async Task<T[]> GetAsync(string[] ids)
        {
            var collectionRef = FirestoreService.Instance
                                   .GetCollection(CollectionName);

            DocumentReference[] documentRefs = null;

            collectionRef.AddSnapshotListener((snapshot, error) =>
                    {
                        documentRefs = snapshot.Documents.Where(doc => ids.Contains(doc.Reference.Id)).Select(doc => doc.Reference).ToArray();
                    });
            var documents = await documentRefs.GetDocumentAsync<T>();

            return documents;
        }

        public void AddUpdate(string id, T obj)
        {
            AddUpdateAsync(id, obj).GetAwaiter();
        }

        public async Task AddUpdateAsync(string id, T obj)
        {
            var docRef = FirestoreService.Instance
                .GetCollection(CollectionName)
                .GetDocument(id);
            await AddUpdateAsync(docRef, obj);
        }
       
        public void AddUpdate(object docRef, T obj)
        {
            AddUpdateAsync(docRef, obj).GetAwaiter();
        }

        public async Task AddUpdateAsync(object docRef, T obj)
        {
            await obj.Obj2DocAsync((DocumentReference)docRef);
       }
    }
}