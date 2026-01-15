using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Enums;
using Reflex.Resolvers;

namespace Reflex.Core
{
    // I implemented registrations so that we can easily find all concrete types that implement specific interface.
    // Same as `IDisposable` is doing, but it's not handled as special case. I need it to implement `IInitializable` on
    // my end.
    public class Registrations
    {
        private readonly Dictionary<IResolver, Registration> registrations = new();

        public Registrations(Registrations parentRegistrations = null)
            : this(ChangeOwnership(parentRegistrations, isInherited:true))
        {
        }

        // Copy Constructor
        internal Registrations(Registrations toCopy, Registrations parentRegistrations)
            : this(ChangeOwnership(parentRegistrations, isInherited:true)
                .Concat(toCopy.GetAllMappings()))
        {
        }

        private Registrations(IEnumerable<KeyValuePair<IResolver, Registration>> rawRegistrations)
        {
            foreach (var kvp in rawRegistrations)
            {
                registrations[kvp.Key] = kvp.Value;
            }
        }

        public void Add(IResolver resolver, Registration registration) =>
            registrations[resolver] = new Registration(
                isInherited: false,
                concreteType: registration.ConcreteType,
                lifetime: registration.Lifetime,
                contractTypes: registration.ContractTypes
            );

        public bool TryGet(IResolver resolver, out Registration registration) =>
            registrations.TryGetValue(resolver, out registration);

        public Registration Get(IResolver resolver) =>
            registrations.TryGetValue(resolver, out var registration)
                ? registration
                : throw new KeyNotFoundException($"No registration found for resolver of type {resolver}");

        public IEnumerable<KeyValuePair<IResolver, Registration>> GetAllMappings() =>
            registrations;

        public IEnumerable<Registration> GetAllRegistrations() =>
            registrations.Values;

        private static IEnumerable<KeyValuePair<IResolver, Registration>> ChangeOwnership(Registrations parentRegistrations, bool isInherited = true) =>
            parentRegistrations  != null
                ? parentRegistrations
                .GetAllMappings()
                .Select(kv => new KeyValuePair<IResolver, Registration>(kv.Key, kv.Value.WithInheritedSetTo(isInherited)))
                : ArraySegment<KeyValuePair<IResolver, Registration>>.Empty; }

    public readonly struct Registration
    {
        public bool IsInherited { get; }
        public Type ConcreteType { get; }
        public Lifetime Lifetime { get; }
        public IReadOnlyList<Type> ContractTypes { get; }

        public Registration(Type concreteType, Lifetime lifetime, IReadOnlyList<Type> contractTypes)
        {
            ConcreteType = concreteType;
            Lifetime = lifetime;
            ContractTypes = contractTypes;
            IsInherited = false;
        }

        public Registration(bool isInherited, Type concreteType, Lifetime lifetime, IReadOnlyList<Type> contractTypes)
        {
            IsInherited = isInherited;
            ConcreteType = concreteType;
            Lifetime = lifetime;
            ContractTypes = contractTypes;
        }

        public Registration WithInheritedSetTo(bool isInherited)
        {
            return new Registration(
                isInherited: isInherited,
                concreteType: ConcreteType,
                lifetime: Lifetime,
                contractTypes: ContractTypes
            );
        }
    }
}
