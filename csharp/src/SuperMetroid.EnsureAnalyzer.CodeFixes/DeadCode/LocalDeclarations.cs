using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>Removes variables from local declarations: the whole statement when none is left, or the removed
/// declarators from the declaration's list.</summary>
internal static class LocalDeclarations
{
    /// <summary>Removes <paramref name="removed"/>, declarators of local or <c>for</c> declarations, from their
    /// declarations; <paramref name="diagnostic"/> names the site of a declaration this cannot edit.</summary>
    internal static void Remove(SourceEdits edits, IEnumerable<VariableDeclaratorSyntax> removed, Func<SyntaxNode, Diagnostic> diagnostic)
    {
        foreach (var group in removed.GroupBy(declarator => declarator.Parent))
        {
            if (group.Key is not VariableDeclarationSyntax declaration)
                throw new CleanupFailure(diagnostic(group.First()), "the variable is not declared in a variable declaration");
            var gone = new HashSet<VariableDeclaratorSyntax>(group);
            var kept = declaration.Variables.Where(variable => !gone.Contains(variable)).ToList();
            if (kept.Count > 0)
            {
                var list = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(declaration.Variables[0].SpanStart, declaration.Variables[declaration.Variables.Count - 1].Span.End);
                edits.Replace(list, string.Join(", ", kept.Select(variable => variable.ToString())));
                continue;
            }
            switch (declaration.Parent)
            {
                case LocalDeclarationStatementSyntax statement:
                    edits.Remove(statement);
                    break;
                case ForStatementSyntax:
                    edits.Replace(declaration.Span, string.Empty);
                    break;
                default:
                    throw new CleanupFailure(diagnostic(group.First()), $"the variable is declared in a {declaration.Parent?.Kind()}, whose declaration cannot be removed");
            }
        }
    }
}
