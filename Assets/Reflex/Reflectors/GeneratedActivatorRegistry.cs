using System;
using Reflex.Caching;
using Reflex.Delegates;
using UnityEngine.Scripting;

namespace Reflex.Reflectors
{
    // [Preserve] keeps the type alive under IL2CPP managed-code stripping. RegisterCompat is
    // resolved from generated code via Type.GetType + GetMethod (reflection) — IL2CPP can't see
    // that as a static reference, so without this attribute the entire registry could be stripped
    // and generator-emitted activators would silently no-op (falling back to reflection-based
    // construction). The attribute closes that gap.
    [Preserve]
    public static class GeneratedActivatorRegistry
    {
        // Strongly-typed entry point. Source generators emitting code that DOES reference Reflex
        // (e.g. consumers with a direct Reflex asmdef ref) can call this with the native
        // ObjectActivator delegate. Pre-seeds TypeConstructionInfoCache so the container's
        // resolve path returns the supplied activator without reflection.
        public static void Register(Type concrete, ObjectActivator activator, Type[] parameterTypes)
        {
            if (concrete == null) throw new ArgumentNullException(nameof(concrete));
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));
            TypeConstructionInfoCache.Prime(concrete, activator, parameterTypes);
        }

        // BCL-only entry point. Lets source-generated code in assemblies that DON'T reference
        // Reflex (the common case — installer code goes through IContainerBuilder abstractions
        // and never names a Reflex type) populate the registry. The generator resolves this
        // method by name via Type.GetType once at module/runtime init, so the generated
        // assembly never needs to name ObjectActivator (which lives in Reflex.dll).
        [Preserve]
        public static void RegisterCompat(Type concrete, Func<object[], object> activator, Type[] parameterTypes)
        {
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            Register(concrete, args => activator(args), parameterTypes);
        }

        // Public introspection helper. Returns the parameter types of the constructor Reflex
        // would use to construct `concrete` (mirrors the [ReflexConstructor]-or-max-arity rule
        // applied by TypeConstructionInfoCache.Generate).
        //
        // Consumers like FastChair.GameEssentials.DiAbstractions.ReflexBindings.PartialConstructorInjector
        // need this to pre-fill known arguments and resolve the rest from the container.
        // Exposing it as a stable public API removes the previous reflection-based backdoor
        // in ReflexUtilities (the old code grabbed the internal TypeConstructionInfo.ConstructorParameters
        // field by name and broke whenever the field was renamed or its shape changed).
        public static Type[] GetConstructorParameterTypes(Type concrete)
        {
            if (concrete == null) throw new ArgumentNullException(nameof(concrete));
            var info = TypeConstructionInfoCache.Get(concrete);
            var src = info.ConstructorParameters;
            var copy = new Type[src.Length];
            Array.Copy(src, copy, src.Length);
            return copy;
        }
    }
}
