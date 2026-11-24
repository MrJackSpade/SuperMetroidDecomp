using Engine.Core;

namespace Game.Menus;

/// <summary>
/// Which of the title screen's demos is shown next. The demos come in sets: left alone, the title shows the demos
/// of one set one after another and comes back, and the next time it is left alone it shows the next set.
/// </summary>
public sealed class DemoSchedule : ISessionService
{
    private IReadOnlyList<IReadOnlyList<string>> _sets = [];
    private int _set;
    private int _begun;

    /// <summary>The first demo of the set that is due, as the title is left alone. The sets are given afresh each time.</summary>
    /// <exception cref="ArgumentException">There are no sets, or the set that is due has no demos.</exception>
    public string FromTitle(IReadOnlyList<IReadOnlyList<string>> sets)
    {
        ArgumentNullException.ThrowIfNull(sets);
        if (sets.Count == 0)
            throw new ArgumentException("The title has no sets of demos to show.", nameof(sets));
        _sets = sets;
        _set %= sets.Count;
        if (sets[_set].Count == 0)
            throw new ArgumentException("A set of the title's demos has no demos in it.", nameof(sets));
        _begun = 1;
        return sets[_set][0];
    }

    /// <summary>
    /// The demo that follows one that played out, or null when that was the last of its set: the title comes back,
    /// and the next set is the one due.
    /// </summary>
    public string? AfterDemo()
    {
        if (_sets.Count == 0)
            throw new InvalidOperationException("No demo has been shown for another to follow.");
        if (_begun < _sets[_set].Count)
            return _sets[_set][_begun++];
        _set = (_set + 1) % _sets.Count;
        _begun = 0;
        return null;
    }

    /// <summary>As the game starts or starts again, the first demo of the first set is the one due.</summary>
    public void StartOver()
    {
        _set = 0;
        _begun = 0;
    }

    /// <summary>A demo starts the session's game afresh; which demo is due is not forgotten for that.</summary>
    public void BeginAgain()
    {
    }

    /// <inheritdoc/>
    public void Step(GameSession session)
    {
    }

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
