using Microsoft.CodeAnalysis;

namespace CodeAnalysisService.Helpers
{
    public static class TypeHelper
    {
        /// <summary>
        /// Try to unwrap to an element type. 
        /// </summary>
        public static ITypeSymbol? GetElementType(ITypeSymbol type)
        {
            if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) return ((INamedTypeSymbol)type).TypeArguments[0];
            
            if (type is IArrayTypeSymbol arrayType) return arrayType.ElementType;

            if (type is INamedTypeSymbol namedType)
            {
                switch (true)
                {
                    case true when namedType.MetadataName == "Dictionary`2" && namedType.TypeArguments.Length == 2:
                        return namedType.TypeArguments[1];

                    case true when namedType.MetadataName == "ValueCollection" && namedType.ContainingType?.ConstructedFrom?.MetadataName == "Dictionary`2":
                        return namedType.ContainingType.TypeArguments[1];

                    case true when namedType.MetadataName == "WeakReference`1" && namedType.TypeArguments.Length == 1:
                        return namedType.TypeArguments[0];

                    case true when namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T && namedType.TypeArguments.Length == 1:
                        return namedType.TypeArguments[0];

                    case true when namedType.TypeArguments.Length == 1 &&
                        namedType.AllInterfaces.Any(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T):
                        return namedType.TypeArguments[0];
                }

            }

            return null;
        }

        public static ITypeSymbol GetInnerMostElementType(ITypeSymbol type)
        {
            var current = type;
            ITypeSymbol? next;
            while ((next = GetElementType(current)) != null)
            {
                current = next;
            }
            return current;
        }
    }
}

