using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// Editable BG3 tiles for the six room-FX pages, as their installed document places them. Only the import reads them: it
/// pictures each page once into the BG3 sheets (#1466), and the game names the page an effect shows (#1467).
/// </summary>
public sealed class RoomFxLayer3Pages
{
    /// <summary>Each authored page's tiles, row-major, by the effect that shows it.</summary>
    private readonly Dictionary<RoomFxType, BackgroundTile[]> pages;

    /// <summary>Keeps every authored page.</summary>
    private RoomFxLayer3Pages(Dictionary<RoomFxType, BackgroundTile[]> pages) => this.pages = pages;

    /// <summary>Validates the six named pages and places every cell's tile.</summary>
    /// <param name="json">The document stream, read from its current position.</param>
    /// <exception cref="InvalidDataException">The version, a page or a cell is invalid.</exception>
    public static RoomFxLayer3Pages Load(Stream json)
    {
        Ensure.NotNull(json);
        RoomFxLayer3TilemapDocument document = JsonAssetDocument.Read<RoomFxLayer3TilemapDocument>(
            json, JsonOptions, "room-FX BG3 tilemap");
        if (document.Version != RoomFxLayer3TilemapFormat.Version ||
            document.Pages is null ||
            document.Pages.Count != RoomFxLayer3TilemapFormat.Types.Count)
            throw new InvalidDataException(
                "Room-FX BG3 tilemaps require the supported version and six named pages.");

        return new(RoomFxLayer3TilemapFormat.Types.ToDictionary(type => type, Compile));

        BackgroundTile[] Compile(RoomFxType type)
        {
            if (!document.Pages.TryGetValue(type.ToString(), out RoomBackgroundTilemapCell[]? cells) ||
                cells is null || cells.Length != RoomFxLayer3TilemapFormat.CellsPerPage)
                throw new InvalidDataException(
                    $"Room-FX BG3 {type} must contain {RoomFxLayer3TilemapFormat.CellsPerPage} cells.");
            var tiles = new BackgroundTile[RoomFxLayer3TilemapFormat.CellsPerPage];
            for (int cell = 0; cell < cells.Length; cell++)
            {
                RoomBackgroundTilemapCell? value = cells[cell];
                if (value is null ||
                    (uint)value.TileColumn >= RoomBackgroundTilemapFormat.TileColumns ||
                    (uint)value.TileRow >= RoomBackgroundTilemapFormat.TileRows ||
                    (uint)value.Palette >= RoomBackgroundTilemapFormat.PaletteCount)
                    throw new InvalidDataException(
                        $"Room-FX BG3 {type} cell {cell} has an invalid tile or palette.");
                tiles[cell] = new BackgroundTile(value.TileRow * RoomBackgroundTilemapFormat.TileColumns + value.TileColumn,
                    value.Palette, value.FlipX, value.FlipY, value.Priority);
            }
            return tiles;
        }
    }

    /// <summary>Validates a document by loading it, then writes it as indented camel-case JSON.</summary>
    /// <param name="json">The destination stream.</param>
    /// <param name="document">The six named pages.</param>
    /// <exception cref="InvalidDataException">The document fails validation.</exception>
    public static void Write(Stream json, RoomFxLayer3TilemapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>The tiles of the page <paramref name="type"/> shows; the Tourian entrance statue room shows water's.</summary>
    /// <exception cref="InvalidDataException">The effect shows no authored page.</exception>
    public ReadOnlyMemory<BackgroundTile> Resolve(RoomFxType type) => type switch
    {
        RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water or RoomFxType.Spores or RoomFxType.Rain or RoomFxType.Fog => pages[type],
        RoomFxType.TourianEntranceStatue => pages[RoomFxType.Water],
        RoomFxType.None or
            RoomFxType.ScrollingSky or
            RoomFxType.UnusedScrollingSky or
            RoomFxType.Fireflea or
            RoomFxType.CeresRidley or
            RoomFxType.CeresElevator or
            RoomFxType.CeresHaze => throw new InvalidDataException($"Room-FX BG3 tilemap {type} is not an authored page."),
        _ => throw new InvalidDataException($"Room-FX BG3 tilemap {type} is not an authored page."),
    };

    /// <summary>Strict camel-case settings that reject unknown or duplicate page properties.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
