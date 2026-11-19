using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// The name helpers of one compilation (SME6460, SME6462, #2544): the enum names each unmarked method, local function or
/// property makes its result, the keys its callers hand that result to, and the helpers whose results it returns. At the
/// compilation's end each name a helper returns is reported against the key its result reaches, through any chain of
/// helpers returning one another's results.
/// </summary>
internal sealed class NameHelperFlows
{
    /// <summary>The enum names each helper makes its result.</summary>
    private readonly ConcurrentDictionary<ISymbol, ConcurrentBag<EnumNameWrite>> returned = new(SymbolEqualityComparer.Default);

    /// <summary>The key a call of each helper hands its result to, the first one found.</summary>
    private readonly ConcurrentDictionary<ISymbol, KeyDestination> fed = new(SymbolEqualityComparer.Default);

    /// <summary>The helpers whose results each helper returns as its own.</summary>
    private readonly ConcurrentDictionary<ISymbol, ConcurrentBag<ISymbol>> inner = new(SymbolEqualityComparer.Default);

    /// <summary>Records that <paramref name="helper"/> makes <paramref name="write"/>'s enum name its result.</summary>
    internal void Returns(ISymbol helper, EnumNameWrite write) => returned.GetOrAdd(helper, _ => []).Add(write);

    /// <summary>Records that a call of <paramref name="helper"/> hands its result to <paramref name="key"/>.</summary>
    internal void Feeds(ISymbol helper, KeyDestination key) => fed.TryAdd(helper, key);

    /// <summary>Records that <paramref name="outer"/> returns the result of a call of <paramref name="helper"/> as its own.</summary>
    internal void ReturnsResultOf(ISymbol outer, ISymbol helper) => inner.GetOrAdd(outer, _ => []).Add(helper);

    /// <summary>
    /// Hands <paramref name="report"/> each enum name a helper returns whose result reaches a key, with that key, once every
    /// block of the compilation has been read.
    /// </summary>
    internal void Report(Action<(EnumNameWrite Write, KeyDestination Key)> report)
    {
        var reached = new Dictionary<ISymbol, KeyDestination>(fed, SymbolEqualityComparer.Default);
        var pending = new Queue<ISymbol>(reached.Keys);
        while (pending.Count > 0)
        {
            ISymbol outer = pending.Dequeue();
            if (!inner.TryGetValue(outer, out ConcurrentBag<ISymbol>? helpers))
                continue;
            foreach (ISymbol helper in helpers)
            {
                if (reached.ContainsKey(helper))
                    continue;
                reached.Add(helper, reached[outer]);
                pending.Enqueue(helper);
            }
        }
        foreach (KeyValuePair<ISymbol, ConcurrentBag<EnumNameWrite>> entry in returned)
        {
            if (!reached.TryGetValue(entry.Key, out KeyDestination key))
                continue;
            foreach (EnumNameWrite write in entry.Value.OrderBy(write => write.Text.Syntax.SpanStart))
                report((write, key));
        }
    }
}
