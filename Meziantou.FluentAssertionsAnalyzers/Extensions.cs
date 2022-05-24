﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

internal static class Extensions
{
    public static bool Equals(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        return symbol.Equals(compilation.GetTypeByMetadataName(type), SymbolEqualityComparer.Default);
    }

    public static bool IsOrImplements(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        var otherSymbol = compilation.GetTypeByMetadataName(type);
        if (symbol.Equals(otherSymbol, SymbolEqualityComparer.Default))
            return true;

        foreach (var s in symbol.AllInterfaces)
        {
            if (s.OriginalDefinition.Equals(otherSymbol, SymbolEqualityComparer.Default))
                return true;
        }

        return false;
    }

    public static bool IsOrInheritsFrom(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        var otherSymbol = compilation.GetTypeByMetadataName(type);

        do
        {
            if (symbol.Equals(otherSymbol, SymbolEqualityComparer.Default))
                return true;

            symbol = symbol.BaseType;
        } while (symbol != null);

        return false;
    }

    public static IOperation RemoveConversion(this IOperation operation)
    {
        while (operation is IConversionOperation conversionOperation)
        {
            operation = conversionOperation.Operand;
        }

        return operation;
    }
}
