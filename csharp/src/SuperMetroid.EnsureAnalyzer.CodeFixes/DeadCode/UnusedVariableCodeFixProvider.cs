using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// CS0168, a variable declared and never used, enforced by the asynchronous cleanup (#1656). Nothing runs for such a
/// variable, so removing it changes nothing: a local declared without an initializer goes from its declaration (the
/// whole statement when it was the only variable; a constant goes the same way), an unused <c>catch</c> variable
/// leaves <c>catch (T)</c>, and a
/// pattern or <c>out</c> designation becomes the discard <c>_</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnusedVariableCodeFixProvider)), Shared]
public sealed class UnusedVariableCodeFixProvider : DocumentFixProvider
{
    /// <summary>The compiler's unused-variable warning.</summary>
    public const string Id = "CS0168";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [Id];

    /// <inheritdoc />
    internal override string Title => "Remove the unused variable";

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no syntax tree.");
        var edits = new SourceEdits(await document.GetTextAsync(cancellationToken).ConfigureAwait(false), document.FilePath ?? document.Name);
        var declarators = new List<VariableDeclaratorSyntax>();
        var sites = new Dictionary<SyntaxNode, Diagnostic>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            SyntaxToken identifier = root.FindToken(diagnostic.Location.SourceSpan.Start);
            switch (identifier.Parent)
            {
                case VariableDeclaratorSyntax declarator when declarator.Initializer is null ||
                    declarator.Parent?.Parent is LocalDeclarationStatementSyntax { IsConst: true }:
                    declarators.Add(declarator);
                    sites[declarator] = diagnostic;
                    break;
                case CatchDeclarationSyntax catchDeclaration when catchDeclaration.Identifier == identifier:
                    edits.Replace(TextSpan.FromBounds(catchDeclaration.Type.Span.End, identifier.Span.End), string.Empty);
                    break;
                case SingleVariableDesignationSyntax:
                    edits.Replace(identifier.Span, "_");
                    break;
                default:
                    throw new CleanupFailure(diagnostic, $"the variable is declared by a {identifier.Parent?.Kind()}");
            }
        }
        LocalDeclarations.Remove(edits, declarators, node => sites[node]);
        return document.WithText(edits.Apply()).Project.Solution;
    }
}
