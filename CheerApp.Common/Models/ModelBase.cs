using System;
using CheerApp.Common.Interfaces;
using Google.Cloud.Firestore;
using System.Linq;
using System.Reflection;

namespace CheerApp.Common.Models
{
	public abstract class ModelBase
	{
		public ModelBase()
		{
		}

		[FirestoreProperty]
		public string Id { get; set; }
		[FirestoreProperty]
		public string UpdateDateTime { get; set; }

		public abstract PropertyInfo[] Properties { get; }

        protected static PropertyInfo[] GetProperties<T>()
		{
			return typeof(T).GetProperties()
				.Where(prop => Attribute.IsDefined(prop, typeof(Google.Cloud.Firestore.FirestorePropertyAttribute))).ToArray();
		}
	}
}