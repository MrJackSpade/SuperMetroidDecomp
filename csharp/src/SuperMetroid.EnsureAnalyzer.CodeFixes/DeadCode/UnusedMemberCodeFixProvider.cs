using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// IDE0051, an unused private member (#1661), CA1823, an unused private field (#1662), and CS0169, a field never used
/// (#1663, every such field, with no exception for a type's memory layout), enforced by the asynchronous cleanup. The
/// member goes, at every finding (#2599): a method, constructor, property, indexer, field, event or nested type, and
/// every part of a partial method or type. A field or property initializer that could have an effect keeps its
/// evaluation at the start of the type's constructors (<see cref="InitializerEvaluation"/>). Reflection by name into
/// owned state is already forbidden (SME6373), so it is no hazard; debugger save states key fields by name, so a state
/// taken before the fix loses that field's value, which loading only warns about, as after any field rename. A member
/// still named somewhere (in <c>nameof</c> or a cref, say) goes all the same, and the build fails where it is named. A
/// parameter only the removed code read, a primary constructor's among them, goes with it (<see cref="UnreadParameters"/>).
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
        var parts = new List<SyntaxReference>();
        var kept = new List<KeptInitializer>();
        var removedCode = new List<SyntaxNode?>();
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
                case MethodDeclarationSyntax method when method.Identifier == identifier:
                    declaration = method;
                    parts.AddRange(Parts(model.GetDeclaredSymbol(method, cancellationToken), diagnostic));
                    break;
                case ConstructorDeclarationSyntax constructor when constructor.Identifier == identifier:
                    declaration = constructor;
                    members.Add(constructor);
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
                case BaseTypeDeclarationSyntax nested when nested.Identifier == identifier:
                    declaration = nested;
                    parts.AddRange(Parts(model.GetDeclaredSymbol(nested, cancellationToken), diagnostic));
                    break;
                case DelegateDeclarationSyntax nested when nested.Identifier == identifier:
                    declaration = nested;
                    members.Add(nested);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"{Describe(diagnostic)}: {diagnostic.Id} is reported at a {identifier.Parent?.Kind()}, which is no member declaration this fix knows.");
            }
            ISymbol member = model.GetDeclaredSymbol(declaration, cancellationToken)
                ?? throw new InvalidOperationException($"{Describe(diagnostic)}: the {diagnostic.Id} member has no symbol.");
            if (initializer is not null && type is not null && !SideEffects.IsFree(model.GetOperation(initializer, cancellationToken)))
                kept.Add(new KeptInitializer(member, initializer, type, model));
            else if (declaration is VariableDeclaratorSyntax)
                removedCode.Add(initializer);
            else
                removedCode.Add(declaration);
        }
        SourceEdits own = await edits.ForAsync(root.SyntaxTree, cancellationToken).ConfigureAwait(false);
        VariableDeclarations.Remove(own, declarators);
        foreach (MemberDeclarationSyntax member in members)
            own.Remove(member);
        foreach (SyntaxReference part in parts)
            (await edits.ForAsync(part.SyntaxTree, cancellationToken).ConfigureAwait(false)).Remove(await part.GetSyntaxAsync(cancellationToken).ConfigureAwait(false));
        await InitializerEvaluation.KeepAsync(edits, kept, cancellationToken).ConfigureAwait(false);
        var unread = new UnreadParameters(solution, edits);
        foreach (SyntaxNode? removed in removedCode)
        {
            if (removed is not null)
                unread.Removing(removed, model);
        }
        await unread.RemoveAsync(cancellationToken).ConfigureAwait(false);
        return edits.Apply();
    }

    /// <summary>Every declaration of a partial method (both parts) or a partial type.</summary>
    private static IEnumerable<SyntaxReference> Parts(ISymbol? symbol, Diagnostic diagnostic) => symbol switch
    {
        IMethodSymbol method => new[] { method, method.PartialDefinitionPart, method.PartialImplementationPart }
            .OfType<IMethodSymbol>().SelectMany(part => part.DeclaringSyntaxReferences).Distinct(),
        INamedTypeSymbol type => type.DeclaringSyntaxReferences,
        _ => throw new InvalidOperationException($"{Describe(diagnostic)}: the {diagnostic.Id} member has no symbol."),
    };

    /// <summary>A finding's place as <c>path(line,column)</c>.</summary>
    private static string Describe(Diagnostic diagnostic)
    {
        FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
        return $"{span.Path}({span.StartLinePosition.Line + 1},{span.StartLinePosition.Character + 1})";
    }
}
