using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// Removes every write to a variable whose value is never read (#1657, #1659, #1660), keeping each written value's
/// evaluation where it could have an effect. An assignment statement whose value is free of effects goes; one whose
/// value is not becomes <c>_ = value;</c>; an assignment used as a value is replaced by the value; an increment or a
/// compound assignment statement goes, or keeps its right-hand side as <c>_ = value;</c>; an <c>out</c> argument and a
/// deconstruction target become <c>_</c>. Any other use reads the variable, and the fix fails naming it.
/// </summary>
internal static class Writes
{
    /// <summary>Removes the writes at <paramref name="references"/> to <paramref name="symbol"/>.</summary>
    internal static async Task RemoveAsync(SolutionEdits edits, ISymbol symbol, IEnumerable<ReferenceLocation> references, Diagnostic site,
        CancellationToken cancellationToken)
    {
        foreach (ReferenceLocation reference in references)
        {
            if (reference.IsImplicit || reference.Location.SourceTree is not { } tree)
                throw new CleanupFailure(reference.Location, site.Id, $"'{symbol.Name}' is used implicitly");
            SyntaxNode root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            SemanticModel model = await reference.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"{tree.FilePath} has no semantic model.");
            SyntaxNode name = root.FindNode(reference.Location.SourceSpan, getInnermostNodeForTie: true);
            var expression = (ExpressionSyntax)(name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name);
            IOperation operation = model.GetOperation(expression, cancellationToken)
                ?? throw new CleanupFailure(reference.Location, site.Id, $"'{symbol.Name}' is used where it has no operation");
            if (operation is IMemberReferenceOperation { Instance: { IsImplicit: false } instance } && !SideEffects.IsFree(instance))
                throw new CleanupFailure(reference.Location, site.Id, $"'{symbol.Name}' is written through a receiver whose evaluation has effects");
            Remove(await edits.ForAsync(tree, cancellationToken).ConfigureAwait(false), operation, expression, symbol, reference.Location, site);
        }
    }

    /// <summary>Removes one write, the use <paramref name="operation"/> of <paramref name="expression"/>.</summary>
    private static void Remove(SourceEdits edits, IOperation operation, ExpressionSyntax expression, ISymbol symbol, Location location, Diagnostic site)
    {
        switch (operation.Parent)
        {
            case ISimpleAssignmentOperation assignment when assignment.Target.Syntax == expression:
                if (assignment.Parent is IExpressionStatementOperation statement)
                {
                    if (statement.Syntax is ExpressionStatementSyntax written && SideEffects.IsFree(assignment.Value))
                        RemoveStatement(edits, written);
                    else
                        edits.Replace(expression.Span, "_");
                    return;
                }
                if (!SymbolEqualityComparer.Default.Equals(assignment.Value.Type, assignment.Type))
                    throw new CleanupFailure(location, site.Id, $"the value of an assignment to '{symbol.Name}' is used with its converted type");
                edits.Replace(assignment.Syntax.Span, assignment.Value.Syntax.ToString());
                return;
            case ICompoundAssignmentOperation compound when compound.Target.Syntax == expression &&
                compound.Parent is IExpressionStatementOperation { Syntax: ExpressionStatementSyntax line }:
                if (SideEffects.IsFree(compound.Value))
                    RemoveStatement(edits, line);
                else
                    edits.Replace(compound.Syntax.Span, "_ = " + compound.Value.Syntax);
                return;
            case IIncrementOrDecrementOperation step when step.Target.Syntax == expression &&
                step.Parent is IExpressionStatementOperation { Syntax: ExpressionStatementSyntax line }:
                RemoveStatement(edits, line);
                return;
            case IArgumentOperation { Parameter.RefKind: RefKind.Out }:
                edits.Replace(expression.Span, "_");
                return;
            case ITupleOperation { Parent: IDeconstructionAssignmentOperation deconstruction } tuple when deconstruction.Target == tuple:
                edits.Replace(expression.Span, "_");
                return;
            default:
                throw new CleanupFailure(location, site.Id, $"'{symbol.Name}' is used as {operation.Parent?.Kind.ToString() ?? "a value"}, which reads it");
        }
    }

    /// <summary>Removes a statement from its block, or leaves an empty block where the syntax needs a statement.</summary>
    internal static void RemoveStatement(SourceEdits edits, StatementSyntax statement)
    {
        if (statement.Parent is GlobalStatementSyntax global)
            edits.Remove(global);
        else if (statement.Parent is BlockSyntax or SwitchSectionSyntax)
            edits.Remove(statement);
        else
            edits.Replace(statement.Span, "{ }");
    }
}
