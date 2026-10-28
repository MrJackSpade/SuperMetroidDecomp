using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Editable appearance for the cannon PLM. The compiled draw geometry, full
/// level words, hit thresholds, damage transitions, and control writes remain fixed.
/// </summary>
public sealed class RoomPlmDraygonCannonVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => DraygonCannonArtwork.ContentIdentity(this);

    /// <summary>Owned edited visual words by canonical frame pointer; null retains compiled stock appearance.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete reachable cannon-frame coverage and copies visual differences without changing shield damage, hit thresholds, control writes, or draw geometry.</summary>
    /// <param name="entries">One visual-only entry per compiled frame with the exact flattened count of all its run words.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves reachable frame coverage incomplete.</exception>
    public RoomPlmDraygonCannonVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customBlocks = new PlmVisualOverrides(entries, "Draygon cannon", DraygonCannonDraws.TryGetByVisualId,
            DraygonCannonDraws.All.Count());

    /// <summary>Resolves a cannon block's selected appearance by projecting native run-local indices into the flattened authored frame.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the frame's native draw run.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a flattened entry index.</param>
    /// <returns>The selected metatile/flip word, or visual bits from the compiled physical word when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported cannon frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex) =>
        DraygonCannonArtwork.Word(customBlocks, draw, runIndex, blockIndex);
}
