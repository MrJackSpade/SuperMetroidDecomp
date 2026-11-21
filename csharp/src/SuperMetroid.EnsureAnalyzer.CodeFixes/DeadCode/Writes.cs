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
/// deconstruction target become <c>_</c>. An assignment whose value is used with the variable's type, not its own, is
/// replaced by the value converted to that type. Any other use (one that reads the variable, an implicit one, one in
/// documentation or with no operation, a write through a receiver whose evaluation has effects) is left as written
/// (#2599): the variable is removed all the same, and the build fails at that use. A parameter only a removed statement
/// read is left to <see cref="UnreadParameters"/>.
/// </summary>
internal static class Writes
{
    /// <summary>Removes the writes at <paramref name="references"/>.</summary>
    internal static async Task RemoveAsync(SolutionEdits edits, List<ReferenceLocation> references, UnreadParameters unread,
        CancellationToken cancellationToken)
    {
        foreach (ReferenceLocation reference in references)
        {
            if (reference.IsImplicit || reference.Location.SourceTree is not { } tree)
                continue;
            SyntaxNode root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            SemanticModel model = await reference.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"{tree.FilePath} has no semantic model.");
            SyntaxNode name = root.FindNode(reference.Location.SourceSpan, getInnermostNodeForTie: true);
            if ((name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name) is not ExpressionSyntax expression ||
                model.GetOperation(expression, cancellationToken) is not { } operation ||
                operation is IMemberReferenceOperation { Instance: { IsImplicit: false } instance } && !SideEffects.IsFree(instance))
                continue;
            Remove(await edits.ForAsync(tree, cancellationToken).ConfigureAwait(false), operation, expression, unread, model);
        }
    }

    /// <summary>Removes one write, the use <paramref name="operation"/> of <paramref name="expression"/>.</summary>
    private static void Remove(SourceEdits edits, IOperation operation, ExpressionSyntax expression, UnreadParameters unread, SemanticModel model)
    {
        switch (operation.Parent)
        {
            case ISimpleAssignmentOperation assignment when assignment.Target.Syntax == expression:
                if (assignment.Parent is IExpressionStatementOperation statement)
                {
                    if (statement.Syntax is ExpressionStatementSyntax written && SideEffects.IsFree(assignment.Value))
                    {
                        RemoveStatement(edits, written);
                        unread.Removing(written, model);
                    }
                    else
                        edits.Replace(expression.Span, "_");
                    return;
                }
                // The value's own type, before the implicit conversion to the variable's that the assignment makes.
                ITypeSymbol? valueType = (assignment.Value is IConversionOperation { IsImplicit: true } conversion ? conversion.Operand : assignment.Value).Type;
                edits.Replace(assignment.Syntax.Span, assignment.Type is null || SymbolEqualityComparer.Default.Equals(valueType, assignment.Type)
                    ? assignment.Value.Syntax.ToString()
                    : $"({assignment.Type.ToMinimalDisplayString(model, assignment.Syntax.SpanStart)})({assignment.Value.Syntax})");
                return;
            case ICompoundAssignmentOperation compound when compound.Target.Syntax == expression &&
                compound.Parent is IExpressionStatementOperation { Syntax: ExpressionStatementSyntax line }:
                if (SideEffects.IsFree(compound.Value))
                {
                    RemoveStatement(edits, line);
                    unread.Removing(line, model);
                }
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
                // A use that reads the variable stays as written: the variable goes, and the build fails here.
                return;
        }
    }

    /// <summary>Removes a statement from its block, or leaves an empty block where the syntax needs a statement.</summary>
    internal static void RemoveStatement(SourceEdits edits, ExpressionStatementSyntax statement)
    {
        if (statement.Parent is GlobalStatementSyntax global)
            edits.Remove(global);
        else if (statement.Parent is BlockSyntax or SwitchSectionSyntax)
            edits.Remove(statement);
        else
            edits.Replace(statement.Span, "{ }");
    }
}
