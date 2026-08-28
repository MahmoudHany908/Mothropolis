using System;
using System.Collections.Generic;

namespace Mothropolis.Core
{
    public static class GameServices
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new Exception($"Service {typeof(T)} not found in GameServices.");
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}
