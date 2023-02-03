using System;
using CheerApp.iOS.Models;
using Firebase.CloudFirestore;
using System.Threading.Tasks;

namespace CheerApp.iOS
{
    public interface IModel<T> where T : class
    {
        Task<bool> Doc2ObjAsync(DocumentReference docRef);
        Task Obj2DocAsync(DocumentReference docRef);
    }
}