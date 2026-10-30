using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// The menu tiles file select's slot fields and prompts are written with (#1467): only the import reads them; it pictures
/// each patch, digit, slot letter and energy tank once, and the game places those pictures.
/// </summary>
public static class FileSelectFieldTileDefinitions
{
    /// <summary>The blank menu tile.</summary>
    internal static BackgroundTile BlankTile { get; } = new(0x0f, 0, highPriority: false);
    /// <summary>The first numeric menu tile: the digits follow it in order.</summary>
    internal static BackgroundTile DigitTileBase { get; } = new(0x60, 0, highPriority: true);
    /// <summary>The first letter tile used to draw a save slot's name in the Copy and Clear prompts: the slots' letters follow it.</summary>
    internal static BackgroundTile SamusLetterTileBase { get; } = new(0x6a, 0, highPriority: true);
    /// <summary>$81:A0F5: the filled-tank tile, drawn in the installed health digit's colour variant.</summary>
    internal const int FilledTankTile = 0x98;
    /// <summary>$81:A0EC: the empty-tank tile, drawn in the installed health digit's colour variant.</summary>
    internal const int EmptyTankTile = 0x99;
    /// <summary>$81:B4AC-B4C0, the literal small-font empty-slot label including its padding.</summary>
    internal const string NoDataLabel = " NO DATA   ";
    /// <summary>$81:B4B6: tile $6A is the small-font A; following alphabet glyphs are consecutive.</summary>
    internal static readonly BackgroundTile SmallLetterA = new(0x6a, 0, highPriority: true);
    /// <summary>$81:B4B2 and B4BC-B4C0: the small-font space.</summary>
    internal static readonly BackgroundTile SmallSpace = new(0x0f, 0, highPriority: true);
    /// <summary>$81:B496-B49A: three consecutive tiles composing the ENERGY label.</summary>
    internal static readonly BackgroundTile EnergyLabelFirst = new(0x9d, 0, highPriority: true);
    /// <summary>$81:B49C: the separately placed final glyph of the ENERGY patch.</summary>
    internal static readonly BackgroundTile EnergyLabelFinal = new(0xcc, 0, highPriority: true);
    /// <summary>$81:B4A8: the time-field colon glyph.</summary>
    internal static readonly BackgroundTile TimeColonGlyph = new(0x8c, 0, highPriority: true);
    /// <summary>The ENERGY patch's cells: three consecutive label tiles and the final glyph.</summary>
    internal const int EnergyLabelCells = 4;
    /// <summary>The ENERGY patch's consecutive label tiles before its final glyph.</summary>
    internal const int EnergyLabelConsecutiveCells = 3;

    /// <summary>Native one-row patch extents at $81:B496, B4A8 and B4AC, excluding terminators.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectPatch, int> PatchCellCounts =
        new Dictionary<FileSelectPatch, int>
        {
            [FileSelectPatch.Energy] = EnergyLabelCells,
            [FileSelectPatch.TimeColon] = 1,
            [FileSelectPatch.NoData] = NoDataLabel.Length,
        };
}
