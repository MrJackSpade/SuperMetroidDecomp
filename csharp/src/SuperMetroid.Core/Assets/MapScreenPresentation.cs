using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable compiled map-screen layers authored as named JSON tile grids.</summary>
public sealed class MapScreenPresentation
{
    /// <summary>Compiled little-endian tilemap pages indexed by their canonical map-screen identifiers.</summary>
    private readonly Dictionary<string, byte[]> pages;

    /// <summary>Creates a presentation from the validated byte pages produced by <see cref="Load"/>.</summary>
    /// <param name="pages">Compiled 32-by-32 tilemap bytes keyed by page identifier.</param>
    private MapScreenPresentation(Dictionary<string, byte[]> pages) => this.pages = pages;
    /// <summary>Copies the named, compiled 32-by-32 tilemap page to the requested VRAM byte address.</summary>
    public void LoadTo(SnesVram vram, int destinationByteAddress, string page) => vram.LoadBytes(destinationByteAddress, pages[page]);
    /// <summary>Loads and validates the complete set of named map-screen pages from JSON.</summary>
    public static MapScreenPresentation Load(Stream json)
    {
        MapScreenDocument document;
        try { document = JsonAssetDocument.Read<MapScreenDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map-screen document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map-screen JSON.", error); }
        if (document.Version != MapScreenDefinitions.Version || document.Pages is null || document.Pages.Count != MapScreenDefinitions.PageCount)
            throw new InvalidDataException("Map screens require version 1 and all thirteen named pages.");
        var pages = new Dictionary<string, byte[]>();
        foreach (var definition in MapScreenDefinitions.Pages())
        {
            if (!document.Pages.TryGetValue(definition.Id, out var cells) || cells is null || cells.Length != MapScreenDefinitions.PageCells)
                throw new InvalidDataException($"Map-screen {definition.Id} requires 1024 cells in 32-column row order.");
            var bytes = new byte[MapScreenDefinitions.PageBytes];
            for (int index = 0; index < cells.Length; index++)
            {
                var cell = cells[index];
                if (cell is null || (uint)cell.TileColumn >= definition.TileColumns ||
                    (uint)cell.TileRow >= definition.TileCount / definition.TileColumns || (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
                    throw new InvalidDataException($"Map-screen {definition.Id} cell {index} has an invalid atlas coordinate or palette.");
                ushort word = (ushort)(cell.TileRow * definition.TileColumns + cell.TileColumn |
                    cell.Palette << MapPresentationFormat.PaletteShift |
                    (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
                    (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) |
                    (cell.FlipY ? MapPresentationFormat.FlipYBit : 0));
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), word);
            }
            pages.Add(definition.Id, bytes);
        }
        return new(pages);
    }
    /// <summary>Validates and writes a map-screen document as camel-case JSON.</summary>
    public static void Write(Stream output, MapScreenDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

/// <summary>Layout-only content: tile-sheet coordinates, palette selectors, priority and flips.</summary>
public sealed record MapScreenDocument
{
    /// <summary>Gets the map-screen schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets all named 32-by-32 map-screen tile grids.</summary>
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
}
