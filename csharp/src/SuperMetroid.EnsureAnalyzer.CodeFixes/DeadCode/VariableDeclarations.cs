using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>Removes variables from their declarations (locals, <c>for</c> initializers, <c>using</c> statements, fields
/// and field-like events): the whole declaration when none is left, or the removed declarators from the declaration's
/// list. A <c>using</c> statement left without its variable keeps disposing the value it was given.</summary>
internal static class VariableDeclarations
{
    /// <summary>Removes <paramref name="removed"/> from their declarations.</summary>
    internal static void Remove(SourceEdits edits, List<VariableDeclaratorSyntax> removed)
    {
        foreach (var group in removed.GroupBy(declarator => declarator.Parent))
        {
            if (group.Key is not VariableDeclarationSyntax declaration)
                throw new InvalidOperationException($"{edits.FilePath}({edits.Text.Lines.GetLinePosition(group.First().SpanStart)}): a variable declarator outside a variable declaration.");
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
                case UsingStatementSyntax when declaration.Variables.Count == 1 && declaration.Variables[0].Initializer is { } initializer:
                    edits.Replace(declaration.Span, initializer.Value.ToString());
                    break;
                default:
                    throw new InvalidOperationException(
                        $"{edits.FilePath}({edits.Text.Lines.GetLinePosition(declaration.SpanStart)}): a variable declared by a {declaration.Parent?.Kind()}, which this fix does not know.");
            }
        }
    }
}
