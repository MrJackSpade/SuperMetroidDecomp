using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Indexed artwork and ordered compositions for map sprites; palette inheritance retains live animation.</summary>
public sealed class MapSpriteCatalog
{
    private readonly Dictionary<ushort, SpriteComposition> frames;
    private readonly byte[] characters;
    private MapSpriteCatalog(Dictionary<ushort, SpriteComposition> frames, byte[] characters) { this.frames = frames; this.characters = characters; }
    public void Draw(ushort id, OamBuffer oam, ushort x, ushort y, ushort paletteBits) => frames[id].DrawOnScreen(oam, x, y, paletteBits);
    public void LoadArtworkTo(SnesVram vram, int destinationByte) => vram.LoadBytes(destinationByte, characters);
    public static MapSpriteCatalog Load(Stream json, Stream png)
    {
        MapSpriteDocument document;
        try { document = JsonSerializer.Deserialize<MapSpriteDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map sprite document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map sprite JSON.", error); }
        if (document.Version != MapSpriteFormat.Version || document.Frames is null || document.Frames.Count != MapSpriteDefinitions.Frames.Length)
            throw new InvalidDataException("Map sprite content requires version 1 and all 26 named frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (var definition in MapSpriteDefinitions.Frames)
        {
            if (!document.Frames.TryGetValue(definition.Name, out var parts) || parts is null || parts.Length > MapSpriteFormat.MaximumParts)
                throw new InvalidDataException($"Map sprite {definition.Name} requires an ordered array of at most 128 parts.");
            var compiled = new CompiledSpritePart[parts.Length];
            for (int index = 0; index < parts.Length; index++)
            {
                var part = parts[index];
                if (part is null || part.OffsetX is < -256 or > 255 || part.OffsetY is < -128 or > 127 ||
                    part.Size is not (8 or 16) || (uint)part.Priority > 3 || part.Palette is < 0 or > 7 ||
                    part.TileColumn < 0 || part.TileRow < 0 || part.TileColumn > MapSpriteFormat.TileColumns - part.Size / 8 ||
                    part.TileRow > MapSpriteFormat.TileRows - part.Size / 8)
                    throw new InvalidDataException($"Map sprite {definition.Name} part {index} has an invalid offset, region, size, priority or palette.");
                var attributes = SnesObjAttributeWord.Create(part.TileRow * MapSpriteFormat.TileColumns + part.TileColumn,
                    part.Palette ?? 0, part.Priority, (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (part.FlipY ? SnesTileFlipFlags.Vertical : 0));
                compiled[index] = new(SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16), unchecked((byte)(sbyte)part.OffsetY), attributes, part.Palette is null);
            }
            frames.Add(definition.NativeId, new SpriteComposition(compiled));
        }
        var image = IndexedPng.Read(png, MapSpriteFormat.Width, MapSpriteFormat.Height);
        return new(frames, SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4));
    }
}
public sealed record MapSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
public static class MapSpriteFormat
{
    public const int Version = 1;
    public const string JsonFile = "map-sprites.json", PngFile = "map-objects.png";
    public const int TileColumns = 16, TileRows = 16;
    public const int Width = TileColumns * 8, Height = TileRows * 8;
    public const int MaximumParts = 128;
    /// <summary>$B6:C000 shared menu object characters, transferred to the active menu's OBJ base.</summary>
    public const int SourceAddress = 0xb6c000;
    public const int ByteCount = Width * Height / 2;
    /// <summary>File-select OBSEL=$03 places shared menu OBJ characters at VRAM byte $C000.</summary>
    public const int FileSelectDestination = 0xc000;
    /// <summary>Pause OBSEL=$01 places the same shared characters at VRAM byte $4000.</summary>
    public const int PauseDestination = 0x4000;
}
