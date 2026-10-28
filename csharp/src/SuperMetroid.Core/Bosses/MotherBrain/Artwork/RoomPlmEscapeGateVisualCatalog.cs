using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Presentation-only selection for the three escape-gate frames. The compiled
/// level words continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class RoomPlmEscapeGateVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => EscapeGateArtwork.ContentIdentity(this);

    /// <summary>Only escape-gate frames whose visual words differ from compiled stock data.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all three escape-gate frames and copies visual differences without replacing physical level words, animation timing, or door handoff.</summary>
    /// <remarks>Even the visually open frame retains compiled solid collision; an artwork override cannot change that behavior.</remarks>
    /// <param name="entries">Exactly one four-word, visual-only entry for each compiled gate frame.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    public RoomPlmEscapeGateVisualCatalog(IEnumerable<RoomPlmVisualEntry> entries) =>
        customBlocks = new PlmVisualOverrides(entries, "Escape-gate", MotherBrainEscapeGateDraws.TryGetByVisualId,
            MotherBrainEscapeGateDraws.All.Count());

    /// <summary>Resolves one escape-gate block's selected appearance independently of its compiled solid collision word.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="blockIndex">Zero-based row ordinal, zero through three, down the vertical run.</param>
    /// <returns>The authored metatile/flip word, or the compiled frame's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported escape-gate frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The row ordinal is outside the four-block run.</exception>
    internal ushort GetWord(PlmDraw draw, int blockIndex) => EscapeGateArtwork.Word(customBlocks, draw, blockIndex);
}
