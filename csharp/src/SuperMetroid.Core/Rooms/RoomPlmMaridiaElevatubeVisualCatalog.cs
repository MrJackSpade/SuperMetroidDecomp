using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>The elevatube PLM's single editable visible block.</summary>
/// <param name="Id">The compiled visual identity <c>elevatube-block</c>.</param>
/// <param name="Blocks">Exactly one metatile/flip word; the catalog retains its scalar value, not the supplied array.</param>
public sealed record RoomPlmMaridiaElevatubeVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable elevatube presentation. The physical block, hold, sound, and
/// deletion continue to use compiled cartridge definitions.
/// </summary>
public sealed class RoomPlmMaridiaElevatubeVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmMaridiaElevatubeVisualCatalog),
        content => content.Append("visual word", visualWord));

    private readonly ushort visualWord;

    /// <summary>Validates and selects the elevatube's single visual block without changing its physical word, sixteen-update hold, sound, or deletion.</summary>
    /// <param name="entries">Exactly one entry with the compiled elevatube identity and one visual-only word.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">The sequence does not contain exactly one valid single-block elevatube entry.</exception>
    public RoomPlmMaridiaElevatubeVisualCatalog(
        IEnumerable<RoomPlmMaridiaElevatubeVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        RoomPlmMaridiaElevatubeVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            !MaridiaElevatubePlmDefinitions.TryGetDrawByVisualId(
                selected[0].Id, out _) ||
            selected[0].Blocks is not { Length: 1 } blocks ||
            !RoomLevelWord.IsValidVisualWord(blocks[0]))
            throw new InvalidDataException(
                "Maridia elevatube visuals require exactly one visual block.");
        visualWord = blocks[0];
    }

    /// <summary>Resolves the installed elevatube appearance for its sole draw cell; the shared Kraid draw pointer does not share this catalog's selection.</summary>
    /// <param name="drawPointer">The compiled bank-$84 draw identity $9367.</param>
    /// <param name="runIndex">Native run ordinal; must be zero.</param>
    /// <param name="blockIndex">Word ordinal within the run; must be zero.</param>
    /// <returns>The selected metatile/flip word.</returns>
    /// <exception cref="InvalidDataException">The draw pointer, run ordinal, or block ordinal does not identify the sole elevatube cell.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (drawPointer != MaridiaElevatubePlmDefinitions.DrawPointer ||
            runIndex != 0 || blockIndex != 0)
            throw new InvalidDataException(
                $"Maridia elevatube visuals lack draw ${drawPointer:X4} run {runIndex} block {blockIndex}.");
        return visualWord;
    }
}
