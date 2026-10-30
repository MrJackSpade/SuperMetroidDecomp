using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Spatial;
using static SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork.FileSelectFieldTileDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// Spells file select's stock slot fields and prompts in menu tiles (<see cref="FileSelectFieldTileDefinitions"/>, #1467):
/// each patch's cells, each digit and each slot letter.
/// </summary>
public static class FileSelectFieldTiles
{
    /// <summary>Native one-row patch extents at $81:B496, B4A8 and B4AC, excluding terminators.</summary>
    internal static int PatchCellCount(FileSelectPatch patch) => Ensure.Listed(PatchCellCounts, patch);

    /// <summary>Calculates text glyph selection and left-to-right cell placement for one native patch.</summary>
    internal static FileSelectCompiledPatchCell PatchCell(FileSelectPatch patch, int index)
    {
        if ((uint)index >= PatchCellCount(patch)) throw new ArgumentOutOfRangeException(nameof(index));
        return new(new Offset16<TilemapCells>(checked((short)index), 0), patch switch
        {
            FileSelectPatch.Energy => index < EnergyLabelConsecutiveCells
                ? EnergyLabelFirst with { Tile = EnergyLabelFirst.Tile + index }
                : EnergyLabelFinal,
            FileSelectPatch.TimeColon => TimeColonGlyph,
            FileSelectPatch.NoData => index == 0 ? BlankTile
                : NoDataLabel[index] == ' ' ? SmallSpace
                : SmallLetterA with { Tile = SmallLetterA.Tile + NoDataLabel[index] - 'A' },
            _ => throw new ArgumentOutOfRangeException(nameof(patch)),
        });
    }

    /// <summary>The menu tile of decimal digit <paramref name="digit"/>.</summary>
    internal static BackgroundTile Digit(int digit) => DigitTileBase with { Tile = DigitTileBase.Tile + digit };

    /// <summary>The menu tile of save slot <paramref name="slot"/>'s letter.</summary>
    internal static BackgroundTile SlotLetter(int slot) => SamusLetterTileBase with { Tile = SamusLetterTileBase.Tile + slot };
}
