namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The charge-flare components a frame shows, as <c>$90:BAFC</c> draws them after advancing them.</summary>
/// <param name="Count">Components shown: none, the main flare alone, or the flare and both sparks.</param>
/// <param name="Descending">
/// True for Hyper Beam's descending program, which shows the fast spark first (native offsets four, two, zero).
/// </param>
public readonly record struct ChargeFlareComponents(int Count, bool Descending)
{
    /// <summary>The shown components in drawing order.</summary>
    public IEnumerable<int> DrawOrder => Descending ? Enumerable.Range(0, Count).Reverse() : Enumerable.Range(0, Count);
}
