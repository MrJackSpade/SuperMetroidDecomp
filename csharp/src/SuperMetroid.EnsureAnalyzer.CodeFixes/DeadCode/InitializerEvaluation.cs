using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// Keeps the evaluation of a removed member's initializer (#1659, #1661): <c>_ = value;</c> at the start of every
/// constructor of its type that runs field initializers (each one not chained to <c>this(...)</c>; the static
/// constructor for a static member), the kept initializers in the order the compiler runs them. A type without such a
/// constructor (none declared, or a primary constructor) fails the fix: adding one would change when the type's
/// initializers run, and a static one would change when the type is initialized.
/// </summary>
internal static class InitializerEvaluation
{
    /// <summary>Inserts the evaluations of <paramref name="kept"/>.</summary>
    internal static async Task KeepAsync(SolutionEdits edits, List<KeptInitializer> kept, Diagnostic site, CancellationToken cancellationToken)
    {
        foreach (var byType in kept.GroupBy(initializer => initializer.Member.ContainingType, SymbolEqualityComparer.Default))
        {
            var type = (INamedTypeSymbol)byType.Key!;
            foreach (var group in byType.GroupBy(initializer => initializer.Member.IsStatic))
            {
                var ordered = group.OrderBy(initializer => Order(initializer)).ThenBy(initializer => initializer.Value.SpanStart).ToList();
                var constructors = (group.Key ? type.StaticConstructors : type.InstanceConstructors).Where(c => !c.IsImplicitlyDeclared).ToList();
                if (constructors.Count == 0)
                    throw new CleanupFailure(site, $"{type.Name} declares no {(group.Key ? "static " : "")}constructor to keep the evaluation of " +
                        $"'{ordered[0].Member.Name}''s initializer in");
                foreach (IMethodSymbol constructor in constructors)
                {
                    foreach (SyntaxReference reference in constructor.DeclaringSyntaxReferences)
                    {
                        if (await reference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) is not ConstructorDeclarationSyntax declaration)
                            throw new CleanupFailure(site, $"{type.Name}'s primary constructor runs '{ordered[0].Member.Name}''s initializer");
                        if (declaration.Initializer is { } chained && chained.IsKind(SyntaxKind.ThisConstructorInitializer))
                            continue;
                        if (declaration.Body is not { } body)
                            throw new CleanupFailure(site, $"{type.Name} has an expression-bodied constructor to keep '{ordered[0].Member.Name}''s initializer in");
                        Insert(await edits.ForAsync(body.SyntaxTree, cancellationToken).ConfigureAwait(false), body,
                            ordered.Select(initializer => $"_ = {Render(initializer)};").ToList());
                    }
                }
            }
        }
    }

    /// <summary>The compiler's order of a member's syntax tree among its compilation's trees.</summary>
    private static int Order(KeptInitializer initializer) =>
        initializer.Model.Compilation.SyntaxTrees.ToList().IndexOf(initializer.Value.SyntaxTree);

    /// <summary>The initializer as a stand-alone expression: a target-typed value gets the member's type written out,
    /// and a user-defined conversion to it is made explicit, so the same work runs.</summary>
    private static string Render(KeptInitializer initializer)
    {
        ExpressionSyntax value = initializer.Value;
        string type = initializer.DeclaredType.ToString();
        if (value is ImplicitObjectCreationExpressionSyntax creation)
            return $"new {type}{creation.ArgumentList}" + (creation.Initializer is null ? "" : " " + creation.Initializer);
        if (value is CollectionExpressionSyntax)
            return $"({type}){value}";
        if (initializer.Model.GetTypeInfo(value).Type is null || initializer.Model.GetConversion(value).IsUserDefined)
            return $"({type})({value})";
        return value.ToString();
    }

    /// <summary>Inserts statements at the start of a constructor body.</summary>
    private static void Insert(SourceEdits edits, BlockSyntax body, List<string> statements)
    {
        SourceText text = edits.Text;
        SyntaxToken open = body.OpenBraceToken;
        TextLine braceLine = text.Lines.GetLineFromPosition(open.SpanStart);
        SyntaxToken next = open.GetNextToken();
        if (next.SpanStart < braceLine.EndIncludingLineBreak)
        {
            edits.Insert(open.Span.End, string.Concat(statements.Select(statement => " " + statement)));
            return;
        }
        string newline = text.ToString(TextSpan.FromBounds(braceLine.End, braceLine.EndIncludingLineBreak));
        string indent = body.Statements.Count > 0
            ? Indentation(text.Lines.GetLineFromPosition(body.Statements[0].SpanStart).ToString())
            : Indentation(braceLine.ToString()) + "    ";
        edits.Insert(braceLine.EndIncludingLineBreak, string.Concat(statements.Select(statement => indent + statement + newline)));
    }

    /// <summary>The white space a line starts with.</summary>
    private static string Indentation(string line) => line.Substring(0, line.Length - line.TrimStart().Length);
}
