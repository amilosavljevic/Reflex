using System;
using Reflex.Enums;

namespace Reflex.Resolvers.OpenGenerics
{
    internal class SingletonOpenGenericTypeResolversCollection : OpenGenericTypeResolversCollection
    {
        public override Lifetime Lifetime => Lifetime.Transient;

        public SingletonOpenGenericTypeResolversCollection(Type openGenericConcreteType) : base(openGenericConcreteType)
        {
        }

        protected override IResolver CreateResolver(Type type) =>
            new SingletonTypeResolver(type);
    }
}
