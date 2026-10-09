using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable BG3 tile references for the six cartridge room-FX pages.</summary>
public sealed class RoomFxLayer3TilemapCatalog
{
    /// <summary>Non-stock compiled transfer bytes for lava, acid, and water; null selects each effect's calculated stock layout.</summary>
    private readonly byte[]? lava, acid, water;

    /// <summary>Compiled spore-page transfer, retained because its authored layout is not generated from a stock liquid formula.</summary>
    private readonly RoomFxSporeTilemap spores;

    /// <summary>Compiled rain and fog transfers, each represented by its atmosphere tilemap definition.</summary>
    private readonly RoomFxAtmosphereTilemap rain, fog;

    /// <summary>Stores edited liquid transfers only when they differ from stock and prepares the authored spore and atmosphere pages.</summary>
    /// <param name="lava">Compiled lava page bytes, or the stock page bytes used to detect the calculated-layout case.</param>
    /// <param name="acid">Compiled acid page bytes, or the stock page bytes used to detect the calculated-layout case.</param>
    /// <param name="water">Compiled water page bytes, or the stock page bytes used to detect the calculated-layout case.</param>
    /// <param name="spores">Compiled spore page content.</param>
    /// <param name="rain">Compiled rain page content.</param>
    /// <param name="fog">Compiled fog page content.</param>
    private RoomFxLayer3TilemapCatalog(byte[] lava, byte[] acid, byte[] water,
        byte[] spores, byte[] rain, byte[] fog)
    {
        this.lava = RoomFxLiquidTilemapDefinitions.Matches(RoomFxType.Lava, lava) ? null : lava;
        this.acid = RoomFxLiquidTilemapDefinitions.Matches(RoomFxType.Acid, acid) ? null : acid;
        this.water = RoomFxLiquidTilemapDefinitions.Matches(RoomFxType.Water, water) ? null : water;
        this.spores = new RoomFxSporeTilemap(spores);
        this.rain = new RoomFxAtmosphereTilemap(RoomFxType.Rain, rain);
        this.fog = new RoomFxAtmosphereTilemap(RoomFxType.Fog, fog);
    }

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
    /// Arbitrary edited payloads are compiled from the supplied document. Exact
    /// stock liquid pages use their layout formulas and retain no generated cache.
    /// The statue effect aliases the water page; other ushort types are rejected.
    /// </summary>
    public ReadOnlyMemory<byte> Resolve(RoomFxType type) => type switch
    {
        RoomFxType.Lava => lava ?? RoomFxLiquidTilemapDefinitions.CreateTransfer(type),
        RoomFxType.Acid => acid ?? RoomFxLiquidTilemapDefinitions.CreateTransfer(type),
        RoomFxType.Water or RoomFxType.TourianEntranceStatue => water ?? RoomFxLiquidTilemapDefinitions.CreateTransfer(RoomFxType.Water),
        RoomFxType.Spores => spores.CreateTransfer(),
        RoomFxType.Rain => rain.CreateTransfer(),
        RoomFxType.Fog => fog.CreateTransfer(),
        _ => throw new InvalidDataException($"Room-FX BG3 tilemap {type} is not an authored page."),
    };

    /// <summary>Strict JSON settings shared by room-FX tilemap loading and serialization.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

/// <summary>Editable JSON definition of the six room-FX BG3 transfers, storing character selectors and BG attributes instead of packed SNES tilemap words.</summary>
public sealed record RoomFxLayer3TilemapDocument
{
    /// <summary>Schema revision; loading requires <see cref="RoomFxLayer3TilemapFormat.Version"/> before compiling any page.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly six pages keyed by <c>Lava</c>, <c>Acid</c>, <c>Water</c>, <c>Spores</c>, <c>Rain</c>, and <c>Fog</c>; each contains 1056 non-null cells in row-major transfer order with valid character and palette selectors.</summary>
    public required Dictionary<string, RoomBackgroundTilemapCell[]> Pages { get; init; }
}

/// <summary>Native bank-$8A 32x33 BG3 effect-page geometry and presentation identities.</summary>
public static class RoomFxLayer3TilemapFormat
{
    /// <summary>Shared editable JSON filename containing all six room-FX BG3 page definitions.</summary>
    public const string FileName = "room-fx-layer3-tilemaps.json";
    /// <summary>Required revision of the tile-reference document schema, independent of the native FX type identifiers.</summary>
    public const int Version = 1;
    /// <summary>Destination cells per transfer row; cell index modulo 32 selects its map column, not its character-sheet column.</summary>
    public const int WidthInTiles = 32;
    /// <summary>Rows retained from each native transfer, including its thirty-third row; these resources exceed a standard 32-by-32 BG page.</summary>
    public const int HeightInTiles = 33;
    /// <summary>Required editable cells per FX resource, 1056, preserving the complete native 32-by-33 transfer.</summary>
    public const int CellsPerPage = WidthInTiles * HeightInTiles;
    /// <summary>Compiled transfer length, $0840 bytes, with one little-endian sixteen-bit SNES BG tilemap word per cell.</summary>
    public const int PageByteCount = CellsPerPage * sizeof(ushort);

    /// <summary>$8A:8000, first room-FX BG3 page; the six pages end at $8A:B17F.</summary>
    public const int FirstSourceAddress = 0x8a8000;

    /// <summary>
    /// The six even type identities $02..0C at $83:ABF2..ABFC, in page order.
    /// Index0..5 maps to 2*(index+1); enumeration computes values without a roster.
    /// </summary>
    public static IReadOnlyList<RoomFxType> Types { get; } = new CalculatedTypes();

    /// <summary>Provides the six even native room-FX page identities by calculating each value from its ordinal.</summary>
    private sealed class CalculatedTypes : IReadOnlyList<RoomFxType>
    {
        /// <summary>Number of authored BG3 effect pages.</summary>
        public int Count => 6;

        /// <summary>Gets the native even room-FX identity at the requested page ordinal.</summary>
        /// <param name="index">Zero-based page position from lava through fog.</param>
        /// <returns>The corresponding native <see cref="RoomFxType"/> value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the six-page range.</exception>
        public RoomFxType this[int index] => (uint)index < Count
            ? (RoomFxType)(2 * (index + 1))
            : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>Enumerates the native even room-FX page identities in transfer order.</summary>
        /// <returns>An enumerator for lava, acid, water, spores, rain, and fog identities.</returns>
        public IEnumerator<RoomFxType> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Returns $8A:8000 + (type/2-1)*$840 for the six even types $02..0C.
    /// The original pointers at $83:ABF2..ABFC select consecutive 32x33 word pages.
    /// The statue effect aliases water; every other ushort type remains unsupported.
    /// </summary>
    public static int SourceAddress(RoomFxType type)
    {
        if (RoomFxTypes.UsesWater(type)) type = RoomFxType.Water;
        int value = (ushort)type;
        if (value is < 2 or > 12 || (value & 1) != 0)
            throw new InvalidDataException($"Room-FX type {type} has no BG3 tilemap page.");
        return FirstSourceAddress + (value / 2 - 1) * PageByteCount;
    }
}
