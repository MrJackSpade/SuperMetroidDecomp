using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable title character artwork, Mode 7 tile layout, and ordered OBJ compositions.
/// Motion, animation page order, and VRAM destinations remain compiled behavior.
/// </summary>
public sealed class TitleGraphicsPresentation
{
    /// <summary>Stores the encoded title Mode 7 characters, title map, OBJ characters, and baby Metroid characters used by the renderer.</summary>
    private readonly byte[] mode7Characters, mode7Map, objectCharacters, babyCharacters;
    /// <summary>Maps cartridge-selected spritemap pointers to their validated, compiled OBJ compositions.</summary>
    private readonly Dictionary<ushort, SpriteComposition> sprites;

    /// <summary>Creates a presentation from encoded graphics, a byte-indexed map, and compiled sprite compositions.</summary>
    /// <param name="mode7Characters">Encoded eight-bit title Mode 7 character data.</param>
    /// <param name="mode7Map">Byte indexes selecting title Mode 7 characters for each map cell.</param>
    /// <param name="objectCharacters">Encoded four-bit OBJ character graphics for title sprites.</param>
    /// <param name="babyCharacters">Encoded eight-bit Mode 7 character data for the baby Metroid page.</param>
    /// <param name="sprites">Compositions indexed by the native spritemap pointers used to select title frames.</param>
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

    /// <summary>Gets the encoded eight-bit Mode 7 character lane for the title artwork.</summary>
    public ReadOnlySpan<byte> Mode7Characters => mode7Characters;
    /// <summary>Gets the 64-by-64 byte-indexed Mode 7 title map.</summary>
    public ReadOnlySpan<byte> Mode7Map => mode7Map;
    /// <summary>Gets the encoded four-bit title OBJ character graphics.</summary>
    public ReadOnlySpan<byte> ObjectCharacters => objectCharacters;
    /// <summary>Gets the encoded eight-bit Mode 7 baby Metroid character page.</summary>
    public ReadOnlySpan<byte> BabyCharacters => babyCharacters;

    /// <summary>Draws one ROM-selected title frame from installed composition data.</summary>
    public void DrawSprite(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        if (!sprites.TryGetValue(pointer, out SpriteComposition? composition))
            throw new InvalidDataException($"Title spritemap $8C:{pointer:X4} is missing from installed artwork.");
        composition.DrawOnScreen(oam, x, y, paletteBits);
    }

    /// <summary>Loads and validates the editable title graphics and map assets.</summary>
    /// <param name="mode7TilesPng">Indexed 256-color PNG containing the title Mode 7 characters.</param>
    /// <param name="mode7MapJson">JSON document containing the Mode 7 map and OBJ compositions.</param>
    /// <param name="objectTilesPng">Indexed 16-color PNG containing title OBJ characters.</param>
    /// <param name="babyTilesPng">Indexed 256-color PNG containing baby Metroid Mode 7 characters.</param>
    /// <returns>The compiled title graphics presentation.</returns>
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
            map = JsonAssetDocument.Read<TitleMode7MapDocument>(
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

    /// <summary>Validates and writes an editable title Mode 7 map document as UTF-8 JSON.</summary>
    /// <param name="json">The destination stream.</param>
    /// <param name="document">The map and sprite-composition document to serialize.</param>
    public static void WriteMap(Stream json, TitleMode7MapDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        TitleMode7MapDocument restored;
        try
        {
            restored = JsonAssetDocument.Read<TitleMode7MapDocument>(bytes, MapPresentationFormat.JsonOptions)
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

    /// <summary>Validates editable sprite frames and compiles their ordered parts into compositions keyed by native pointers.</summary>
    /// <param name="frames">Sprite-frame records from the map document.</param>
    /// <returns>Validated compositions for all native-selected title frames.</returns>
    /// <exception cref="InvalidDataException">The frame set is incomplete, contains invalid parts or duplicate pointers, or omits a required native selector.</exception>
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
        // A correctly sized document can still replace a required selector with an
        // unrelated identity. Validate the engine's whole selection set now, before
        // any title frame runs, rather than failing later in DrawSprite.
        foreach (ushort pointer in TitleSpriteDefinitions.NativePointers)
            if (!result.ContainsKey(pointer))
                throw new InvalidDataException(
                    $"Title composition is missing required frame $8C:{pointer:X4}; " +
                    "keep the compiled frame identities when editing visual parts.");
        return result;
    }
}

/// <summary>Serializable title Mode 7 map and native-selected sprite compositions.</summary>
public sealed record TitleMode7MapDocument
{
    /// <summary>Gets the title-graphics document schema version.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the Mode 7 map width in cells.</summary>
    public required int Width { get; init; }
    /// <summary>Gets the Mode 7 map height in cells.</summary>
    public required int Height { get; init; }
    /// <summary>Gets the row-major eight-bit Mode 7 character indexes.</summary>
    public required int[] Tiles { get; init; }
    /// <summary>Gets every native-selected title OBJ frame.</summary>
    public required TitleSpriteFrame[] Sprites { get; init; }
}

/// <summary>One ROM-selected title OBJ frame; all parts inherit the active title palette.</summary>
public sealed record TitleSpriteFrame
{
    /// <summary>Gets the bank-$8C spritemap pointer used to select this frame.</summary>
    public required int Pointer { get; init; }
    /// <summary>Gets the ordered OBJ parts composing the frame.</summary>
    public required SpriteVisualPart[] Parts { get; init; }
}

/// <summary>Schema, dimensions, filenames, and validation limits for editable title graphics.</summary>
public static class TitleGraphicsFormat
{
    /// <summary>Current title-graphics document schema version.</summary>
    public const int Version = 2;
    /// <summary>Number of cartridge-selected title sprite frames.</summary>
    public const int SpriteFrameCount = 31;
    /// <summary>Maximum OBJ parts accepted in one title sprite frame.</summary>
    public const int MaximumSpriteParts = 128;
    /// <summary>Width of the title OBJ character sheet in eight-pixel tiles.</summary>
    public const int ObjectTileColumns = 32;
    /// <summary>Height of the title OBJ character sheet in eight-pixel tiles.</summary>
    public const int ObjectTileRows = 16;
    /// <summary>Installed filename of the title Mode 7 character PNG.</summary>
    public const string Mode7TilesFile = "title-mode7-tiles.png";
    /// <summary>Installed filename of the title Mode 7 map JSON document.</summary>
    public const string Mode7MapFile = "title-mode7-map.json";
    /// <summary>Installed filename of the title OBJ character PNG.</summary>
    public const string ObjectTilesFile = "title-object-tiles.png";
    /// <summary>Installed filename of the baby Metroid Mode 7 character PNG.</summary>
    public const string BabyTilesFile = "title-baby-tiles.png";
    /// <summary>Width in pixels of the title Mode 7 character image.</summary>
    public const int Mode7Width = 128;
    /// <summary>Height in pixels of the title Mode 7 character image.</summary>
    public const int Mode7Height = 128;
    /// <summary>Width in cells of the title Mode 7 map.</summary>
    public const int MapWidth = 64;
    /// <summary>Height in cells of the title Mode 7 map.</summary>
    public const int MapHeight = 64;
    /// <summary>Width in pixels of the title OBJ character image.</summary>
    public const int ObjectWidth = 256;
    /// <summary>Height in pixels of the title OBJ character image.</summary>
    public const int ObjectHeight = 128;
    /// <summary>Width in pixels of the baby Metroid Mode 7 character image.</summary>
    public const int BabyWidth = 32;
    /// <summary>Height in pixels of the baby Metroid Mode 7 character image.</summary>
    public const int BabyHeight = 32;
}
