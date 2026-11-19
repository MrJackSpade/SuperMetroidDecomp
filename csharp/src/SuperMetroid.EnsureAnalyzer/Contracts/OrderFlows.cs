using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// The flows of one compilation SME6469 follows across members (#2567): how every read of each parameter, member and
/// method result of the assembly uses the sequence it holds, and the findings that feed an order only if one of those
/// does. At the compilation's end each such finding is reported when a symbol it is handed to feeds an order with it.
/// </summary>
internal sealed class OrderFlows
{
    /// <summary>The verdicts of every read of each parameter, member and method result.</summary>
    private readonly ConcurrentDictionary<ISymbol, ConcurrentBag<OrderVerdict>> uses = new(SymbolEqualityComparer.Default);

    /// <summary>The parameters whose function's body was read, so every read of them is known.</summary>
    private readonly ConcurrentDictionary<ISymbol, byte> bodies = new(SymbolEqualityComparer.Default);

    /// <summary>The findings and the verdicts that decide whether each is reported.</summary>
    private readonly ConcurrentBag<KeyValuePair<Diagnostic, OrderVerdict>> pending = [];

    /// <summary>Records one read of <paramref name="symbol"/> and what it does with the sequence.</summary>
    internal void Use(ISymbol symbol, OrderVerdict verdict) => uses.GetOrAdd(symbol, _ => []).Add(verdict);

    /// <summary>Records that every read of <paramref name="parameter"/> is known.</summary>
    internal void Body(IParameterSymbol parameter) => bodies.TryAdd(parameter, 0);

    /// <summary>Records <paramref name="finding"/>, reported if <paramref name="verdict"/> feeds an order.</summary>
    internal void Pending(Diagnostic finding, OrderVerdict verdict) => pending.Add(new KeyValuePair<Diagnostic, OrderVerdict>(finding, verdict));

    /// <summary>Reports every finding whose verdict feeds an order.</summary>
    internal void Report(Action<Diagnostic> report)
    {
        var resolved = new Dictionary<ISymbol, bool>(SymbolEqualityComparer.Default);
        foreach (KeyValuePair<Diagnostic, OrderVerdict> finding in pending)
        {
            if (finding.Value.FeedsAtOnce || finding.Value.Conditions.Any(symbol => Feeds(symbol, resolved, new HashSet<ISymbol>(SymbolEqualityComparer.Default))))
                report(finding.Key);
        }
    }

    /// <summary>
    /// Whether a sequence handed to <paramref name="symbol"/> feeds an order: a parameter whose function is unread, or a
    /// member or method nothing in the assembly reads, is taken to; otherwise any read that does, directly or through the
    /// symbols it hands the sequence on to. A symbol already being followed adds nothing.
    /// </summary>
    private bool Feeds(ISymbol symbol, Dictionary<ISymbol, bool> resolved, HashSet<ISymbol> visiting)
    {
        if (resolved.TryGetValue(symbol, out bool known))
            return known;
        if (!visiting.Add(symbol))
            return false;
        bool feeds;
        if (symbol is IParameterSymbol)
        {
            feeds = !bodies.ContainsKey(symbol) || ReadsFeed(uses.TryGetValue(symbol, out ConcurrentBag<OrderVerdict>? reads) ? reads : [], resolved, visiting);
        }
        else
        {
            OrderVerdict[] reads = [.. Related(symbol).SelectMany(related => uses.TryGetValue(related, out ConcurrentBag<OrderVerdict>? found) ? found : [])];
            feeds = reads.Length == 0 || ReadsFeed(reads, resolved, visiting);
        }
        visiting.Remove(symbol);
        resolved[symbol] = feeds;
        return feeds;
    }

    /// <summary>Whether any of <paramref name="reads"/> feeds an order.</summary>
    private bool ReadsFeed(IEnumerable<OrderVerdict> reads, Dictionary<ISymbol, bool> resolved, HashSet<ISymbol> visiting) =>
        reads.Any(read => read.FeedsAtOnce || read.Conditions.Any(condition => Feeds(condition, resolved, visiting)));

    /// <summary>
    /// <paramref name="symbol"/> and the members a read of it may be made through: the interface members it implements and
    /// the members it overrides.
    /// </summary>
    private static IEnumerable<ISymbol> Related(ISymbol symbol)
    {
        yield return symbol;
        INamedTypeSymbol? owner = symbol.ContainingType;
        if (owner is not null)
        {
            foreach (INamedTypeSymbol face in owner.AllInterfaces)
            {
                foreach (ISymbol member in face.GetMembers(symbol.Name))
                {
                    if (SymbolEqualityComparer.Default.Equals(owner.FindImplementationForInterfaceMember(member)?.OriginalDefinition, symbol))
                        yield return member.OriginalDefinition;
                }
            }
        }
        ISymbol? overridden = symbol switch
        {
            IMethodSymbol method => method.OverriddenMethod,
            IPropertySymbol property => property.OverriddenProperty,
            _ => null,
        };
        for (; overridden is not null; overridden = overridden switch
        {
            IMethodSymbol method => method.OverriddenMethod,
            IPropertySymbol property => property.OverriddenProperty,
            _ => null,
        })
            yield return overridden.OriginalDefinition;
    }
}
