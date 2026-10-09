using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>The visible block selected by one Speed Booster terrain frame.</summary>
/// <param name="Id">Case-sensitive visual identity <c>bomb-reveal</c> for native draw $84:A4F3.</param>
/// <param name="Blocks">Mutable input array containing exactly one twelve-bit visual word: a ten-bit 16-by-16 metatile index plus parent flip bits, without collision bits.</param>
public sealed record RoomPlmSpeedBoosterVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable bomb-reveal appearance. The type-B collision word and PLM timing
/// remain in compiled cartridge definitions.
/// </summary>
public sealed class RoomPlmSpeedBoosterVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmSpeedBoosterVisualCatalog),
        content => content.Append("visual word", visualWord));

    private readonly ushort visualWord;

    /// <summary>Validates and captures the single bomb-reveal appearance while preserving compiled Speed Booster collision and timing.</summary>
    /// <param name="entries">Exactly one <c>bomb-reveal</c> entry containing one valid visual word; its value is captured, so later input-array edits do not affect the catalog.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Entry count, identity, block count, or presentation-only word bits do not match the compiled reveal.</exception>
    public RoomPlmSpeedBoosterVisualCatalog(
        IEnumerable<RoomPlmSpeedBoosterVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        RoomPlmSpeedBoosterVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            !SpeedBoosterBlockPlmDrawDefinitions.TryGetByVisualId(
                selected[0].Id, out _) ||
            selected[0].Blocks is not { Length: 1 } blocks ||
            !RoomLevelWord.IsValidVisualWord(blocks[0]))
            throw new InvalidDataException(
                "Speed Booster visuals require exactly one bomb-reveal visual block.");
        visualWord = blocks[0];
    }

    /// <summary>Resolves the installed visual-only word for the sole Speed Booster bomb-reveal cell.</summary>
    /// <param name="drawPointer">Bank-$84 draw-list pointer; only $A4F3 is accepted.</param>
    /// <param name="runIndex">Zero, the reveal's only draw run.</param>
    /// <param name="blockIndex">Zero, the run's only 16-by-16 block.</param>
    /// <returns>The ten-bit metatile index and two parent flip bits; the compiled physical word supplies collision separately.</returns>
    /// <exception cref="InvalidDataException">Any selector does not name the sole compiled reveal cell.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (drawPointer != SpeedBoosterBlockPlmDrawDefinitions.BombReveal.Pointer ||
            runIndex != 0 || blockIndex != 0)
            throw new InvalidDataException(
                $"Speed Booster visuals lack draw ${drawPointer:X4} run {runIndex} block {blockIndex}.");
        return visualWord;
    }
}
