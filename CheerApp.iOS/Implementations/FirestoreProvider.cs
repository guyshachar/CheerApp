using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;
using Firebase.CloudFirestore;
using Foundation;
using UIKit;

namespace CheerApp.iOS.Implementations
{
    public class FirestoreProvider : IFirestoreProvider
    {
        private Firebase.CloudFirestore.Firestore _instance => Firebase.CloudFirestore.Firestore.SharedInstance;

        private static Dictionary<Type, Func<object, object>> obj2DocTypeMapper = new Dictionary<Type, Func<object, dynamic>>
        {
            // array
            { typeof(bool), (obj) => ((Foundation.NSNumber)obj).BoolValue },
            // byte
            { typeof(DateTime), (obj) => ((Foundation.NSDate)obj) },
            // double
            // pos
            // int
            // KVP
            // null
            // reference
            { typeof(string), obj => obj?.ToString() },
//            { typeof(string[]), obj => ((NSArray)obj).Select(x => x?.ToString()).ToArray() },
//            { typeof(List<string>), obj => ((NSArray)obj).Select(x => x?.ToString()).ToList() }
            { typeof(string[]), obj => NSArray.FromStrings(((string[])obj).Select(x => x).ToArray()) },
            { typeof(List<string>), obj => NSArray.FromStrings(((List<string>)obj).Select(x => x).ToList()) }
        };

        private static Dictionary<Type, Func<object, object>> _doc2ObjTypeMapper = new Dictionary<Type, Func<object, dynamic>>
        {
            // array
            { typeof(bool), (obj) => ((Foundation.NSNumber)obj).BoolValue },
            // byte
            { typeof(DateTime), (obj) => ((Foundation.NSDate)obj) },
            // double
            // pos
            // int
            // KVP
            // null
            // reference
            { typeof(string), obj => obj?.ToString() },
            { typeof(string[]), obj => ((NSArray)obj).Select(x => x?.ToString()).ToArray() },
            { typeof(List<string>), obj => ((NSArray)obj).Select(x => x?.ToString()).ToList() }
        };

        public FirestoreProvider()
        {
            try
            {
                Firebase.Core.App.Configure();
            }
            catch (Exception ex)
            { }
        }

        public T Get<T>(string id) where T : ModelBase, new()
        {
            return this.GetAsync<T>(id).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<T> GetAsync<T>(string id) where T : ModelBase, new()
        {
            var docSnapshot = await _instance
                                   .GetCollection(GetCollectionName<T>())
                                   .GetDocument(id)
                                   .GetDocumentAsync();
            if (docSnapshot.Exists)
                return Data2Obj<T>(docSnapshot.Data);

            System.Diagnostics.Debug.WriteLine($"{nameof(T)} {nameof(GetAsync)} id={id} found={false}");
            return (T)default;
        }

        public IEnumerable<T> GetAll<T>() where T : ModelBase, new()
        {
            return this.GetAllAsync<T>().Result;
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : ModelBase, new()
        {
            var querySnapshot = await _instance
                         .GetCollection(GetCollectionName<T>())
                         .GetDocumentsAsync();
            return querySnapshot.Documents.Select(doc => Data2Obj<T>(doc.Data));
        }

        public IEnumerable<T> Get<T>(string[] ids) where T : ModelBase, new()
        {
            return GetAsync<T>(ids).Result;
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string[] ids) where T : ModelBase, new()
        {
            var query = _instance
                    .GetCollection(GetCollectionName<T>())
                    .WhereFieldIn(nameof(DeviceDetail.Id), ids);

            var querySnapshot = await query.GetDocumentsAsync();

            return querySnapshot.Documents.Select(doc => Data2Obj<T>(doc.Data));
        }

        public void AddUpdate<T>(T obj) where T : ModelBase
        {
            AddUpdateAsync<T>(obj).GetAwaiter();
        }

        public async Task AddUpdateAsync<T>(T obj) where T : ModelBase
        {
            var docRef = _instance
                .GetCollection(GetCollectionName<T>())
                .GetDocument(obj.Id);
            await AddUpdateAsync<T>(docRef, obj);
        }

        public void AddUpdate<T>(object docRef, T obj) where T : ModelBase
        {
            AddUpdateAsync<T>((DocumentReference)docRef, obj).GetAwaiter();
        }

        public async Task AddUpdateAsync<T>(object docRef, T obj) where T : ModelBase
        {
            try
            {
                obj.UpdateDateTime = DateTime.UtcNow.ToString("u");
                var data = Obj2Data(obj);
                await ((DocumentReference)docRef).SetDataAsync(data);
            }
            catch (Exception ex)
            {

            }
        }

        public void RegisterListener<T>(string id, Action<T> listenAction) where T : ModelBase, new()
        {
            var docRef = _instance.GetCollection(GetCollectionName<T>()).GetDocument(id);
            var docSnapshotHandler = new DocumentSnapshotHandler((docSnapshot, nsError) =>
            {
                Console.WriteLine("Callback received document snapshot.");
                Console.WriteLine("Document exists? {0}", docSnapshot.Exists);
                if (docSnapshot.Exists)
                {
                    docSnapshot.DidChangeValue("");
                    Console.WriteLine("Document data for {0} document:", docSnapshot.Id);
                    var obj = Data2Obj<T>(docSnapshot.Data);
                    if (listenAction != null)
                        listenAction(obj);
                };
            });
            var listener = docRef.AddSnapshotListener(docSnapshotHandler);
        }

        #region Private
        private string GetCollectionName<T>()
        {
            var collectionName = $"CheerApp{typeof(T).Name}s";
            return collectionName;
        }

        private static Dictionary<object, object> Obj2Data<T>(T model) where T : ModelBase
        {
            var data = model.Properties.ToDictionary(prop => (object)prop.Name, prop => obj2DocTypeMapper[prop.PropertyType](prop.GetValue(model, null)));
            return data;
        }

        private static T Data2Obj<T>(NSDictionary<NSString, NSObject> data) where T : ModelBase, new()
        {
            if (data == null)
                return (T)default;

            var obj = new T();
            foreach (var prop in obj.Properties)
            {
                if (data.ContainsKey((NSString)prop.Name))
                {
                    try
                    {
                        var field = data[prop.Name];
                        var value = _doc2ObjTypeMapper[prop.PropertyType](data[prop.Name]);
                        prop.SetValue(obj, value);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return obj;
        }
        #endregion Private
    }
}