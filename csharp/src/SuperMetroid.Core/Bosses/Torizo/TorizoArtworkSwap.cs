namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One of the fifty-two pieces of artwork a Torizo's body programs swap in under its poses (the glowing eyes, the
/// crumbling statue, the awakening sheets), named by its place in the order the programs were authored.
/// </summary>
/// <param name="Ordinal">The swap's place among the fifty-two, from zero.</param>
internal readonly record struct TorizoArtworkSwap(int Ordinal)
{
    /// <summary>The artwork swaps the body programs make.</summary>
    internal const int Count = 52;

    /// <summary>The swap's place among the fifty-two.</summary>
    internal int Ordinal { get; } = (uint)Ordinal < Count ? Ordinal
        : throw new ArgumentOutOfRangeException(nameof(Ordinal), Ordinal, "Torizo's body programs make no such artwork swap.");
}
