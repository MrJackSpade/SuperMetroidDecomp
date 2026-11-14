using Microsoft.CodeAnalysis;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// Declarations the compiler requires beside another declaration: a paired operator needs its partner (CS0216), a
/// checked operator its unchecked form, and an override the member it overrides (CS0115). Each is an edge from the
/// declaration to what it requires, so reaching it, from production or from the dead code checks still use, reaches what
/// it requires: a pair is kept when either of it is used and removed only together, and nothing a kept declaration needs
/// is ever deleted. The rule belongs to the declarations, not to the sites that use them, since the code that keeps a
/// declaration alive need not be code the analysis walks: on 2026-11-13 only verification used
/// <c>PowerBombRadius.operator &gt;=</c>, so nothing tied the unused <c>operator &lt;=</c> to it and the prune deleted it.
/// </summary>
/// <remarks>
/// An <c>Equals</c> override and its <c>GetHashCode</c> need no edge: both override framework members, which the runtime
/// calls whenever their type exists (<see cref="RootCollector"/>), so they stay exactly as long as their type. An interface
/// member needs none either: an explicit implementation is reached only through the member it implements, and an implicit
/// one does not need it.
/// </remarks>
internal sealed class RequiredDeclarations(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Prefix of a checked operator's metadata name; the unchecked form it requires drops <c>Checked</c>.</summary>
    private const string CheckedOperatorPrefix = "op_Checked";

    /// <summary>The prefix every operator's metadata name keeps when its checked form becomes the unchecked one.</summary>
    private const string OperatorPrefix = "op_";

    /// <summary>The operator C# requires to be declared alongside each paired operator (CS0216).</summary>
    private static readonly Dictionary<string, string> OperatorPartners = new(StringComparer.Ordinal)
    {
        [WellKnownMemberNames.EqualityOperatorName] = WellKnownMemberNames.InequalityOperatorName,
        [WellKnownMemberNames.InequalityOperatorName] = WellKnownMemberNames.EqualityOperatorName,
        [WellKnownMemberNames.LessThanOperatorName] = WellKnownMemberNames.GreaterThanOperatorName,
        [WellKnownMemberNames.GreaterThanOperatorName] = WellKnownMemberNames.LessThanOperatorName,
        [WellKnownMemberNames.LessThanOrEqualOperatorName] = WellKnownMemberNames.GreaterThanOrEqualOperatorName,
        [WellKnownMemberNames.GreaterThanOrEqualOperatorName] = WellKnownMemberNames.LessThanOrEqualOperatorName,
        [WellKnownMemberNames.TrueOperatorName] = WellKnownMemberNames.FalseOperatorName,
        [WellKnownMemberNames.FalseOperatorName] = WellKnownMemberNames.TrueOperatorName,
    };

    /// <summary>Adds an edge from every member of the compilation's types to each declaration it requires.</summary>
    public void Collect(Compilation compilation)
    {
        foreach (var type in RootCollector.AllTypes(compilation.Assembly.GlobalNamespace))
            foreach (var member in type.GetMembers())
                foreach (var required in Required(member))
                    graph.Edge(identity.Key(member), identity.Key(required));
    }

    /// <summary>The declarations <paramref name="member"/> cannot compile without; framework ones have no key and add nothing.</summary>
    private static IEnumerable<ISymbol> Required(ISymbol member) => member switch
    {
        IMethodSymbol { MethodKind: MethodKind.UserDefinedOperator or MethodKind.Conversion } @operator => OperatorsRequiredBy(@operator),
        IMethodSymbol { OverriddenMethod: { } overridden } => [overridden],
        IPropertySymbol { OverriddenProperty: { } overridden } => [overridden],
        IEventSymbol { OverriddenEvent: { } overridden } => [overridden],
        _ => [],
    };

    /// <summary>
    /// The operators of <paramref name="operator"/>'s type with the return and parameter types it has and the name the
    /// language requires beside it: its pair's partner, or, for a checked operator, its unchecked form.
    /// </summary>
    private static IEnumerable<IMethodSymbol> OperatorsRequiredBy(IMethodSymbol @operator)
    {
        string? name = OperatorPartners.TryGetValue(@operator.Name, out string? partner) ? partner
            : @operator.Name.StartsWith(CheckedOperatorPrefix, StringComparison.Ordinal)
                ? OperatorPrefix + @operator.Name[CheckedOperatorPrefix.Length..]
                : null;
        if (name is null)
            return [];
        return @operator.ContainingType.GetMembers(name).OfType<IMethodSymbol>().Where(candidate =>
            SymbolEqualityComparer.Default.Equals(candidate.ReturnType, @operator.ReturnType) &&
            candidate.Parameters.Select(parameter => parameter.Type)
                .SequenceEqual(@operator.Parameters.Select(parameter => parameter.Type), SymbolEqualityComparer.Default));
    }
}
