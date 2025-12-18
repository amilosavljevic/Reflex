using System;
using Reflex.Enums;

namespace Reflex.Resolvers.OpenGenerics
{
    public class ScopedOpenGenericTypeResolversCollection : OpenGenericTypeResolversCollection
    {
        public override Lifetime Lifetime => Lifetime.Transient;

        public ScopedOpenGenericTypeResolversCollection(Type openGenericConcreteType) : base(openGenericConcreteType)
        {
        }

        protected override IResolver CreateResolver(Type type) =>
            new ScopedTypeResolver(type);
    }
}
