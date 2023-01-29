using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheerApp.iOS.Extensions;
using CheerApp.iOS.Services;
using CheerApp.Models;
using CorePush;
using Firebase.CloudFirestore;
using Xamarin.Forms;

[assembly: Dependency(typeof(CheerApp.iOS.PlatformImplementations.EventRepository))]
namespace CheerApp.iOS.PlatformImplementations
{
    public class EventRepository : IRepository<Event>
    {
        public IEnumerable<Event> GetAll(string collectionName)
        {
            return this.GetAllAsync(collectionName).Result;
        }

        public Task<IEnumerable<Event>> GetAllAsync(string collectionName)
        {
            return FirestoreService.Instance
                                   .GetCollection(collectionName)
                                   .GetCollectionAsync<Event>();
        }

        public Event GetOne(string collectionName, string id)
        {
            return this.GetOneAsync(collectionName, id).Result;
        }

        public Task<Event> GetOneAsync(string collectionName, string id)
        {
            return FirestoreService.Instance
                                   .GetCollection(collectionName)
                                   .GetDocument(id)
                                   .GetDocumentAsync<Event>();
        }

        public void AddUpdateOne(string collectionName, string id, Dictionary<object, object> data)
        {
            AddUpdateOneAsync(collectionName, id, data).GetAwaiter();
        }

        public Task AddUpdateOneAsync(string collectionName, string id, Dictionary<object, object> data)
        {
            var docRef = FirestoreService.Instance
                .GetCollection(collectionName)
                .GetDocument(id);
            return AddUpdateOneAsync(docRef, data);
        }

        public void AddUpdateOne(object docRef, Dictionary<object, object> data)
        {
            AddUpdateOneAsync(docRef, data).GetAwaiter();
        }

        public Task AddUpdateOneAsync(object docRef, Dictionary<object, object> data)
        {
            return ((DocumentReference)docRef).SetDataAsync(data);
        }
    }
}