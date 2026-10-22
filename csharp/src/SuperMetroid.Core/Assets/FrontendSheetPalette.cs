namespace SuperMetroid.Core.Assets;

/// <summary>
/// The palette of a frontend sheet frame drawn with a palette chosen per draw (#1392): the sheet colours the frame's
/// palette-inheriting parts in <see cref="FramePalette"/>, and a draw in another row overrides it for those parts.
/// </summary>
internal static class FrontendSheetPalette
{
    /// <summary>The row a frontend sheet colours each frame's palette-inheriting parts in.</summary>
    internal const int FramePalette = 0;

    /// <summary>
    /// The override that places a frame's palette-inheriting parts in the row <paramref name="paletteBits"/> select, or
    /// null where that is the row the sheet coloured them in.
    /// </summary>
    internal static int? Override(ushort paletteBits)
    {
        int row = SnesObjAttributeWord.FromPaletteBits(paletteBits).PaletteIndex;
        return row == FramePalette ? null : row;
    }
}
