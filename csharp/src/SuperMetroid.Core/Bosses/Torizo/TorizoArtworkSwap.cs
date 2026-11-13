using System.Globalization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One of the fifty-two pieces of artwork a Torizo's body animations swap in under its poses (the glowing eyes, the
/// crumbling statue, the awakening sheets), named by its place in the order the animations were authored.
/// </summary>
internal readonly record struct TorizoArtworkSwap
{
    /// <summary>The artwork swaps the body animations make.</summary>
    internal const int Count = 52;

    /// <summary>
    /// The swap's place among the fifty-two, from zero: the identity two swaps compare by. The import names each swap's
    /// pictures by it.
    /// </summary>
    internal readonly int ordinal;

    /// <summary>Names the swap at <paramref name="ordinal"/> among the fifty-two.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The body animations make no swap at that place.</exception>
    internal TorizoArtworkSwap(int ordinal) => this.ordinal = (uint)ordinal < Count ? ordinal
        : throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Torizo's body programs make no such artwork swap.");

    /// <summary>The look the swap gives <paramref name="torizo"/>, named by the swap's place (#1538).</summary>
    internal EncounterLook Look(EncounterArtwork torizo) => new(torizo, string.Create(CultureInfo.InvariantCulture, $"Swap{ordinal}"));

    /// <summary>Names the swap by its place, as a sequence that does not show it reports it.</summary>
    public override string ToString() => $"Torizo artwork swap {ordinal}";
}
