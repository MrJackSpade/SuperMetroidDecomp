using Engine.Core;

namespace Game.Tests.Movies;

/// <summary>
/// What of the game is held against the cartridge's record of an update: results a player can observe, each read
/// from a member the game has for its own use. Nothing of the cartridge's own workings is compared (its poses, its
/// handlers, its places for things), and nothing is added to the game to be read here.
/// </summary>
/// <remarks>
/// What is compared so far: the random number, on every update. The rest of the plan (#3534) is added as the movies
/// reach it: the room, Samus's place and what she carries, the view, the escape's time, and the enemies and what they
/// have launched.
/// </remarks>
public sealed class MovieCheck
{
    private readonly MovieTrace _trace;
    private readonly GameSession _session;

    /// <summary>A check of <paramref name="session"/> against <paramref name="trace"/>.</summary>
    public MovieCheck(MovieTrace trace, GameSession session)
    {
        ArgumentNullException.ThrowIfNull(trace);
        ArgumentNullException.ThrowIfNull(session);
        (_trace, _session) = (trace, session);
    }

    /// <summary>The random number as the game has it: the sixteen bits the cartridge keeps.</summary>
    private int Random => (int)(_session.Random.Last.Number & 0xffff);

    /// <summary>The game as it stands, in a line, for a report.</summary>
    public string Describe() => $"{ModeNames(_session.Mode)}; random {Random:X4}";

    /// <summary>
    /// How the game differs from the cartridge's record last read (<see cref="MovieTrace.ReadTo"/>), each difference
    /// in a line with both values; none when they agree.
    /// </summary>
    public IReadOnlyList<string> Differences()
    {
        var differences = new List<string>();
        int random = _trace.Word(Cartridge.Random);
        if (random != Random)
            differences.Add($"the random number: the cartridge has {random:X4}, the game {Random:X4}");
        return differences;
    }

    private static string ModeNames(GameMode mode)
    {
        var names = new List<string>();
        for (GameMode? each = mode; each is not null; each = each.Keeps)
            names.Add(each.GetType().Name);
        return string.Join(" over ", names);
    }
}
