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

    /// <summary>The map cells explored so far, each as its area and its column and row on that area's map.</summary>
    private readonly HashSet<(string Area, int Column, int Row)> _explored = [];

    /// <summary>How many updates of play have been recorded.</summary>
    public long PlayUpdates { get; private set; }

    /// <summary>Every map cell explored so far, for saving and for drawing the map.</summary>
    public IReadOnlyCollection<(string Area, int Column, int Row)> Explored => _explored;

    /// <summary>Records that a map cell has been explored.</summary>
    public void MarkExplored(string area, int column, int row) => _explored.Add((RequireName(area), column, row));

    /// <summary>Whether a map cell has been explored.</summary>
    public bool IsExplored(string area, int column, int row) => _explored.Contains((area, column, row));

    /// <summary>
    /// Counts one more update of play, unless <paramref name="most"/> have been counted already, when the play time
    /// stays where it is; no limit when <paramref name="most"/> is nothing.
    /// </summary>
    public void CountPlayUpdate(long most)
    {
        if (most <= 0 || PlayUpdates < most)
            PlayUpdates++;
    }

    /// <summary>Sets how many updates of play have been recorded, as loading a save does.</summary>
    public void RestorePlayUpdates(long updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        PlayUpdates = updates;
    }

    /// <summary>Every fact recorded so far, for saving.</summary>
    public IReadOnlyCollection<string> Facts => _facts;

    /// <summary>Whether the named fact has been recorded.</summary>
    public bool Has(string fact) => _facts.Contains(RequireName(fact));

    /// <summary>Records the named fact. Recording a fact already recorded changes nothing.</summary>
    public void Record(string fact) => _facts.Add(RequireName(fact));

    /// <summary>Takes the named fact back, as though it had never been recorded. Taking back one that is not recorded changes nothing.</summary>
    public void Forget(string fact) => _facts.Remove(RequireName(fact));

    /// <summary>The name, which must say something.</summary>
    private static string RequireName(string fact)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fact);
        return fact;
    }
}
