using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// Presentation-only elevator-platform block selection. The native four-step
/// instruction loop, draw geometry, and physical collision words stay compiled.
/// </summary>
public sealed class RoomPlmElevatorPlatformVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => ElevatorPlatformArtwork.ContentIdentity(this);

    /// <summary>Only frames whose visual words differ from the compiled source are stored here.</summary>
    private readonly Dictionary<PlmDraw, ushort[][]>? customWords;

    /// <summary>Validates all three elevator-platform draw layouts and copies visual differences while retaining native run geometry, collision words, and animation cadence.</summary>
    /// <remarks>The four-step instruction loop selects first, second, third, then second layout; the repeated second layout has only one entry.</remarks>
    /// <param name="entries">One visual-only entry per compiled frame, with its exact native run and word counts.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    public RoomPlmElevatorPlatformVisualCatalog(
        IEnumerable<RoomPlmVisualRunEntry> entries) =>
        customWords = ElevatorPlatformArtwork.Overrides(entries);

    /// <summary>Resolves one elevator-platform visual block without replacing its compiled physical word or draw offset.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the native draw run.</param>
    /// <param name="wordIndex">Zero-based word ordinal within that run, not a room coordinate.</param>
    /// <returns>The selected metatile/flip word, derived from the compiled stock frame when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported platform frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int wordIndex) =>
        ElevatorPlatformArtwork.Word(customWords, draw, runIndex, wordIndex);
}
