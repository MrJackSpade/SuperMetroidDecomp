using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Editable elevatube presentation. The physical block, hold, sound, and
/// deletion continue to use compiled definitions.
/// </summary>
public sealed class InstalledRoomPlmMaridiaElevatubeVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmMaridiaElevatubeVisuals),
        content => content.AppendAppearances("block", [appearance]));

    /// <summary>
    /// Captured metatile and flip selection for the tube's sole visible cell,
    /// independent of the compiled physical block and hold behavior.
    /// </summary>
    private readonly RoomBlockAppearance appearance;

    /// <summary>Holds the import-validated appearance of the elevatube's single visual block.</summary>
    /// <param name="appearance">The block the elevatube's sole cell shows.</param>
    internal InstalledRoomPlmMaridiaElevatubeVisuals(RoomBlockAppearance appearance) => this.appearance = appearance;

    /// <summary>Resolves the installed elevatube appearance for its sole draw cell; the Kraid crumble that shares its draw does not share this catalog's selection.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Run ordinal; must be zero.</param>
    /// <param name="blockIndex">Block ordinal within the run; must be zero.</param>
    /// <returns>The selected appearance.</returns>
    /// <exception cref="InvalidDataException">The draw pointer, run ordinal, or block ordinal does not identify the sole elevatube cell.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (draw != MaridiaElevatubePlmDefinitions.BlockDraw ||
            runIndex != 0 || blockIndex != 0)
            throw new InvalidDataException(
                $"Maridia elevatube visuals lack draw {draw} run {runIndex} block {blockIndex}.");
        return appearance;
    }
}
