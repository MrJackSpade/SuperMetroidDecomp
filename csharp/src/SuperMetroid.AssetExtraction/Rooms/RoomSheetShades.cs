using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The shades the import pre-colours a tileset's room pictures in (#1512): every colouring a colour effect of its rooms
/// shows besides the tileset's standard colours, which gameplay names by the effect's typed state.
/// </summary>
internal static class RoomSheetShades
{
    /// <summary>
    /// The shades of <paramref name="tileset"/>'s layer-3 pictures, the HUD's and the room effects' pages: the HUD's door
    /// fades and the room effects' palette blends.
    /// </summary>
    internal static IReadOnlyList<SheetShade> Layer3(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        RgbColor[] standard = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));
        return [.. DoorFadeHudShades.Of(standard), .. RoomFxBlendSheetShades.Of(inputs.Maps.RoomFxPaletteBlends)];
    }

    /// <summary><paramref name="sheet"/>, also pre-coloured in <paramref name="shades"/>.</summary>
    internal static ComposedWrittenPictureSheet With(ComposedWrittenPictureSheet sheet, IReadOnlyList<SheetShade> shades) =>
        Ensure.NotNull(sheet) with { Sheet = ShadedSheets.With(sheet.Sheet, shades) };
}
