using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The shades the import pre-colours a tileset's room pictures in (#1512): every colouring a colour effect of its rooms
/// shows besides the tileset's standard colours, which gameplay names by the effect's typed state.
/// </summary>
internal static class RoomSheetShades
{
    /// <summary>
    /// The shades of <paramref name="tileset"/>'s room pictures, its blocks, backgrounds, written pictures and floor: the
    /// records its rooms' palette effects show.
    /// </summary>
    internal static IReadOnlyList<SheetShade> Room(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        return PaletteFxRoomShades.Of(RoomsOf(tileset), inputs.Maps.RoomPaletteFx);
    }

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

    /// <summary><paramref name="background"/>, its image also pre-coloured in <paramref name="shades"/>.</summary>
    internal static ComposedRoomBackground With(ComposedRoomBackground background, IReadOnlyList<SheetShade> shades) =>
        Ensure.NotNull(background) with { Sheet = ShadedSheets.With(background.Sheet, shades) };

    /// <summary>Every retail room state that loads <paramref name="tileset"/>, with its room.</summary>
    private static IEnumerable<(RoomHeaderDefinition Room, RoomStateSettings State)> RoomsOf(TilesetDefinition tileset) =>
        RetailRoomHeaders.All.SelectMany(room => RoomStateLists.GetStates(room)
            .Where(state => RoomTilesets.Get(state.GraphicsSet) == tileset)
            .Select(state => (room, state)));
}
