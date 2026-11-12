using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>Removes variables from their declarations (locals, <c>for</c> initializers, fields and field-like events):
/// the whole declaration when none is left, or the removed declarators from the declaration's list.</summary>
internal static class VariableDeclarations
{
    /// <summary>Removes <paramref name="removed"/> from their declarations; <paramref name="diagnostic"/> names the site
    /// of a declaration this cannot edit.</summary>
    internal static void Remove(SourceEdits edits, List<VariableDeclaratorSyntax> removed, Func<SyntaxNode, Diagnostic> diagnostic)
    {
        foreach (var group in removed.GroupBy(declarator => declarator.Parent))
        {
            if (group.Key is not VariableDeclarationSyntax declaration)
                throw new CleanupFailure(diagnostic(group.First()), "the variable is not declared in a variable declaration");
            var gone = new HashSet<VariableDeclaratorSyntax>(group);
            var kept = declaration.Variables.Where(variable => !gone.Contains(variable)).ToList();
            if (kept.Count > 0)
            {
                var list = TextSpan.FromBounds(declaration.Variables[0].SpanStart, declaration.Variables[declaration.Variables.Count - 1].Span.End);
                edits.Replace(list, string.Join(", ", kept.Select(variable => variable.ToString())));
                continue;
            }
            switch (declaration.Parent)
            {
                case LocalDeclarationStatementSyntax statement:
                    edits.Remove(statement);
                    break;
                case BaseFieldDeclarationSyntax field:
                    edits.Remove(field);
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
