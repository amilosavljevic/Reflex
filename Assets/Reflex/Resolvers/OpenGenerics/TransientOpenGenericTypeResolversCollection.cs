using System;
using Reflex.Enums;

namespace Reflex.Resolvers.OpenGenerics
{
    internal class TransientOpenGenericTypeResolversCollection : OpenGenericTypeResolversCollection
    {
        public override Lifetime Lifetime => Lifetime.Transient;

        public TransientOpenGenericTypeResolversCollection(Type openGenericConcreteType) : base(openGenericConcreteType)
        {
        }

        protected override IResolver CreateResolver(Type type) =>
            new TransientTypeResolver(type);
    }
}
