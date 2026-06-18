namespace System;

public static class TypeExtensions
{
    extension(IEnumerable<Type> allTypes)
    {
        public IEnumerable<Type> Derives(Type baseType)
        {
            ArgumentNullException.ThrowIfNull(baseType);

            var snapshot = allTypes as IReadOnlyCollection<Type> ?? allTypes.ToArray();

            return baseType.IsGenericType
                ? snapshot.DerivesGeneric(baseType)
                : snapshot.DerivesNonGeneric(baseType);
        }

        private IEnumerable<Type> DerivesGeneric(Type baseType)
        {
            ArgumentNullException.ThrowIfNull(baseType);

            return allTypes
                .Where(t =>
                    t.BaseType != null
                    && t.BaseType.IsGenericType
                    && t.BaseType.GetGenericTypeDefinition() == baseType
                )
                .DerivesRecursively(allTypes);
        }

        private IEnumerable<Type> DerivesNonGeneric(Type baseType)
        {
            ArgumentNullException.ThrowIfNull(baseType);

            // IsAssignableFrom is transitive, so this already returns every
            // descendant at any depth — no recursion needed (that would only
            // re-emit deeper types once per ancestor).
            return allTypes.Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }
    }

    extension(IEnumerable<Type> derivedTypes)
    {
        private IEnumerable<Type> DerivesRecursively(IEnumerable<Type> allTypes)
        {
            foreach (var derivedType in derivedTypes)
            {
                yield return derivedType;

                var recursiveTypes = allTypes.Derives(derivedType);

                foreach (var recursiveType in recursiveTypes)
                {
                    yield return recursiveType;
                }
            }
        }
    }
}
