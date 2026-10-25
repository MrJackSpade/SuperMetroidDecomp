using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Immutable compiled map-screen layers authored as named JSON tile grids.</summary>
public sealed class MapScreenPresentation
{
    /// <summary>Validated named map pages stored as little-endian SNES tilemap words.</summary>
    private readonly Dictionary<MapScreenPageId, byte[]> pages;
    /// <summary>Takes ownership of the compiled page buffers after load-time validation.</summary>
    private MapScreenPresentation(Dictionary<MapScreenPageId, byte[]> pages) => this.pages = pages;

    /// <summary>Copies the named page into a screen's own tilemap words (#1358).</summary>
    internal void LoadTo(Span<ushort> destination, MapScreenPageId page) => SuperMetroid.Core.Frontend.Menus.MenuTilemaps.Load(destination, pages[page]);
    /// <summary>Loads and validates the complete set of named map-screen pages from JSON.</summary>
    public static MapScreenPresentation Load(Stream json)
    {
        MapScreenDocument document;
        try { document = JsonAssetDocument.Read<MapScreenDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Map-screen document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map-screen JSON.", error); }
        if (document.Version != MapScreenDefinitions.Version || document.Pages is null || document.Pages.Count != MapScreenDefinitions.PageCount)
            throw new InvalidDataException("Map screens require version 1 and all thirteen named pages.");
        var pages = new Dictionary<MapScreenPageId, byte[]>();
        foreach (var definition in MapScreenDefinitions.Pages())
        {
            if (!document.Pages.TryGetValue(definition.Id.Name, out var cells) || cells is null || cells.Length != MapScreenDefinitions.PageCells)
                throw new InvalidDataException($"Map-screen {definition.Id.Name} requires 1024 cells in 32-column row order.");
            var bytes = new byte[MapScreenDefinitions.PageBytes];
            for (int index = 0; index < cells.Length; index++)
            {
                var cell = cells[index];
                if (cell is null || (uint)cell.TileColumn >= definition.TileColumns ||
                    (uint)cell.TileRow >= definition.TileCount / definition.TileColumns || (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                    throw new InvalidDataException($"Map-screen {definition.Id.Name} cell {index} has an invalid atlas coordinate or palette.");
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), (ushort)(cell.TileRow * definition.TileColumns + cell.TileColumn |
                    cell.Palette << MapPresentationFormat.PaletteShift |
                    (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
                    (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) |
                    (cell.FlipY ? MapPresentationFormat.FlipYBit : 0)));
            }
            pages.Add(definition.Id, bytes);
        }
        return new(pages);
    }
    /// <summary>Validates and writes a map-screen document as camel-case JSON.</summary>
    public static void Write(Stream output, MapScreenDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}
