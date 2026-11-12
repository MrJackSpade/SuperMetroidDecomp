using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// IDE0051, an unused private member (#1661), CA1823, an unused private field (#1662), and CS0169, a field never used
/// (#1663, every such field, with no exception for a type's memory layout), enforced by the asynchronous cleanup. The
/// member goes: a method,
/// property, indexer, field, event or nested type. A field or property initializer that could have an effect keeps its
/// evaluation at the start of the type's constructors (<see cref="InitializerEvaluation"/>). Reflection by name into
/// owned state is already forbidden (SME6373), so it is no hazard; debugger save states key fields by name, so a state
/// taken before the fix loses that field's value, which loading only warns about, as after any field rename. A member
/// still named somewhere (in <c>nameof</c>, say) fails the fix.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnusedMemberCodeFixProvider)), Shared]
public sealed class UnusedMemberCodeFixProvider : DocumentFixProvider
{
    /// <summary>The code-style rule for an unused private member.</summary>
    public const string UnusedId = "IDE0051";

    /// <summary>The code-analysis rule for an unused private field.</summary>
    public const string UnusedFieldId = "CA1823";

    /// <summary>The compiler's never-used field warning.</summary>
    public const string NeverUsedFieldId = "CS0169";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [UnusedId, UnusedFieldId, NeverUsedFieldId];

    /// <inheritdoc />
    internal override string Title => "Remove the unused member";

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
        var members = new List<MemberDeclarationSyntax>();
        var sites = new Dictionary<SyntaxNode, Diagnostic>();
        var kept = new List<KeptInitializer>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            SyntaxToken identifier = root.FindToken(diagnostic.Location.SourceSpan.Start);
            SyntaxNode declaration;
            ExpressionSyntax? initializer = null;
            TypeSyntax? type = null;
            switch (identifier.Parent)
            {
                case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: BaseFieldDeclarationSyntax } variables } declarator:
                    declaration = declarator;
                    initializer = declarator.Initializer?.Value;
                    type = variables.Type;
                    declarators.Add(declarator);
                    break;
                case MethodDeclarationSyntax method when method.Identifier == identifier && !method.Modifiers.Any(SyntaxKind.PartialKeyword):
                    declaration = method;
                    members.Add(method);
                    break;
                case PropertyDeclarationSyntax property when property.Identifier == identifier:
                    declaration = property;
                    initializer = property.Initializer?.Value;
                    type = property.Type;
                    members.Add(property);
                    break;
                case IndexerDeclarationSyntax indexer when indexer.ThisKeyword == identifier:
                    declaration = indexer;
                    members.Add(indexer);
                    break;
                case EventDeclarationSyntax eventDeclaration when eventDeclaration.Identifier == identifier:
                    declaration = eventDeclaration;
                    members.Add(eventDeclaration);
                    break;
                case BaseTypeDeclarationSyntax nested when nested.Identifier == identifier && !nested.Modifiers.Any(SyntaxKind.PartialKeyword):
                    declaration = nested;
                    members.Add(nested);
                    break;
                case DelegateDeclarationSyntax nested when nested.Identifier == identifier:
                    declaration = nested;
                    members.Add(nested);
                    break;
                default:
                    throw new CleanupFailure(diagnostic, $"the finding is at a {identifier.Parent?.Kind()}, not a whole member of one declaration");
            }
            sites[declaration] = diagnostic;
            ISymbol member = model.GetDeclaredSymbol(declaration, cancellationToken) ?? throw new CleanupFailure(diagnostic, "the member has no symbol");
            foreach (ReferenceLocation reference in await MemberReferences.FindAsync(member, solution, cancellationToken).ConfigureAwait(false))
            {
                if (reference.Location.SourceTree != declaration.SyntaxTree || !declaration.Span.Contains(reference.Location.SourceSpan))
                    throw new CleanupFailure(reference.Location, diagnostic.Id, $"'{member.Name}' is still named here");
            }
            if (initializer is not null && type is not null && !SideEffects.IsFree(model.GetOperation(initializer, cancellationToken)))
                kept.Add(new KeptInitializer(member, initializer, type, model));
        }
        SourceEdits own = await edits.ForAsync(root.SyntaxTree, cancellationToken).ConfigureAwait(false);
        VariableDeclarations.Remove(own, declarators, node => sites[node]);
        foreach (MemberDeclarationSyntax member in members)
            own.Remove(member);
        await InitializerEvaluation.KeepAsync(edits, kept, diagnostics[0], cancellationToken).ConfigureAwait(false);
        return edits.Apply();
    }
}
