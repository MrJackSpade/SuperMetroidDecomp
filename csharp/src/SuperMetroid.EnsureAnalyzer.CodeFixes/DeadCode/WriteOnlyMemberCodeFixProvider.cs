using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// CS0414, a private field assigned but never read, enforced by the asynchronous cleanup (#1659). The field and every
/// write to it go (<see cref="Writes"/>): a write of a value free of effects is deleted, any other keeps its value as
/// <c>_ = value;</c>, and an assignment used as a value is replaced by the value. An initializer that could have an
/// effect is kept at the start of the type's constructors (<see cref="InitializerEvaluation"/>).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WriteOnlyMemberCodeFixProvider)), Shared]
public sealed class WriteOnlyMemberCodeFixProvider : DocumentFixProvider
{
    /// <summary>The compiler's assigned-but-never-read field warning.</summary>
    public const string FieldId = "CS0414";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [FieldId];

    /// <inheritdoc />
    internal override string Title => "Remove the member that is never read";

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no syntax tree.");
        SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no semantic model.");
        Solution solution = document.Project.Solution;
        var edits = new SolutionEdits(solution);
        var declarators = new List<VariableDeclaratorSyntax>();
        var properties = new List<PropertyDeclarationSyntax>();
        var sites = new Dictionary<SyntaxNode, Diagnostic>();
        var kept = new List<KeptInitializer>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            SyntaxToken identifier = root.FindToken(diagnostic.Location.SourceSpan.Start);
            ISymbol member;
            ExpressionSyntax? initializer;
            TypeSyntax type;
            switch (identifier.Parent)
            {
                case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax } declaration } declarator:
                    member = model.GetDeclaredSymbol(declarator, cancellationToken) ?? throw new CleanupFailure(diagnostic, "the field has no symbol");
                    initializer = declarator.Initializer?.Value;
                    type = declaration.Type;
                    declarators.Add(declarator);
                    sites[declarator] = diagnostic;
                    break;
                case PropertyDeclarationSyntax property when property.Identifier == identifier && IsAutomatic(property):
                    member = model.GetDeclaredSymbol(property, cancellationToken) ?? throw new CleanupFailure(diagnostic, "the property has no symbol");
                    initializer = property.Initializer?.Value;
                    type = property.Type;
                    properties.Add(property);
                    break;
                default:
                    throw new CleanupFailure(diagnostic, $"the finding is at a {identifier.Parent?.Kind()}, not a field or an automatic property");
            }
            var references = await MemberReferences.FindAsync(member, solution, cancellationToken).ConfigureAwait(false);
            await Writes.RemoveAsync(edits, member, references, diagnostic, cancellationToken).ConfigureAwait(false);
            if (initializer is not null && !SideEffects.IsFree(model.GetOperation(initializer, cancellationToken)))
                kept.Add(new KeptInitializer(member, initializer, type, model));
        }
        SourceEdits own = await edits.ForAsync(root.SyntaxTree, cancellationToken).ConfigureAwait(false);
        VariableDeclarations.Remove(own, declarators, node => sites[node]);
        foreach (PropertyDeclarationSyntax property in properties)
            own.Remove(property);
        await InitializerEvaluation.KeepAsync(edits, kept, diagnostics[0], cancellationToken).ConfigureAwait(false);
        return edits.Apply();
    }

    /// <summary>Whether a property is automatic: accessors without bodies, so its value is only stored.</summary>
    private static bool IsAutomatic(PropertyDeclarationSyntax property) =>
        property.ExpressionBody is null && property.AccessorList is { } accessors &&
        accessors.Accessors.All(accessor => accessor.Body is null && accessor.ExpressionBody is null);
}
