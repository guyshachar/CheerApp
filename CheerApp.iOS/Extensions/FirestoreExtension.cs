using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Firebase.CloudFirestore;
using Foundation;

namespace CheerApp.iOS.Extensions
{
    public static class FirestoreExtension
    {
        // ToDo: Support more types...
        // https://firebase.google.com/docs/firestore/manage-data/data-types
        // ToDo: Add interface to add custom types
        private static Dictionary<Type, Func<object, object>> Doc2ObjTypeMapper = new Dictionary<Type, Func<object, dynamic>>
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
            { typeof(string), obj => obj.ToString() },
            { typeof(string[]), obj => ((NSArray)obj).Select(x => x.ToString()).ToArray() },
            { typeof(List<string>), obj => ((NSArray)obj).Select(x => x.ToString()).ToList() }
        };

        private static Dictionary<Type, Func<object, object>> Obj2DocTypeMapper1 = new Dictionary<Type, Func<object, dynamic>>
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
            { typeof(string), obj => obj.ToString() },
            { typeof(string[]), obj => ((string[])obj).Select(x => (NSObject)(object)x).ToArray() },
            { typeof(List<string>), obj => ((List<string>)obj).Select(x => (NSObject)(object)x).ToList() }
        };

        private static T Cast<T>(DocumentSnapshot doc)
        {
            var instance = Activator.CreateInstance<T>();

            var baseData = doc.Data;

            if (baseData == null)
                return default(T);

            foreach (var key in baseData.Keys)
            {
                try
                {
                    // ToDo: Error handling
                    var prop = typeof(T).GetProperty(key);
                    var propType = prop.PropertyType;
                    prop.SetValue(instance, Doc2ObjTypeMapper[propType](baseData[key]));
                }
                catch (Exception e)
                {
                }
            }

            return instance;
        }

        public static DocumentSnapshot Cast<T>(this DocumentSnapshot doc, Dictionary<NSString, NSObject> data)
        {
            if (data == null)
                return default(DocumentSnapshot);

            foreach (var key in data.Keys)
            {
                try
                {
                    // ToDo: Error handling
                    var prop = typeof(T).GetProperty(key);
                    var propType = prop.PropertyType;
                    prop.SetValue(doc, Doc2ObjTypeMapper[propType](data[key]));
                }
                catch (Exception e)
                {
                }
            }

            return doc;
        }

        public static async Task<NSDictionary<NSString, NSObject>> CastAsync<T>(this DocumentReference docRef, T obj)
        {
            if (obj == null)
                return null;

            try
            {
                var data = obj.GetType()
                 .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                 .ToDictionary(prop => prop.Name, prop => (object)prop.GetValue(obj, null));

                var docSnapshot = await docRef.GetDocumentAsync();

                foreach (var key in data.Keys)
                {
                    try
                    {
                        var prop = typeof(T).GetProperty(key.ToString());
                        var propType = prop.PropertyType;
                        docSnapshot.Data[key] = (NSObject)Obj2DocTypeMapper1[propType](data[key]);
                    }
                    catch (Exception e)
                    {
                    }
                }

                return docSnapshot.Data;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public static async Task<T> GetDocumentAsync1<T>(this DocumentReference docRef) where T : class
        {
            try
            {
                var obj = Activator.CreateInstance<T>();
                var docSnapshot = await docRef.GetDocumentAsync();
                if (docSnapshot.Exists)
                {
                    var data = docSnapshot.Data;
                    foreach (var key in data.Keys)
                    {
                        var prop = typeof(T).GetProperty(key.ToString());
                        var propType = prop.PropertyType;
                        prop.SetValue(obj, Doc2ObjTypeMapper[propType](data[key]));
                    }
                    return obj;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static Task<T[]> GetDocumentAsync<T>(this DocumentReference[] references) where T : class
        {
            try
            {
                var tcs = new TaskCompletionSource<T[]>();

                var documents = new ConcurrentBag<T>();
                Parallel.ForEach(references, reference =>
                {
                    reference.AddSnapshotListener((snapshot, error) =>
                    {
                        if (error != null)
                        {
                            tcs.SetException(new Exception(error.LocalizedDescription));
                            return;
                        }

                        documents.Add(Cast<T>(snapshot));
                    });
                });

                tcs.SetResult(documents.ToArray());

                return tcs.Task;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static Task<IEnumerable<T>> GetCollectionAsync<T>(this CollectionReference reference) where T : class
        {
            try
            {
                var tcs = new TaskCompletionSource<IEnumerable<T>>();

                reference
                    .AddSnapshotListener((snapshot, error) =>
                    {
                        if (error != null)
                        {
                            tcs.SetException(new Exception(error.LocalizedDescription));
                            return;
                        }

                        var data = snapshot.Documents.Select((doc) => Cast<T>(doc)).ToList();
                        tcs.SetResult(data);
                    });

                return tcs.Task;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static NSString[] GetKeys(object obj)
        {
            var keys = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(prop => (NSString)prop.Name)
                .ToArray();
            return keys;
        }

        // ToDo: Add QueryReference
    }
}