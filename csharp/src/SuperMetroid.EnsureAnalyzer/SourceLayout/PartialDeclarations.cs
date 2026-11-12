using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.SourceLayout;

/// <summary>A partial type's declarations, and which of them are hand-written rather than generated (#621).</summary>
public sealed class PartialDeclarations
{
    private PartialDeclarations(List<TypeDeclarationSyntax> all, List<TypeDeclarationSyntax> handWritten)
    {
        All = all;
        HandWritten = handWritten;
    }

    /// <summary>Every declaration of the type, in the order its syntax references list them.</summary>
    public IReadOnlyList<TypeDeclarationSyntax> All { get; }

    /// <summary>The declarations that no generator wrote (<see cref="GeneratedSource"/>).</summary>
    public IReadOnlyList<TypeDeclarationSyntax> HandWritten { get; }

    /// <summary>The declarations of <paramref name="type"/> when one of them is partial and one is hand-written, or
    /// null otherwise.</summary>
    public static PartialDeclarations? Of(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        var declarations = type.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax(cancellationToken))
            .OfType<TypeDeclarationSyntax>()
            .ToList();
        if (!declarations.Any(declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword)))
            return null;
        var handWritten = declarations.Where(declaration => !GeneratedSource.IsGenerated(declaration.SyntaxTree, cancellationToken)).ToList();
        return handWritten.Count == 0 ? null : new PartialDeclarations(declarations, handWritten);
    }
}
