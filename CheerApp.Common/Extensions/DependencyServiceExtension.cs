using System;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;

namespace CheerApp.iOS
{
	public static class DependencyServiceExtension
	{
		private static readonly Dictionary<Type, UIViewController> screensDic = new Dictionary<Type, UIViewController>();

		static DependencyServiceExtension()
		{
            Xamarin.Forms.Forms.Init();
        }

        public static void Check()
        {
        }

        public static void Register<T>() where T : UIViewController
		{
			DependencyService.Register<T>();
            if (!screensDic.ContainsKey(typeof(T)))
                screensDic.Add(typeof(T), DependencyService.Get<T>());
        }

        public static UIViewController Get<T>() where T : UIViewController
		{
			return screensDic[typeof(T)];
		}

        public static UIViewController Get(Type t)
        {
            if (screensDic.ContainsKey(t))
                return screensDic[t];
			return null;
        }
    }
}