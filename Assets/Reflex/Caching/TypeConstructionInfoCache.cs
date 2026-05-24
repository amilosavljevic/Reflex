using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using Reflex.Delegates;
using Reflex.Extensions;
using Reflex.Reflectors;

namespace Reflex.Caching
{
    internal static class TypeConstructionInfoCache
    {
        private static readonly Dictionary<IntPtr, TypeConstructionInfo> _dictionary = new();

        internal static TypeConstructionInfo Get(Type type)
        {
            var key = type.TypeHandle.Value;
            if (!_dictionary.TryGetValue(key, out var info))
            {
                info = Generate(type);
                _dictionary.Add(key, info);
            }

            return info;
        }

        // Pre-seeds the cache with an externally supplied activator (typically emitted by a
        // source generator and installed via Reflex.Reflectors.GeneratedActivatorRegistry.Register).
        // Called from module/runtime init before any container resolves, so no synchronization needed.
        // Get() is left unchanged from upstream baseline; its existing memoization returns
        // primed entries before Generate() is ever called.
        internal static void Prime(Type type, ObjectActivator activator, Type[] parameterTypes)
        {
            _dictionary[type.TypeHandle.Value] = new TypeConstructionInfo(activator, parameterTypes);
        }
        
        private static TypeConstructionInfo Generate(Type type)
        {
            if (type.TryGetConstructors(out var constructors))
            {
                var constructor = constructors.FirstOrDefault(c => Attribute.IsDefined(c, typeof(ReflexConstructorAttribute))); // Try to get a constructor that defines ReflexConstructor

                if (constructor == null)
                {
                    constructor = constructors.MaxBy(ctor => ctor.GetParameters().Length); // Gets the constructor with most arguments
                }
                
                var parameters = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
                return new TypeConstructionInfo(ActivatorFactoryManager.Factory.GenerateActivator(type, constructor, parameters), parameters);
            }

            // Should we add this complexity yo be able to inject value types?
            return new TypeConstructionInfo(ActivatorFactoryManager.Factory.GenerateDefaultActivator(type), Type.EmptyTypes);
        }
    }
}