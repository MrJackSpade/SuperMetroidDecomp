using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pictures the turning floor of a tileset whose rooms show one (#1392, #1467, #1649, #2104): the whole floor a room of the
/// tileset starts from (<see cref="FloorSheetComposer.Whole"/>), whose extent the game takes from it, and every floor
/// animation frame drawn over it. Its tiles' pixels are the high bytes of the VRAM words the room's character uploads fill, and its
/// 128 by 128 cells the low bytes of the first 16384 of those words (#1422): the elevator shaft's and Ceres Ridley's
/// tilesets upload their turning floor's map and tiles there, interleaved, as the whole of their tile characters. They are
/// coloured with the tileset's standard palette and, where a pixel names a sprite colour, the sprite half as a room load
/// leaves it: the initial gameplay palette with the common sprite and enemy-projectile rows restored.
/// </summary>
internal static partial class RoomFloorSheetComposer
{
    /// <summary>
    /// Composes <paramref name="tileset"/>'s floor sheet over the room as it has loaded, shown whole where a room of the
    /// tileset repeats the floor beyond its edges (<see cref="RepeatingFloors"/>, #1646).
    /// </summary>
    /// <exception cref="InvalidDataException">The installed palettes are invalid, no cell of the floor shows anything, or the floor's pictures overlap.</exception>
    internal static ComposedSpriteSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        // The room tile characters' bytes as the room's load leaves them, a later upload over an earlier.
        var words = new byte[BgCharacterSet.Tiles.CharacterCount() * BgCharacterSet.Tiles.CharacterBytes()];
        foreach (BgCharacterUpload upload in RoomBlockUploads.LoadUploads(inputs, tileset).Where(upload => upload.Set == BgCharacterSet.Tiles))
            upload.Bytes.Span.CopyTo(words.AsSpan(upload.FirstCharacter * BgCharacterSet.Tiles.CharacterBytes()));
        RgbColor[] background = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));
        // The sprite half as a room load leaves it: gameplay starts from the initial palette, and each room load restores
        // the common sprite and enemy-projectile rows; an actor's own row shows its live colours over these.
        var colours = new RgbColor[ImageColourGroup.ColourCountOf(SpriteImageColours.FullPalette)];
        background.CopyTo(colours, 0);
        RoomLoadPalettes.SpriteColours(inputs.Objects.Palettes).CopyTo(colours, background.Length);
        return ComposeFloor(words, colours) with { ShownWhole = RepeatingFloors.Repeats(tileset) };
    }

    /// <summary>
    /// The floor sheet of floor memory <paramref name="words"/>, coloured from <paramref name="colours"/>: each of its first
    /// 16384 words holds a floor cell in its low byte and a tile pixel in its high byte.
    /// </summary>
    /// <exception cref="InvalidDataException">The memory holds less than the whole floor, no cell of it shows anything, or the floor's pictures overlap.</exception>
    internal static ComposedSpriteSheet ComposeFloor(ReadOnlySpan<byte> words, ReadOnlySpan<RgbColor> colours)
    {
        if (words.Length < FloorSheetComposer.CellCount * sizeof(ushort))
            throw new InvalidDataException($"Floor memory holds {words.Length} bytes, less than the {FloorSheetComposer.CellCount * sizeof(ushort)} of a whole floor.");
        var map = new byte[FloorSheetComposer.CellCount];
        var pixels = new byte[FloorSheetComposer.TileSetBytes];
        for (int word = 0; word < map.Length; word++)
        {
            map[word] = words[word * sizeof(ushort)];
            pixels[word] = words[word * sizeof(ushort) + 1];
        }
        if (!FloorSheetComposer.ShowsAnything(RoomFloorSheet.FloorFrameName, map, pixels))
            throw new InvalidDataException($"No cell of floor {RoomFloorSheet.FloorFrameName} shows anything.");
        // The floor holds only the floor (#2573): where an actor rests on it, as the room's map pictures one of the actor's
        // pictures, it shows nothing, and the actor shows that picture there, installed as its resting picture.
        IReadOnlyDictionary<CeresFloorActor, CeresFloorActorPicture> resting = CeresFloorDrawings.Resting(map);
        FloorSheetComposer.FloorPicture whole = FloorSheetComposer.Whole(RoomFloorSheet.FloorFrameName, map, pixels)
            with { Cleared = resting.Values.SelectMany(CeresFloorDrawings.Covered).ToHashSet() };
        return FloorSheetComposer.ComposeRoom([whole, .. CeresFloorDrawings.Pictures(map, pixels),
            .. CeresFloorActors.DrawingOrder.Where(resting.ContainsKey)
                .Select(actor => CeresFloorDrawings.Picture(resting[actor], map, pixels) with { Name = RoomFloorSheet.RestingNameOf(resting[actor]) })],
            colours);
    }
}
