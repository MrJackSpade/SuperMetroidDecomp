using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// CS0162, unreachable code, enforced by the asynchronous cleanup (#1658). The compiler has proven the statements never
/// run, so they go: the reported statement and every following statement of its block or switch section whose start
/// is unreachable (a local function, which never runs in place, stays). A nested block left empty goes too, and a
/// switch section left empty goes with its labels; a body the syntax needs keeps its braces, and an unreachable embedded
/// statement becomes <c>{ }</c>. A label that only the removed code jumped to loses its label.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnreachableCodeCodeFixProvider)), Shared]
public sealed class UnreachableCodeCodeFixProvider : DocumentFixProvider
{
    /// <summary>The compiler's unreachable-code warning.</summary>
    public const string Id = "CS0162";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [Id];

    /// <inheritdoc />
    internal override string Title => "Remove the unreachable code";

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no syntax tree.");
        SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no semantic model.");
        var edits = new SourceEdits(await document.GetTextAsync(cancellationToken).ConfigureAwait(false), document.FilePath ?? document.Name);
        var removed = new HashSet<StatementSyntax>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            StatementSyntax first = StatementAt(root, diagnostic.Location.SourceSpan.Start)
                ?? throw new CleanupFailure(diagnostic, "no statement starts at the finding");
            switch (first.Parent)
            {
                case BlockSyntax block:
                    Collect(block.Statements, first, model, removed, diagnostic);
                    break;
                case SwitchSectionSyntax section:
                    Collect(section.Statements, first, model, removed, diagnostic);
                    break;
                case GlobalStatementSyntax:
                    throw new CleanupFailure(diagnostic, "the unreachable code is a top-level statement");
                default:
                    edits.Replace(first.Span, "{ }");
                    removed.Add(first);
                    break;
            }
        }
        foreach (var group in removed
            .SelectMany(statement => statement.Parent is (BlockSyntax or SwitchSectionSyntax) and var parent
                ? new[] { (Statement: statement, Parent: parent) } : Array.Empty<(StatementSyntax Statement, SyntaxNode Parent)>())
            .GroupBy(entry => entry.Parent, entry => entry.Statement))
        {
            int count = group.Count();
            switch (group.Key)
            {
                case BlockSyntax { Parent: BlockSyntax or SwitchSectionSyntax } block when count == block.Statements.Count:
                    edits.Remove(block);
                    break;
                case SwitchSectionSyntax section when count == section.Statements.Count:
                    edits.Remove(section);
                    break;
                default:
                    foreach (StatementSyntax statement in group)
                        edits.Remove(statement);
                    break;
            }
        }
        RemoveOrphanedLabels(root, removed, edits);
        return document.WithText(edits.Apply()).Project.Solution;
    }

    /// <summary>The outermost statement that starts at <paramref name="position"/>.</summary>
    private static StatementSyntax? StatementAt(SyntaxNode root, int position) =>
        root.FindToken(position).Parent?.AncestorsAndSelf().OfType<StatementSyntax>().LastOrDefault(statement => statement.SpanStart == position);

    /// <summary>Adds <paramref name="first"/> and every following statement of the list whose start is unreachable.</summary>
    private static void Collect(SyntaxList<StatementSyntax> statements, StatementSyntax first, SemanticModel model, HashSet<StatementSyntax> removed,
        Diagnostic diagnostic)
    {
        int before = removed.Count;
        for (int index = statements.IndexOf(first); index < statements.Count; index++)
        {
            StatementSyntax statement = statements[index];
            if (statement is LocalFunctionStatementSyntax)
                continue;
            ControlFlowAnalysis? flow = model.AnalyzeControlFlow(statement);
            if (flow is null || !flow.Succeeded)
                throw new CleanupFailure(diagnostic, "the reachability of the statements after it cannot be analyzed");
            if (flow.StartPointIsReachable)
                break;
            removed.Add(statement);
        }
        if (removed.Count == before)
            throw new CleanupFailure(diagnostic, "the reported statement is reachable");
    }

    /// <summary>Unlabels each statement whose label only the removed code jumped to.</summary>
    private static void RemoveOrphanedLabels(SyntaxNode root, HashSet<StatementSyntax> removed, SourceEdits edits)
    {
        var spans = removed.Select(statement => statement.Span).ToList();
        bool Removed(SyntaxNode node) => spans.Any(span => span.Contains(node.Span));
        var jumps = root.DescendantNodes().OfType<GotoStatementSyntax>()
            .Where(jump => jump.IsKind(SyntaxKind.GotoStatement))
            .SelectMany(jump => jump.Expression is { } target
                ? new[] { (Jump: jump, Label: target.ToString()) } : Array.Empty<(GotoStatementSyntax Jump, string Label)>())
            .ToList();
        var orphaned = new HashSet<string>(jumps.Where(entry => Removed(entry.Jump)).Select(entry => entry.Label));
        foreach (string label in jumps.Where(entry => !Removed(entry.Jump)).Select(entry => entry.Label))
            orphaned.Remove(label);
        foreach (LabeledStatementSyntax labeled in root.DescendantNodes().OfType<LabeledStatementSyntax>())
        {
            if (!orphaned.Contains(labeled.Identifier.ValueText) || Removed(labeled))
                continue;
            int labelLine = edits.Text.Lines.GetLineFromPosition(labeled.SpanStart).LineNumber;
            int statementLine = edits.Text.Lines.GetLineFromPosition(labeled.Statement.SpanStart).LineNumber;
            edits.Replace(labelLine == statementLine
                ? TextSpan.FromBounds(labeled.SpanStart, labeled.Statement.SpanStart)
                : TextSpan.FromBounds(edits.Text.Lines[labelLine].Start, edits.Text.Lines[statementLine].Start), string.Empty);
        }
    }
}
