using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>What every step of a relocation asks of symbols and their declarations: owners, names, paths and parts.</summary>
internal static class RelocationSymbols
{
    /// <summary>Compares Roslyn symbols across declaration and reference passes.</summary>
    internal static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;

    /// <summary>Includes a single-variable field's whole declaration when relocating its symbol.</summary>
    internal static SyntaxNode DeclarationNode(SyntaxNode node) => node switch
    {
        VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax field } when
            ((VariableDeclarationSyntax)node.Parent).Variables.Count == 1 => field,
        _ => node,
    };

    /// <summary>Walks all containing types so members of moved types are not moved twice.</summary>
    internal static IEnumerable<INamedTypeSymbol> Containers(ISymbol symbol)
    {
        for (var type = symbol.ContainingType; type is not null; type = type.ContainingType)
            yield return type.OriginalDefinition;
    }

    /// <summary>Checks whether a candidate owner appears in the original owner's type or base chain.</summary>
    internal static bool InOwnerChain(INamedTypeSymbol owner, INamedTypeSymbol candidate)
    {
        for (var type = owner; type is not null; type = type.ContainingType)
            for (var current = type; current is not null; current = current.BaseType)
                if (Comparer.Equals(current.OriginalDefinition, candidate.OriginalDefinition)) return true;
        return false;
    }

    /// <summary>A staying type's name as written from its own namespace: containing types joined by '.'.</summary>
    internal static string TypeName(INamedTypeSymbol type) =>
        type.ContainingType is { } outer ? TypeName(outer) + "." + type.Name : type.Name;

    /// <summary>The type that declares a member: for a member of an extension block, the static class declaring the block.</summary>
    internal static INamedTypeSymbol? DeclaringOwner(ISymbol symbol) =>
        symbol.ContainingType is { IsExtension: true, ContainingType: { } declaring } ? declaring : symbol.ContainingType;

    /// <summary>Formats a repository-relative source location for manual relocation reports.</summary>
    internal static string Location(SymbolIdentity identity, SyntaxNode node) =>
        $"{identity.Relative(node.SyntaxTree.FilePath)}:{node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}";

    /// <summary>
    /// csharp/src/SuperMetroid.Core/Game/X.cs maps to Core/Game/X.cs under the target, or to Game/X.cs
    /// when the target already stands for the source project.
    /// </summary>
    internal static string TargetRelativePath(string repositoryRelative, bool keepsProjectDirectory)
    {
        string[] parts = repositoryRelative.Split('/');
        int project = Array.FindIndex(parts, p => p.StartsWith("SuperMetroid.", StringComparison.Ordinal));
        if (project < 0) throw new InvalidOperationException($"{repositoryRelative} is not under a project directory.");
        return keepsProjectDirectory
            ? Path.Combine([parts[project]["SuperMetroid.".Length..], .. parts[(project + 1)..]])
            : Path.Combine(parts[(project + 1)..]);
    }

    /// <summary>
    /// Every repository declaration of a symbol: each part of a partial type, and both parts of a partial method, property or
    /// event, whose accessibility must stay identical (CS8799). A part a source generator writes (a LibraryImport stub, say)
    /// is not repository source: the generator writes it again from the declaration it implements.
    /// </summary>
    internal static IEnumerable<SyntaxNode> DeclarationsOf(SymbolIdentity identity, ISymbol symbol)
    {
        ISymbol? implementation = symbol switch
        {
            IMethodSymbol method => method.PartialImplementationPart,
            IPropertySymbol property => property.PartialImplementationPart,
            IEventSymbol @event => @event.PartialImplementationPart,
            _ => null,
        };
        return symbol.DeclaringSyntaxReferences.Concat(implementation?.DeclaringSyntaxReferences ?? [])
            .Where(reference => identity.IsRepositorySource(reference.SyntaxTree))
            .Select(reference => reference.GetSyntax()).Distinct();
    }

    /// <summary>
    /// Whether a symbol is a partial method declared without an implementation: implicitly private, with every call to it,
    /// arguments included, left out of the program. An accessibility modifier would require an implementation (CS8795).
    /// </summary>
    internal static bool Unimplemented(ISymbol symbol) => symbol is IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: null };
}
