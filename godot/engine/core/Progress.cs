namespace Engine.Core;

/// <summary>
/// What has happened in a game that must not be forgotten: each a named fact. A room forgets everything when it is
/// left; an item collected, a boss defeated or a barrier opened for good survives only as a fact here. It answers and
/// records "has this happened" and decides nothing about what follows from it.
/// </summary>
public sealed class Progress
{
    /// <summary>The facts recorded so far.</summary>
    private readonly HashSet<string> _facts = new(StringComparer.Ordinal);

    /// <summary>Every fact recorded so far, for saving.</summary>
    public IReadOnlyCollection<string> Facts => _facts;

    /// <summary>Whether the named fact has been recorded.</summary>
    public bool Has(string fact) => _facts.Contains(RequireName(fact));

    /// <summary>Records the named fact. Recording a fact already recorded changes nothing.</summary>
    public void Record(string fact) => _facts.Add(RequireName(fact));

    /// <summary>The name, which must say something.</summary>
    private static string RequireName(string fact)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fact);
        return fact;
    }
}
