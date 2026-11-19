using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Whether a sequence feeds an order (SME6469, #2567): at once, not at all, or only if one of the parameters, members or
/// method results of the same assembly it is handed to feeds one with what it is given.
/// </summary>
internal sealed class OrderVerdict
{
    /// <summary>Holds the verdict.</summary>
    private OrderVerdict(bool feedsAtOnce, ImmutableHashSet<ISymbol> conditions)
    {
        FeedsAtOnce = feedsAtOnce;
        Conditions = conditions;
    }

    /// <summary>The sequence feeds an order where it is used.</summary>
    internal static OrderVerdict Feeds { get; } = new(true, ImmutableHashSet.Create<ISymbol>(SymbolEqualityComparer.Default));

    /// <summary>The sequence feeds no order.</summary>
    internal static OrderVerdict Free { get; } = new(false, ImmutableHashSet.Create<ISymbol>(SymbolEqualityComparer.Default));

    /// <summary>Whether the sequence feeds an order where it is used, whatever the symbols it is handed to do.</summary>
    internal bool FeedsAtOnce { get; }

    /// <summary>The parameters, members and methods whose uses decide whether the sequence feeds an order.</summary>
    internal ImmutableHashSet<ISymbol> Conditions { get; }

    /// <summary>The sequence feeds an order only if <paramref name="symbol"/>'s uses do.</summary>
    internal static OrderVerdict DependsOn(ISymbol symbol) => new(false, Free.Conditions.Add(symbol));

    /// <summary>The verdict of a sequence used both as this verdict's and as <paramref name="other"/>'s.</summary>
    internal OrderVerdict With(OrderVerdict other) => FeedsAtOnce || other.FeedsAtOnce
        ? Feeds
        : new OrderVerdict(false, Conditions.Union(other.Conditions));
}
