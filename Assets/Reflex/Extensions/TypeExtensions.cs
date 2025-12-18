using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reflex.Extensions
{
    internal static class TypeExtensions
    {
        internal static bool IsEnumerable(this Type type, out Type elementType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GenericTypeArguments.Single();
                return true;
            }

            elementType = null;
            return false;
        }

        internal static bool TryGetConstructors(this Type type, out ConstructorInfo[] constructors)
        {
            constructors = type.GetConstructors();
            return constructors.Length > 0;
        }

        internal static string GetName(this Type type)
        {
            if (type.IsGenericType)
            {
                var outerTypeName = type.Name!.Split('`').First();
                var innerTypeNames = string.Join(", ", type.GenericTypeArguments.Select(GetName));
                return $"{outerTypeName}<{innerTypeNames}>";
            }

            return type.Name;
        }

        internal static string GetFullName(this Type type)
        {
            if (type.IsGenericType)
            {
                var outerTypeName = type.FullName!.Split('`').First();
                var innerTypeNames = string.Join(", ", type.GenericTypeArguments.Select(GetFullName));
                return $"{outerTypeName}<{innerTypeNames}>";
            }

            return type.FullName;
        }

        /// <summary>
        /// Returns true if <paramref name="openGenericTarget"/> is assignable from
        /// <paramref name="openGenericCandidate"/>, where both are open generic type definitions
        /// (e.g. typeof(IEnumerable&lt;&gt;), typeof(List&lt;&gt;)).
        /// </summary>
        internal static bool IsOpenGenericAssignableFrom(this Type openGenericTarget, Type openGenericCandidate)
        {
            if (openGenericTarget is null)
                throw new ArgumentNullException(nameof(openGenericTarget));

            if (openGenericCandidate is null)
                throw new ArgumentNullException(nameof(openGenericCandidate));

            if (!openGenericTarget.IsGenericTypeDefinition)
                throw new ArgumentException("Must be an open generic type definition.", nameof(openGenericTarget));

            if (!openGenericCandidate.IsGenericTypeDefinition)
                throw new ArgumentException("Must be an open generic type definition.", nameof(openGenericCandidate));

            // Quick rejects
            if (openGenericTarget == openGenericCandidate)
                return true;

            // Walk candidate's base-type chain
            for (var t = openGenericCandidate; t != null && t != typeof(object); t = t.BaseType)
            {
                if (t.IsGenericTypeDefinition && t == openGenericTarget)
                    return true;

                if (t.IsGenericType && t.GetGenericTypeDefinition() == openGenericTarget)
                    return true;
            }

            // Walk candidate's interfaces
            foreach (var it in openGenericCandidate.GetInterfaces())
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == openGenericTarget)
                    return true;

                if (it.IsGenericTypeDefinition && it == openGenericTarget)
                    return true;
            }

            return false;
        }
    }
}
