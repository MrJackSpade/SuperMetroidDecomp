using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Editable elevatube presentation. The physical block, hold, sound, and
/// deletion continue to use compiled cartridge definitions.
/// </summary>
public sealed class InstalledRoomPlmMaridiaElevatubeVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmMaridiaElevatubeVisuals),
        content => content.AppendAppearances("block", [appearance]));

    /// <summary>
    /// Captured metatile and flip selection for the tube's sole visible cell,
    /// independent of the compiled physical block and hold behavior.
    /// </summary>
    private readonly RoomBlockAppearance appearance;

    /// <summary>Validates and selects the elevatube's single visual block without changing its terrain, sixteen-update hold, sound, or deletion.</summary>
    /// <param name="entries">Exactly one entry with the compiled elevatube identity and one appearance.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">The sequence does not contain exactly one valid single-block elevatube entry.</exception>
    public InstalledRoomPlmMaridiaElevatubeVisuals(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        RoomPlmVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            !MaridiaElevatubeDraws.TryGetDrawByVisualId(
                selected[0].Id, out _) ||
            selected[0].Blocks is not { Length: 1 } blocks)
            throw new InvalidDataException(
                "Maridia elevatube visuals require exactly one visual block.");
        appearance = blocks[0];
    }

    /// <summary>Resolves the installed elevatube appearance for its sole draw cell; the Kraid crumble that shares its cartridge draw does not share this catalog's selection.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Native run ordinal; must be zero.</param>
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
