using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>
/// The installed title artwork as the import pipeline reads it (#1409, #1463): the ordered OBJ compositions, the Mode 7
/// floor and Baby Metroid characters as chunky 8-bit pixels and the OBJ characters as SNES 4-bpp planar bytes, which the
/// title sheets compose from.
/// </summary>
public sealed class TitleGraphicsCharacters : TitleGraphicsPresentation
{
    /// <summary>Compiled OAM compositions keyed by title composition.</summary>
    private readonly Dictionary<TitleSprite, SpriteComposition> sprites;

    /// <summary>Owned Mode 7 floor characters, OBJ characters, and Baby Metroid characters.</summary>
    private readonly byte[] floorCharacters, objectCharacters, babyCharacters;

    /// <summary>Retains the compiled characters beside the validated map and compositions.</summary>
    /// <param name="map">The validated Mode 7 map and OBJ compositions.</param>
    /// <param name="floorCharacters">The chunky Mode 7 floor characters.</param>
    /// <param name="objectCharacters">The planar 4-bpp OBJ characters.</param>
    /// <param name="babyCharacters">The chunky Baby Metroid Mode 7 characters.</param>
    private TitleGraphicsCharacters(TitleFloorMapDocument map, byte[] floorCharacters, byte[] objectCharacters, byte[] babyCharacters)
        : base(map.Tiles.Select(tile => checked((byte)tile)).ToArray())
    {
        sprites = CompileSprites(map.Sprites);
        this.floorCharacters = floorCharacters;
        this.objectCharacters = objectCharacters;
        this.babyCharacters = babyCharacters;
    }

    /// <summary>Eight-bit Mode-7 character lane uploaded for the title backdrop.</summary>
    public ReadOnlySpan<byte> FloorCharacters => floorCharacters;

    /// <summary>Separate eight-bit Mode-7 character page for the baby Metroid scene.</summary>
    public ReadOnlySpan<byte> BabyCharacters => babyCharacters;

    /// <summary>The title OBJ page the compositions draw from: the OBJ sheet as the title uploads it (#1358).</summary>
    public SpriteArtwork ObjectPage => field ??= new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.TitleObjects),
        objectCharacters.AsSpan(0, Math.Min(TitleSequenceDefinitions.Graphics.ObjectCharacterByteCount, objectCharacters.Length)));

    /// <summary>One title composition as a whole frame over the title OBJ page (#1392); its parts take their draw's palette.</summary>
    internal SpriteFrame Frame(TitleSprite sprite, string name) => sprites.TryGetValue(sprite, out SpriteComposition? composition)
        ? composition.ToFrame(name, 0) : throw new InvalidDataException($"Title composition {sprite} is missing from installed artwork.");

    /// <summary>Loads and validates the editable title graphics and map assets.</summary>
    /// <param name="floorTilesPng">Indexed 256-color PNG containing the title Mode 7 characters.</param>
    /// <param name="floorMapJson">JSON document containing the Mode 7 map and OBJ compositions.</param>
    /// <param name="objectTilesPng">Indexed 16-color PNG containing title OBJ characters.</param>
    /// <param name="babyTilesPng">Indexed 256-color PNG containing baby Metroid Mode 7 characters.</param>
    /// <returns>The compiled title graphics presentation.</returns>
    /// <exception cref="InvalidDataException">A PNG, the map or a composition is invalid.</exception>
    public static TitleGraphicsCharacters Read(
        Stream floorTilesPng,
        Stream floorMapJson,
        Stream objectTilesPng,
        Stream babyTilesPng)
    {
        IndexedPngImage floorImage = IndexedPng.Read(
            floorTilesPng,
            new(TitleGraphicsFormat.FloorWidth, TitleGraphicsFormat.FloorHeight));
        IndexedPngImage objects = IndexedPng.Read(
            objectTilesPng,
            new(TitleGraphicsFormat.ObjectWidth, TitleGraphicsFormat.ObjectHeight));
        IndexedPngImage baby = IndexedPng.Read(
            babyTilesPng,
            new(TitleGraphicsFormat.BabyWidth, TitleGraphicsFormat.BabyHeight));
        if (floorImage.Colours.Length != 256 || baby.Colours.Length != 256 || objects.Colours.Length != 16)
            throw new InvalidDataException("Title graphics require 256-color Mode 7/Baby PNGs and a 16-color OBJ PNG.");
        byte[] floorCharacters = SnesMode7TileEncoder.Encode(floorImage.Pixels, floorImage.Size);
        byte[] objectCharacters = SnesPlanarTileEncoder.Encode(objects.Pixels, objects.Size, 4);
        byte[] babyCharacters = SnesMode7TileEncoder.Encode(baby.Pixels, baby.Size);
        return new TitleGraphicsCharacters(ReadMap(floorMapJson), floorCharacters, objectCharacters, babyCharacters);
    }

    /// <summary>Reads and validates the title's Mode 7 map document.</summary>
    /// <param name="floorMapJson">JSON document containing the Mode 7 map and OBJ compositions.</param>
    /// <exception cref="InvalidDataException">The map is invalid.</exception>
    private static TitleFloorMapDocument ReadMap(Stream floorMapJson)
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
        return map;
    }

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
                parts[index] = new(SpritePart.Create(Offset16<ScreenPixels>.Of(part.OffsetX, part.OffsetY), part.Size == 16, tile, 0, part.Priority,
                    (part.FlipX ? TileFlip.Horizontal : 0) |
                    (part.FlipY ? TileFlip.Vertical : 0)), InheritPalette: true);
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

    /// <summary>The characters <paramref name="graphics"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The presentation was not read from installed artwork.</exception>
    public static TitleGraphicsCharacters Of(TitleGraphicsPresentation graphics) => graphics as TitleGraphicsCharacters
        ?? throw new InvalidDataException("The title graphics were not read from installed artwork.");
}
