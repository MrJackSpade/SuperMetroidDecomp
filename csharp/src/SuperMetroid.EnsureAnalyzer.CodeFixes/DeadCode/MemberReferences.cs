using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>Finds the references to a private member, which only its type's declarations can hold.</summary>
internal static class MemberReferences
{
    /// <summary>Every reference to <paramref name="member"/> in the documents that declare its outermost type.</summary>
    internal static async Task<List<ReferenceLocation>> FindAsync(ISymbol member, Solution solution, CancellationToken cancellationToken)
    {
        INamedTypeSymbol type = member.ContainingType;
        while (type.ContainingType is { } outer)
            type = outer;
        var documents = ImmutableHashSet.CreateRange(type.DeclaringSyntaxReferences
            .Select(reference => solution.GetDocument(reference.SyntaxTree)).OfType<Document>());
        var found = await SymbolFinder.FindReferencesAsync(member, solution, documents, cancellationToken).ConfigureAwait(false);
        return [.. found.SelectMany(symbol => symbol.Locations)];
    }
}
