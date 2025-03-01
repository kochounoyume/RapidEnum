using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RapidEnum;

public record RapidEnumGeneratorContext
{
    public string GeneratedFileName => $"{ClassName}.g.cs";
    public string ClassName { get; }
    
    public string? NameSpace { get; }
    public string? Accessibility { get; }
    
    public string EnumFullName { get; }
    public string[] EnumNames { get; }

    public RapidEnumGeneratorContext(INamedTypeSymbol enumSymbol)
    {
        ClassName = $"{enumSymbol.Name}EnumExtensions";

        Accessibility = GetAccessibilityName(enumSymbol.DeclaredAccessibility);
        NameSpace = enumSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : enumSymbol.ContainingNamespace.ToDisplayString();

        EnumFullName = enumSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        EnumNames = GetEnumNames(enumSymbol);
    }

    public RapidEnumGeneratorContext(INamedTypeSymbol targetSymbol, INamedTypeSymbol enumSymbol)
    {
        ClassName = targetSymbol.Name;
        
        NameSpace = targetSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : targetSymbol.ContainingNamespace.ToDisplayString();
        Accessibility = GetAccessibilityName(targetSymbol.DeclaredAccessibility);

        EnumFullName = enumSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        EnumNames = GetEnumNames(enumSymbol);
    }

    private static string GetAccessibilityName(Accessibility accessibility)
    {
        return accessibility switch
        {
            Microsoft.CodeAnalysis.Accessibility.Internal => "internal",
            Microsoft.CodeAnalysis.Accessibility.Public => "public",
            _ => ""
        };
    }
    
    private static string[] GetEnumNames(INamedTypeSymbol enumSymbol)
    {
        return enumSymbol.GetMembers()
            .Where(x => x.Kind == SymbolKind.Field && x is IFieldSymbol { HasConstantValue: true })
            .Select(x => x.ToDisplayString())
            .ToArray();
    }
    
    public virtual bool Equals(RapidEnumGeneratorContext? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityContract == other.EqualityContract &&
               EqualityComparer<string>.Default.Equals(ClassName, other.ClassName) &&
               EqualityComparer<string?>.Default.Equals(NameSpace, other.NameSpace) &&
               EqualityComparer<string?>.Default.Equals(Accessibility, other.Accessibility) &&
               EqualityComparer<string>.Default.Equals(EnumFullName, other.EnumFullName) &&
               EnumNames.SequenceEqual(other.EnumNames);
    }

    public override int GetHashCode()
    {
        var hashCode = ClassName.GetHashCode();
        hashCode = (hashCode * 397) ^ (NameSpace?.GetHashCode() ?? 17);
        hashCode = (hashCode * 397) ^ (Accessibility?.GetHashCode() ?? 17);
        hashCode = (hashCode * 397) ^ EnumFullName.GetHashCode();
        hashCode = (hashCode * 397) ^ EnumNames.GetHashCode();
        return hashCode;
    }
}