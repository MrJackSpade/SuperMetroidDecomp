using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// Editable title character artwork, Mode 7 tile layout, and ordered OBJ compositions.
/// Motion, animation page order, and VRAM destinations remain compiled behavior.
/// </summary>
public sealed class TitleGraphicsPresentation
{
    /// <summary>Owned Mode-7 title characters and map, OBJ characters, and baby character page.</summary>
    private readonly byte[] mode7Characters, mode7Map, objectCharacters, babyCharacters;
    /// <summary>Compiled OAM compositions keyed by cartridge-selected bank-$8C pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> sprites;

    /// <summary>Retains validated title transfer bytes and compiled sprite compositions.</summary>
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

    /// <summary>Eight-bit Mode-7 character lane uploaded for the title backdrop.</summary>
    public ReadOnlySpan<byte> Mode7Characters => mode7Characters;
    /// <summary>64-by-64 title map of Mode-7 character indexes.</summary>
    public ReadOnlySpan<byte> Mode7Map => mode7Map;
    /// <summary>Four-bit title OBJ character graphics selected by sprite compositions.</summary>
    public ReadOnlySpan<byte> ObjectCharacters => objectCharacters;
    /// <summary>Separate eight-bit Mode-7 character page for the baby Metroid scene.</summary>
    public ReadOnlySpan<byte> BabyCharacters => babyCharacters;

    /// <summary>Draws one ROM-selected title frame from installed composition data.</summary>
    public void DrawSprite(ushort pointer, OamBuffer oam, Position16<OamPixels> origin, ushort paletteBits)
    {
        if (!sprites.TryGetValue(pointer, out SpriteComposition? composition))
            throw new InvalidDataException($"Title spritemap $8C:{pointer:X4} is missing from installed artwork.");
        composition.DrawOnScreen(oam, origin, paletteBits);
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
            new(TitleGraphicsFormat.Mode7Width, TitleGraphicsFormat.Mode7Height));
        IndexedPngImage objects = IndexedPng.Read(
            objectTilesPng,
            new(TitleGraphicsFormat.ObjectWidth, TitleGraphicsFormat.ObjectHeight));
        IndexedPngImage baby = IndexedPng.Read(
            babyTilesPng,
            new(TitleGraphicsFormat.BabyWidth, TitleGraphicsFormat.BabyHeight));
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
            SnesMode7TileEncoder.Encode(mode7.Pixels, mode7.Size),
            map.Tiles.Select(tile => checked((byte)tile)).ToArray(),
            SnesPlanarTileEncoder.Encode(objects.Pixels, objects.Size, 4),
            SnesMode7TileEncoder.Encode(baby.Pixels, baby.Size),
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

    /// <summary>Validates the full native title-frame pointer set and compiles each editable part into OAM words.</summary>
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
