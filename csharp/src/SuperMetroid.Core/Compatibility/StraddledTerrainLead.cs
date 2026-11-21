namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One class of blocks whose end contributes the same to a Wave shot's straddled read (<see cref="StraddledTerrain{TTerrain}"/>),
/// with the terrain it makes with each tail class.
/// </summary>
/// <typeparam name="TTerrain">The terrain a room holds.</typeparam>
public sealed class StraddledTerrainLead<TTerrain>
    where TTerrain : notnull
{
    /// <summary>The terrain the class makes with each tail class, by the tail class.</summary>
    private readonly Dictionary<StraddledTerrainTail, TTerrain> outcomes;

    /// <summary>A lead class that makes <paramref name="outcomes"/> with the tail classes they are keyed by.</summary>
    /// <param name="outcomes">The terrain the class makes with each tail class, by the tail class.</param>
    public StraddledTerrainLead(IReadOnlyDictionary<StraddledTerrainTail, TTerrain> outcomes) =>
        this.outcomes = new Dictionary<StraddledTerrainTail, TTerrain>(Ensure.NotNull(outcomes));

    /// <summary>Whether the class makes terrain with <paramref name="tail"/>.</summary>
    internal bool MakesTerrainWith(StraddledTerrainTail tail) => outcomes.ContainsKey(tail);

    /// <summary>The terrain the class makes with <paramref name="tail"/>.</summary>
    /// <exception cref="KeyNotFoundException">The class makes no terrain with the tail class.</exception>
    internal TTerrain With(StraddledTerrainTail tail) => outcomes[tail];
}
