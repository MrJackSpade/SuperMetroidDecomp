using SuperMetroid.Core.Frontend.Menus.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Names and composes the file-select document's parts (<see cref="FileSelectPresentationDefinitions"/>): its pages, patches,
/// borders and anchors, each bordered heading, and each patch's cells.
/// </summary>
public static class FileSelectParts
{
    /// <summary>The JSON name of a foreground page.</summary>
    public static string PageName(FileSelectPage page) =>
        Ensure.Listed(FileSelectPresentationDefinitions.PageNames, page);

    /// <summary>The JSON name of a slot-field patch.</summary>
    public static string PatchName(FileSelectPatch patch) =>
        Ensure.Listed(FileSelectPresentationDefinitions.PatchNames, patch);

    /// <summary>The JSON name of a bordered heading, which keys its anchor.</summary>
    public static string BorderName(FileSelectBorder border) =>
        Ensure.Listed(FileSelectPresentationDefinitions.BorderNames, border);

    /// <summary>The JSON name of a bordered heading's sprite.</summary>
    public static string BorderSpriteName(FileSelectBorder border) =>
        Ensure.Listed(FileSelectPresentationDefinitions.BorderSpriteNames, border);

    /// <summary>The JSON name of a copy or clear page's slot-letter anchor.</summary>
    public static string AnchorName(FileSelectAnchor anchor) =>
        Ensure.Listed(FileSelectPresentationDefinitions.AnchorNames, anchor);

    /// <summary>The menu heading a bordered heading draws.</summary>
    internal static MenuHeading Heading(FileSelectBorder border) =>
        Ensure.Listed(FileSelectPresentationDefinitions.Headings, border);

    /// <summary>Native one-row patch extents at $81:B496, B4A8 and B4AC, excluding terminators.</summary>
    internal static int PatchCellCount(FileSelectPatch patch) =>
        Ensure.Listed(FileSelectPresentationDefinitions.PatchCellCounts, patch);

    /// <summary>Calculates text glyph selection and left-to-right cell placement for one native patch.</summary>
    internal static FileSelectCompiledPatchCell PatchCell(FileSelectPatch patch, int index)
    {
        if ((uint)index >= PatchCellCount(patch)) throw new ArgumentOutOfRangeException(nameof(index));
        return new(new Offset16<TilemapCells>(checked((short)index), 0), patch switch
        {
            FileSelectPatch.Energy => index < 3
                ? FileSelectPresentationDefinitions.EnergyLabelFirst with { Tile = FileSelectPresentationDefinitions.EnergyLabelFirst.Tile + index }
                : FileSelectPresentationDefinitions.EnergyLabelFinal,
            FileSelectPatch.TimeColon => FileSelectPresentationDefinitions.TimeColonGlyph,
            FileSelectPatch.NoData => index == 0 ? FileSelectLayout.BlankTile
                : FileSelectPresentationDefinitions.NoDataLabel[index] == ' ' ? FileSelectPresentationDefinitions.SmallSpace
                : FileSelectPresentationDefinitions.SmallLetterA with
                {
                    Tile = FileSelectPresentationDefinitions.SmallLetterA.Tile + FileSelectPresentationDefinitions.NoDataLabel[index] - 'A',
                },
            _ => throw new ArgumentOutOfRangeException(nameof(patch)),
        });
    }
}
