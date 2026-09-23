using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable BG3 tile references for the six cartridge room-FX pages.</summary>
public sealed class RoomFxLayer3TilemapCatalog
{
    private readonly Dictionary<RoomFxType, byte[]> pages;

    private RoomFxLayer3TilemapCatalog(Dictionary<RoomFxType, byte[]> pages) => this.pages = pages;

    /// <summary>Compiles named 32x33 pages to the original ordered VRAM transfer words.</summary>
    public static RoomFxLayer3TilemapCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        RoomFxLayer3TilemapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<RoomFxLayer3TilemapDocument>(json, JsonOptions)
                ?? throw new InvalidDataException("Room-FX BG3 tilemap JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid room-FX BG3 tilemap JSON.", error);
        }
        if (document.Version != RoomFxLayer3TilemapFormat.Version ||
            document.Pages is null ||
            document.Pages.Count != RoomFxLayer3TilemapFormat.Types.Count)
            throw new InvalidDataException(
                "Room-FX BG3 tilemaps require the supported version and six named pages.");

        var pages = new Dictionary<RoomFxType, byte[]>();
        foreach (RoomFxType type in RoomFxLayer3TilemapFormat.Types)
        {
            if (!document.Pages.TryGetValue(type.ToString(), out RoomBackgroundTilemapCell[]? cells) ||
                cells is null || cells.Length != RoomFxLayer3TilemapFormat.CellsPerPage)
                throw new InvalidDataException(
                    $"Room-FX BG3 {type} must contain {RoomFxLayer3TilemapFormat.CellsPerPage} cells.");
            var bytes = new byte[RoomFxLayer3TilemapFormat.PageByteCount];
            for (int cell = 0; cell < cells.Length; cell++)
            {
                RoomBackgroundTilemapCell? value = cells[cell];
                if (value is null ||
                    (uint)value.TileColumn >= RoomBackgroundTilemapFormat.TileColumns ||
                    (uint)value.TileRow >= RoomBackgroundTilemapFormat.TileRows ||
                    (uint)value.Palette >= RoomBackgroundTilemapFormat.PaletteCount)
                    throw new InvalidDataException(
                        $"Room-FX BG3 {type} cell {cell} has an invalid tile or palette.");
                SnesTileFlipFlags flips =
                    (value.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (value.FlipY ? SnesTileFlipFlags.Vertical : 0);
                ushort word = SnesBgTilemapWord.Create(
                    value.TileRow * RoomBackgroundTilemapFormat.TileColumns + value.TileColumn,
                    value.Palette, value.Priority, flips).Raw;
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(cell * sizeof(ushort)), word);
            }
            pages.Add(type, bytes);
        }
        return new(pages);
    }

    /// <summary>Validates and writes a complete editable tilemap document.</summary>
    public static void Write(Stream json, RoomFxLayer3TilemapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Returns the full 33-row native transfer for one room-FX type.</summary>
    public ReadOnlyMemory<byte> Resolve(RoomFxType type) => pages.TryGetValue(type, out byte[]? bytes)
        ? bytes
        : throw new InvalidDataException($"Room-FX BG3 tilemap {type} is not an authored page.");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}

public sealed record RoomFxLayer3TilemapDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, RoomBackgroundTilemapCell[]> Pages { get; init; }
}

/// <summary>Native bank-$8A 32x33 BG3 effect-page geometry and presentation identities.</summary>
public static class RoomFxLayer3TilemapFormat
{
    public const string FileName = "room-fx-layer3-tilemaps.json";
    public const int Version = 1;
    public const int WidthInTiles = 32;
    public const int HeightInTiles = 33;
    public const int CellsPerPage = WidthInTiles * HeightInTiles;
    public const int PageByteCount = CellsPerPage * sizeof(ushort);

    /// <summary>$8A:8000, first room-FX BG3 page; the six pages end at $8A:B17F.</summary>
    public const int FirstSourceAddress = 0x8a8000;

    private static readonly RoomFxType[] AuthoredTypes =
    [
        RoomFxType.Lava,
        RoomFxType.Acid,
        RoomFxType.Water,
        RoomFxType.Spores,
        RoomFxType.Rain,
        RoomFxType.Fog,
    ];

    public static IReadOnlyList<RoomFxType> Types { get; } = Array.AsReadOnly(AuthoredTypes);

    /// <summary>Returns the cartridge art source corresponding to a named effect page.</summary>
    public static int SourceAddress(RoomFxType type)
    {
        int index = Array.IndexOf(AuthoredTypes, type);
        if (index < 0)
            throw new InvalidDataException($"Room-FX type {type} has no BG3 tilemap page.");
        return FirstSourceAddress + index * PageByteCount;
    }
}
