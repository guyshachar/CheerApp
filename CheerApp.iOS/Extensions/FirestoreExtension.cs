using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.CloudFirestore;

namespace CheerApp.iOS.Extensions
{
    public static class FirestoreExtension
    {
        // ToDo: Support more types...
        // https://firebase.google.com/docs/firestore/manage-data/data-types
        // ToDo: Add interface to add custom types
        private static Dictionary<Type, Func<Foundation.NSObject, System.Object>> TypeMapper = new Dictionary<Type, Func<Foundation.NSObject, dynamic>>
        {
            // array
            { typeof(bool), (obj) => ((Foundation.NSNumber)obj).BoolValue },
            // byte
            // Date?
            // double
            // pos
            // int
            // KVP
            // null
            // reference
            { typeof(string), (obj) => obj.ToString() },
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
                    prop.SetValue(instance, TypeMapper[propType](baseData[key]));
                }
                catch (Exception e)
                {

                }
            }

            return instance;
        }

        public static Task<T> GetDocumentAsync<T>(this DocumentReference reference) where T : class
        {
            try
            {
                var tcs = new TaskCompletionSource<T>();

                reference
                    .AddSnapshotListener((snapshot, error) =>
                {
                    if (error != null)
                    {
                        tcs.SetException(new Exception(error.LocalizedDescription));
                        return;
                    }

                    tcs.SetResult(Cast<T>(snapshot));
                });

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

        // ToDo: Add QueryReference
    }
}