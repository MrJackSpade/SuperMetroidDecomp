using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The installed title presentation (#1409): its Mode 7 tile layout, its ordered OBJ compositions and how many floor, OBJ
/// and Baby Metroid characters its artwork holds. The installed title sheets picture the characters; their pixels belong
/// to the import pipeline that reads the installed artwork. Motion, animation page order, and VRAM destinations remain
/// compiled behavior.
/// </summary>
public class TitleGraphicsPresentation
{
    /// <summary>The 64-by-64 title map of Mode 7 tile numbers.</summary>
    private readonly byte[] floorMap;
    /// <summary>Compiled OAM compositions keyed by title composition.</summary>
    private readonly Dictionary<TitleSprite, SpriteComposition> sprites;

    /// <summary>Reads and validates the title's Mode 7 map and compositions, and records its artwork's character counts.</summary>
    /// <param name="floorMapJson">JSON document containing the Mode 7 map and OBJ compositions.</param>
    /// <param name="floorCharacterCount">The installed Mode 7 floor characters.</param>
    /// <param name="objectCharacterCount">The installed title OBJ characters.</param>
    /// <param name="babyCharacterCount">The installed Baby Metroid Mode 7 characters.</param>
    /// <exception cref="InvalidDataException">The map or a composition is invalid.</exception>
    internal TitleGraphicsPresentation(Stream floorMapJson, int floorCharacterCount, int objectCharacterCount, int babyCharacterCount)
    {
        TitleFloorMapDocument map;
        try
        {
            map = JsonAssetDocument.Read<TitleFloorMapDocument>(
                floorMapJson,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Title Mode 7 map is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid title Mode 7 map JSON.", error);
        }
        if (map.Version != TitleGraphicsFormat.Version ||
            map.Width != TitleGraphicsFormat.MapWidth ||
            map.Height != TitleGraphicsFormat.MapHeight ||
            map.Tiles is null ||
            map.Tiles.Length != TitleGraphicsFormat.MapWidth * TitleGraphicsFormat.MapHeight ||
            map.Tiles.Any(tile => (uint)tile > byte.MaxValue))
        {
            throw new InvalidDataException(
                $"Title Mode 7 map requires version {TitleGraphicsFormat.Version}, " +
                $"{TitleGraphicsFormat.MapWidth}x{TitleGraphicsFormat.MapHeight} cells, and eight-bit tile indexes.");
        }
        sprites = CompileSprites(map.Sprites);
        floorMap = map.Tiles.Select(tile => checked((byte)tile)).ToArray();
        FloorCharacterCount = floorCharacterCount;
        ObjectCharacterCount = objectCharacterCount;
        BabyCharacterCount = babyCharacterCount;
    }

    /// <summary>64-by-64 title map of Mode-7 character indexes.</summary>
    public ReadOnlySpan<byte> FloorMap => floorMap;

    /// <summary>The installed Mode 7 floor characters.</summary>
    public int FloorCharacterCount { get; }

    /// <summary>The installed title OBJ characters.</summary>
    public int ObjectCharacterCount { get; }

    /// <summary>The installed Baby Metroid Mode 7 characters.</summary>
    public int BabyCharacterCount { get; }

    /// <summary>One title composition as a whole frame over the title OBJ page (#1392); its parts take their draw's palette.</summary>
    internal SpriteFrame Frame(TitleSprite sprite, string name) => sprites.TryGetValue(sprite, out SpriteComposition? composition)
        ? composition.ToFrame(name, 0) : throw new InvalidDataException($"Title composition {sprite} is missing from installed artwork.");

    /// <summary>Places one title frame of the title sheet, its parts in the row <paramref name="paletteBits"/> select.</summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Position16<OamPixels> origin, int paletteBits) =>
        Ensure.NotNull(sprites).AddImage(Ensure.NotNull(sheet).Frame(TitleSpriteDefinitions.FrameName(sprite)), origin,
            FrontendSheetPalette.Override(checked((ushort)paletteBits)));

    /// <summary>Validates and writes an editable title Mode 7 map document as UTF-8 JSON.</summary>
    /// <param name="json">The destination stream.</param>
    /// <param name="document">The map and sprite-composition document to serialize.</param>
    public static void WriteMap(Stream json, TitleFloorMapDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        TitleFloorMapDocument restored;
        try
        {
            restored = JsonAssetDocument.Read<TitleFloorMapDocument>(bytes, ArtworkJson.Options)
                ?? throw new InvalidDataException("Title Mode 7 map is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid title Mode 7 map JSON.", error);
        }
        if (restored.Version != TitleGraphicsFormat.Version ||
            restored.Width != TitleGraphicsFormat.MapWidth ||
            restored.Height != TitleGraphicsFormat.MapHeight ||
            restored.Tiles is null ||
            restored.Tiles.Length != TitleGraphicsFormat.MapWidth * TitleGraphicsFormat.MapHeight ||
            restored.Tiles.Any(tile => (uint)tile > byte.MaxValue))
            throw new InvalidDataException("Invalid title Mode 7 map document.");
        _ = CompileSprites(restored.Sprites);
        json.Write(bytes);
    }

    /// <summary>Validates the full title composition set and compiles each editable part into OAM words.</summary>
    private static Dictionary<TitleSprite, SpriteComposition> CompileSprites(TitleSpriteFrame[]? frames)
    {
        if (frames is null || frames.Length != TitleGraphicsFormat.SpriteFrameCount)
            throw new InvalidDataException("Title composition requires all 31 title sprite frames.");
        var result = new Dictionary<TitleSprite, SpriteComposition>();
        foreach (TitleSpriteFrame frame in frames)
        {
            if (frame is null || !Enum.IsDefined(frame.Sprite) || frame.Parts is null ||
                frame.Parts.Length > TitleGraphicsFormat.MaximumSpriteParts)
                throw new InvalidDataException("Title composition has an undefined frame name or an invalid part count.");
            var parts = new CompiledSpritePart[frame.Parts.Length];
            for (int index = 0; index < parts.Length; index++)
            {
                SpriteVisualPart part = frame.Parts[index];
                if (part is null || part.OffsetX is < -256 or > 255 || part.OffsetY is < -128 or > 127 ||
                    part.Size is not (8 or 16) || part.Priority is < 0 or > 3 || part.Palette is not null ||
                    part.TileColumn < 0 || part.TileColumn >= TitleGraphicsFormat.ObjectTileColumns ||
                    part.TileRow < 0 || part.TileRow >= TitleGraphicsFormat.ObjectTileRows ||
                    part.TileColumn % 16 > 16 - part.Size / 8 ||
                    part.TileRow > TitleGraphicsFormat.ObjectTileRows - part.Size / 8)
                    throw new InvalidDataException($"Title frame {frame.Sprite} part {index} is invalid.");
                int tile = part.TileRow * TitleGraphicsFormat.ObjectTileColumns + part.TileColumn;
                var attributes = SnesObjAttributeWord.Create(tile, 0, part.Priority,
                    (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (part.FlipY ? SnesTileFlipFlags.Vertical : 0));
                parts[index] = new(SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                    new SnesSpritemapYByte(unchecked((byte)part.OffsetY)), attributes, InheritPalette: true);
            }
            if (!result.TryAdd(frame.Sprite, new SpriteComposition(parts)))
                throw new InvalidDataException($"Duplicate title frame {frame.Sprite}.");
        }
        // A correctly sized document can still repeat one composition in place of
        // another. Validate the engine's whole composition set now, before any title
        // frame runs, rather than failing later in DrawSprite.
        foreach (TitleSprite sprite in TitleSpriteDefinitions.All)
            if (!result.ContainsKey(sprite))
                throw new InvalidDataException(
                    $"Title composition is missing required frame {sprite}; " +
                    "keep the frame names when editing visual parts.");
        return result;
    }
}
