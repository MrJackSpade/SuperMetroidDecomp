using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Floors composed for checks (#1467) as the import composes installed floors: from a cartridge memory image's floor map
/// (each word's low byte) and tile pixels (each word's high byte), under a palette.
/// </summary>
internal static class AffineFloorConstants
{
    /// <summary>The name the composed floor's whole picture takes.</summary>
    private const string FloorFrame = "floor";

    /// <summary>
    /// The floor memory image <paramref name="image"/> maps, coloured from <paramref name="palette"/>. The import's room floor
    /// path composes it, so its colour groups name the gameplay background sets they draw in as installed room floors do;
    /// a frontend screen's colours ignore those sets, so the same floor serves gameplay and frontend frames.
    /// </summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image.</exception>
    public static AffineFloor Capture(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette)
    {
        Ensure.LengthEqual(image, SnesVram.ByteCount);
        FloorMemory memory = Split(image);
        return new AffineFloor(Load(FloorSheetComposer.ComposeRoom([FloorSheetComposer.Whole(FloorFrame, memory.Map, memory.Pixels)], palette), "floor-fixture").Frame(FloorFrame));
    }

    /// <summary>
    /// A Ceres room floor sheet whose floor is <paramref name="map"/>'s cells over <paramref name="pixels"/>' tiles, with every
    /// floor animation frame pictured over it, coloured from <paramref name="palette"/>.
    /// </summary>
    public static RoomFloorSheet CeresRoom(ReadOnlySpan<byte> map, byte[] pixels, ReadOnlySpan<RgbColor> palette) =>
        new(default, Load(FloorSheetComposer.ComposeRoom(
            [FloorSheetComposer.Whole(RoomFloorSheet.FloorFrameName, map, pixels), .. CeresFloorDrawings.Pictures(map, pixels)], palette),
            "ceres-floor-fixture"));

    /// <summary>A Ceres room floor sheet of tile zero everywhere, whose tiles hold zeros and show nothing.</summary>
    public static RoomFloorSheet BlankCeresRoom() =>
        CeresRoom(new byte[FloorSheetComposer.CellCount], new byte[FloorSheetComposer.TileSetBytes], new RgbColor[SnesCgram.ColorCount]);

    /// <summary>The floor map (each word's low byte) and tile pixels (each word's high byte) of memory image <paramref name="image"/>.</summary>
    public static FloorMemory Split(ReadOnlySpan<byte> image)
    {
        var map = new byte[FloorSheetComposer.CellCount];
        var pixels = new byte[FloorSheetComposer.TileSetBytes];
        for (int word = 0; word < map.Length; word++)
        {
            map[word] = image[word * sizeof(ushort)];
            pixels[word] = image[word * sizeof(ushort) + 1];
        }
        return new FloorMemory(map, pixels);
    }

    /// <summary>The composed sheet as an installed sheet would load.</summary>
    private static SpriteSheet Load(ComposedSpriteSheet composed, string name) =>
        SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), name,
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartGroups);
}
