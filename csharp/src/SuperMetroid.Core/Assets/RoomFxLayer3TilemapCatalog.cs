using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable BG3 tile references for the six cartridge room-FX pages.</summary>
public sealed class RoomFxLayer3TilemapCatalog
{
    private readonly byte[] lava, acid, water, spores, rain, fog;

    private RoomFxLayer3TilemapCatalog(byte[] lava, byte[] acid, byte[] water,
        byte[] spores, byte[] rain, byte[] fog) =>
        (this.lava, this.acid, this.water, this.spores, this.rain, this.fog) =
        (lava, acid, water, spores, rain, fog);

    /// <summary>Compiles named 32x33 pages to the original ordered VRAM transfer words.</summary>
    public static RoomFxLayer3TilemapCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        RoomFxLayer3TilemapDocument document = JsonAssetDocument.Read<RoomFxLayer3TilemapDocument>(
            json, JsonOptions, "room-FX BG3 tilemap");
        if (document.Version != RoomFxLayer3TilemapFormat.Version ||
            document.Pages is null ||
            document.Pages.Count != RoomFxLayer3TilemapFormat.Types.Count)
            throw new InvalidDataException(
                "Room-FX BG3 tilemaps require the supported version and six named pages.");

        return new(Compile(RoomFxType.Lava), Compile(RoomFxType.Acid), Compile(RoomFxType.Water),
            Compile(RoomFxType.Spores), Compile(RoomFxType.Rain), Compile(RoomFxType.Fog));

        byte[] Compile(RoomFxType type)
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
            return bytes;
        }
    }

    /// <summary>Validates and writes a complete editable tilemap document.</summary>
    public static void Write(Stream json, RoomFxLayer3TilemapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>
    /// Selects one of the six named editable 33-row resources directly. The six
    /// even identities $02..0C match the original $83:ABF2..ABFC page dispatch;
    /// their payload is compiled from the supplied document, not stock-only data.
    /// Other ushort type identities throw InvalidDataException.
    /// </summary>
    public ReadOnlyMemory<byte> Resolve(RoomFxType type) => type switch
    {
        RoomFxType.Lava => lava,
        RoomFxType.Acid => acid,
        RoomFxType.Water => water,
        RoomFxType.Spores => spores,
        RoomFxType.Rain => rain,
        RoomFxType.Fog => fog,
        _ => throw new InvalidDataException($"Room-FX BG3 tilemap {type} is not an authored page."),
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
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

    /// <summary>
    /// The six even type identities $02..0C at $83:ABF2..ABFC, in page order.
    /// Index0..5 maps to 2*(index+1); enumeration computes values without a roster.
    /// </summary>
    public static IReadOnlyList<RoomFxType> Types { get; } = new CalculatedTypes();

    private sealed class CalculatedTypes : IReadOnlyList<RoomFxType>
    {
        public int Count => 6;
        public RoomFxType this[int index] => (uint)index < Count
            ? (RoomFxType)(2 * (index + 1))
            : throw new ArgumentOutOfRangeException(nameof(index));

        public IEnumerator<RoomFxType> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Returns $8A:8000 + (type/2-1)*$840 for the six even types $02..0C.
    /// The original pointers at $83:ABF2..ABFC select consecutive 32x33 word pages.
    /// Every other ushort type remains unsupported, including the separate statue alias.
    /// </summary>
    public static int SourceAddress(RoomFxType type)
    {
        int value = (ushort)type;
        if (value is < 2 or > 12 || (value & 1) != 0)
            throw new InvalidDataException($"Room-FX type {type} has no BG3 tilemap page.");
        return FirstSourceAddress + (value / 2 - 1) * PageByteCount;
    }
}
