using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// One place Core calls a method or constructor (#1784): its arguments, the semantic model they bind in, and the unit
/// whose code makes the call, through which a walk tells whether the call is among the code it reaches.
/// </summary>
/// <param name="Arguments">The call's arguments.</param>
/// <param name="Semantic">The semantic model of the call's tree.</param>
/// <param name="Caller">The unit whose code makes the call (a lambda's call is its enclosing member's), or null when none is one.</param>
internal sealed record CallSite(ArgumentListSyntax Arguments, SemanticModel Semantic, Unit? Caller)
{
    /// <summary>
    /// The argument the call passes for <paramref name="parameter"/>: the one naming it, or the positional one at its
    /// place; null when the call leaves it to its default.
    /// </summary>
    public ExpressionSyntax? ArgumentFor(IParameterSymbol parameter)
    {
        if (Arguments.Arguments.FirstOrDefault(argument => argument.NameColon?.Name.Identifier.Text == parameter.Name) is { } named)
            return named.Expression;
        return parameter.Ordinal < Arguments.Arguments.Count && Arguments.Arguments[parameter.Ordinal].NameColon is null
            ? Arguments.Arguments[parameter.Ordinal].Expression
            : null;
    }
}
