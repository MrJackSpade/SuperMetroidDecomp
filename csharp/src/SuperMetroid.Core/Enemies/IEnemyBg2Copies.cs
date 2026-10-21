namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The copies of the enemy BG2 image that bosses keep besides the presented one (#1395): an extended frame's BG2 half
/// writes into each copy that exists now. The bosses implement it.
/// </summary>
internal interface IEnemyBg2Copies
{
    /// <summary>Copies <paramref name="words"/> at <paramref name="destinationWord"/> into each copy that exists now.</summary>
    void Write(int destinationWord, ReadOnlySpan<ushort> words);

    /// <summary>
    /// Whether a boss stages its BG2 body instead of uploading it: Mother Brain stages hers until her phase-two
    /// transition attaches the enemy BG2 image (#1356).
    /// </summary>
    bool StagesBody { get; }
}
