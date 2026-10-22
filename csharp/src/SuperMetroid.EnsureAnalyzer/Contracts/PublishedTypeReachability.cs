using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// The Core types a published value reaches: the types named as its roots, every type their instance fields, properties
/// and base types reach, and every type derived from one, as a kind of a published value is published too. The
/// frame-composition (SME6442) and display-packet (SME6443) rules report members of these types.
/// </summary>
internal static class PublishedTypeReachability
{
    /// <summary>The Core types reachable from the types named <paramref name="roots"/>.</summary>
    internal static ImmutableHashSet<INamedTypeSymbol> Reachable(Compilation compilation, IAssemblySymbol core, ImmutableHashSet<string> roots)
    {
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var pending = new Stack<INamedTypeSymbol>();
        foreach (INamedTypeSymbol root in compilation.GetSymbolsWithName(roots.Contains, SymbolFilter.Type).OfType<INamedTypeSymbol>())
            Enqueue(root);
        INamedTypeSymbol[] all = [.. Types(core.GlobalNamespace)];
        bool grew = true;
        while (grew)
        {
            Drain();
            grew = false;
            foreach (INamedTypeSymbol type in all)
            {
                if (seen.Contains(type))
                    continue;
                for (INamedTypeSymbol? baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
                {
                    if (seen.Contains(baseType.OriginalDefinition))
                    {
                        Enqueue(type);
                        grew = true;
                        break;
                    }
                }
            }
        }
        return seen.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        void Drain()
        {
            while (pending.Count > 0)
            {
                INamedTypeSymbol type = pending.Pop();
                if (type.BaseType is { } baseType)
                    Visit(baseType);
                foreach (ISymbol member in type.GetMembers())
                {
                    if (member.IsStatic)
                        continue;
                    if (member is IFieldSymbol field)
                        Visit(field.Type);
                    else if (member is IPropertySymbol property)
                        Visit(property.Type);
                }
            }
        }

        void Visit(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol array)
            {
                Visit(array.ElementType);
            }
            else if (type is INamedTypeSymbol named)
            {
                foreach (ITypeSymbol argument in named.TypeArguments)
                    Visit(argument);
                Enqueue(named.OriginalDefinition);
            }
        }

        void Enqueue(INamedTypeSymbol type)
        {
            if (SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, core) && seen.Add(type))
                pending.Push(type);
        }
    }

    /// <summary>The field or property type of a source-declared instance member of a reachable type, or null for any other member.</summary>
    internal static ITypeSymbol? ReachedMemberType(ISymbol symbol, ImmutableHashSet<INamedTypeSymbol> reachable, out Location? location)
    {
        location = null;
        if (symbol.IsStatic || symbol.IsImplicitlyDeclared || !reachable.Contains(symbol.ContainingType.OriginalDefinition))
            return null;
        location = symbol.Locations.FirstOrDefault(candidate => candidate.IsInSource);
        if (location is null)
            return null;
        return symbol switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => null,
        };
    }

    /// <summary>Every type declared in <paramref name="space"/> and its nested namespaces and types.</summary>
    private static IEnumerable<INamedTypeSymbol> Types(INamespaceOrTypeSymbol space)
    {
        foreach (ISymbol member in space.GetMembers())
        {
            if (member is INamespaceOrTypeSymbol nested)
            {
                if (nested is INamedTypeSymbol type)
                    yield return type;
                foreach (INamedTypeSymbol inner in Types(nested))
                    yield return inner;
            }
        }
    }
}
