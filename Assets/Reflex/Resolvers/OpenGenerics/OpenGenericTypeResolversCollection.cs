using System;
using System.Collections.Concurrent;
using System.Linq;
using Reflex.Core;
using Reflex.Enums;

namespace Reflex.Resolvers.OpenGenerics
{
    /// <summary>
    /// This collection is just resolver for the sake of handing it as any other resolver registration, but in practice
    /// Resolve should never be called on it directly. Instead, when resolving an open-generic type, the container
    /// should create a closed-generic resolver for the specific closed type being requested, cache it here, and delegate
    /// the resolution to that closed-generic resolver.
    /// </summary>
    internal abstract class OpenGenericTypeResolversCollection : IResolver
    {
        private readonly Type _openGenericConcreteType;
        private readonly ConcurrentDictionary<Type, IResolver> _closedResolvers = new();
        private readonly Func<Type, IResolver> _cachedResolverFactory;

        public abstract Lifetime Lifetime { get; }

        protected OpenGenericTypeResolversCollection(Type openGenericConcreteType)
        {
            _openGenericConcreteType = openGenericConcreteType;
            // caching the factory method to avoid allocating a new delegate on each GetOrAdd call
            _cachedResolverFactory = CreateOpenGenericResolverForType;
        }

        public IResolver GetOrCreateClosedResolver(Type type) =>
            _closedResolvers.GetOrAdd(type, _cachedResolverFactory);

        /// <summary>
        /// For passed closed-generic contract type, creates a closed-generic resolver for the corresponding closed concrete type.
        /// </summary>
        private IResolver CreateOpenGenericResolverForType(Type type)
        {
            // TODO: optimize?
            var closedConcreteType = _openGenericConcreteType.MakeGenericType(type.GetGenericArguments());
            return CreateResolver(closedConcreteType);
        }

        protected abstract IResolver CreateResolver(Type type);

        public object Resolve(Container container)
        {
            throw new InvalidOperationException("Open-generic resolvers cannot resolve instances directly. " +
                "A closed-generic resolver must be created for the specific closed type being requested.");
        }

        public void Dispose()
        {
            foreach (var resolver in _closedResolvers.Values.ToList())
                resolver.Dispose();
            _closedResolvers.Clear();
        }
    }
}
