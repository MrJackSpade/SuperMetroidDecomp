using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable title character artwork, Mode 7 tile layout, and ordered OBJ compositions.
/// Motion, animation page order, and VRAM destinations remain compiled behavior.
/// </summary>
public sealed class TitleGraphicsPresentation
{
    private readonly byte[] mode7Characters, mode7Map, objectCharacters, babyCharacters;
    private readonly Dictionary<ushort, SpriteComposition> sprites;

    private TitleGraphicsPresentation(
        byte[] mode7Characters,
        byte[] mode7Map,
        byte[] objectCharacters,
        byte[] babyCharacters,
        Dictionary<ushort, SpriteComposition> sprites)
    {
        this.mode7Characters = mode7Characters;
        this.mode7Map = mode7Map;
        this.objectCharacters = objectCharacters;
        this.babyCharacters = babyCharacters;
        this.sprites = sprites;
    }

    public ReadOnlySpan<byte> Mode7Characters => mode7Characters;
    public ReadOnlySpan<byte> Mode7Map => mode7Map;
    public ReadOnlySpan<byte> ObjectCharacters => objectCharacters;
    public ReadOnlySpan<byte> BabyCharacters => babyCharacters;

    /// <summary>Draws one ROM-selected title frame from installed composition data.</summary>
    public void DrawSprite(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (!sprites.TryGetValue(pointer, out SpriteComposition? composition))
            throw new InvalidDataException($"Title spritemap $8C:{pointer:X4} is missing from installed artwork.");
        composition.DrawOnScreen(oam, x, y, paletteBits);
    }

    public static TitleGraphicsPresentation Load(
        Stream mode7TilesPng,
        Stream mode7MapJson,
        Stream objectTilesPng,
        Stream babyTilesPng)
    {
        IndexedPngImage mode7 = IndexedPng.Read(
            mode7TilesPng,
            TitleGraphicsFormat.Mode7Width,
            TitleGraphicsFormat.Mode7Height);
        IndexedPngImage objects = IndexedPng.Read(
            objectTilesPng,
            TitleGraphicsFormat.ObjectWidth,
            TitleGraphicsFormat.ObjectHeight);
        IndexedPngImage baby = IndexedPng.Read(
            babyTilesPng,
            TitleGraphicsFormat.BabyWidth,
            TitleGraphicsFormat.BabyHeight);
        if (mode7.Palette.Length != 256 || baby.Palette.Length != 256 || objects.Palette.Length != 16)
            throw new InvalidDataException("Title graphics require 256-color Mode 7/Baby PNGs and a 16-color OBJ PNG.");

        TitleMode7MapDocument map;
        try
        {
            map = JsonSerializer.Deserialize<TitleMode7MapDocument>(
                mode7MapJson,
                MapPresentationFormat.JsonOptions)
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

        Dictionary<ushort, SpriteComposition> sprites = CompileSprites(map.Sprites);

        return new TitleGraphicsPresentation(
            SnesMode7TileEncoder.Encode(mode7.Pixels, mode7.Width, mode7.Height),
            map.Tiles.Select(tile => checked((byte)tile)).ToArray(),
            SnesPlanarTileEncoder.Encode(objects.Pixels, objects.Width, objects.Height, 4),
            SnesMode7TileEncoder.Encode(baby.Pixels, baby.Width, baby.Height),
            sprites);
    }

    public static void WriteMap(Stream json, TitleMode7MapDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        TitleMode7MapDocument restored;
        try
        {
            restored = JsonSerializer.Deserialize<TitleMode7MapDocument>(bytes, MapPresentationFormat.JsonOptions)
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

    private static Dictionary<ushort, SpriteComposition> CompileSprites(TitleSpriteFrame[]? frames)
    {
        if (frames is null || frames.Length != TitleGraphicsFormat.SpriteFrameCount)
            throw new InvalidDataException("Title composition requires all 31 cartridge-selected sprite frames.");
        var result = new Dictionary<ushort, SpriteComposition>();
        foreach (TitleSpriteFrame frame in frames)
        {
            if (frame is null || frame.Pointer < 0x8000 || frame.Parts is null ||
                frame.Parts.Length > TitleGraphicsFormat.MaximumSpriteParts)
                throw new InvalidDataException("Title composition has an invalid frame pointer or part count.");
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
                    throw new InvalidDataException($"Title frame $8C:{frame.Pointer:X4} part {index} is invalid.");
                int tile = part.TileRow * TitleGraphicsFormat.ObjectTileColumns + part.TileColumn;
                var attributes = SnesObjAttributeWord.Create(tile, 0, part.Priority,
                    (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (part.FlipY ? SnesTileFlipFlags.Vertical : 0));
                parts[index] = new(SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                    unchecked((byte)(sbyte)part.OffsetY), attributes, InheritPalette: true);
            }
            if (!result.TryAdd(checked((ushort)frame.Pointer), new SpriteComposition(parts)))
                throw new InvalidDataException($"Duplicate title frame $8C:{frame.Pointer:X4}.");
        }
        return result;
    }
}

public sealed record TitleMode7MapDocument
{
    public required int Version { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required int[] Tiles { get; init; }
    public required TitleSpriteFrame[] Sprites { get; init; }
}

/// <summary>One ROM-selected title OBJ frame; all parts inherit the active title palette.</summary>
public sealed record TitleSpriteFrame
{
    public required int Pointer { get; init; }
    public required SpriteVisualPart[] Parts { get; init; }
}

public static class TitleGraphicsFormat
{
    public const int Version = 2;
    public const int SpriteFrameCount = 31;
    public const int MaximumSpriteParts = 128;
    public const int ObjectTileColumns = 32;
    public const int ObjectTileRows = 16;
    public const string Mode7TilesFile = "title-mode7-tiles.png";
    public const string Mode7MapFile = "title-mode7-map.json";
    public const string ObjectTilesFile = "title-object-tiles.png";
    public const string BabyTilesFile = "title-baby-tiles.png";
    public const int Mode7Width = 128;
    public const int Mode7Height = 128;
    public const int MapWidth = 64;
    public const int MapHeight = 64;
    public const int ObjectWidth = 256;
    public const int ObjectHeight = 128;
    public const int BabyWidth = 32;
    public const int BabyHeight = 32;
}
