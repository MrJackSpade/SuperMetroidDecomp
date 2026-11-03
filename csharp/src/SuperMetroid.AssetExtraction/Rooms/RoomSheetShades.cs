using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
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
    /// records its rooms' palette effects show, and the colours of the bosses its rooms hold that recolour the room.
    /// </summary>
    internal static IReadOnlyList<SheetShade> Room(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        (RoomHeaderDefinition Room, RoomStateSettings State)[] rooms = [.. RoomsOf(tileset)];
        var shades = new List<SheetShade>(PaletteFxRoomShades.Of(rooms, inputs.Maps.RoomPaletteFx));
        if (rooms.Any(room => room.State.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.Kraid)))
        {
            RgbColor[] standard = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));
            shades.AddRange(KraidBackdropShades.Of(standard, inputs.Bosses.KraidColors
                ?? throw new InvalidDataException("Kraid's room is shaded from the installed Kraid colours.")));
        }
        if (rooms.Any(room => room.State.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.DraygonBody)))
            shades.AddRange(BossSheetShades.DraygonBackground(inputs.Bosses));
        if (rooms.Any(room => room.State.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.SporeSpawn)))
            shades.AddRange(SporeSpawnSheetShades.Room(inputs.Bosses));
        if (HoldsPhantoon(rooms))
            shades.AddRange(PhantoonSheetShades.PowerOn(inputs.Bosses, RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette))));
        return shades;
    }

    /// <summary>Whether any of <paramref name="rooms"/> places Phantoon, whose death brings the Wrecked Ship's power on over it.</summary>
    private static bool HoldsPhantoon(IEnumerable<(RoomHeaderDefinition Room, RoomStateSettings State)> rooms) =>
        rooms.Any(room => room.State.EnemyPopulation.Records.Span.Any(record => record.DefinitionId == EnemyDefinitionId.PhantoonBody));
    }

    /// <summary>
    /// The shades of <paramref name="tileset"/>'s layer-3 pictures, the HUD's and the room effects' pages: the HUD's door
    /// fades and the room effects' palette blends.
    /// </summary>
    internal static IReadOnlyList<SheetShade> Layer3(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        RgbColor[] standard = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));
        return [.. DoorFadeHudShades.Of(standard), .. RoomFxBlendSheetShades.Of(inputs.Maps.RoomFxPaletteBlends),
            .. HoldsPhantoon(RoomsOf(tileset)) ? PhantoonSheetShades.PowerOn(inputs.Bosses, standard) : []];
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
