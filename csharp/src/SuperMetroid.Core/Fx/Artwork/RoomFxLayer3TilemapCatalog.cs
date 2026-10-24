using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Editable BG3 tile references for the six cartridge room-FX pages.</summary>
public sealed class RoomFxLayer3TilemapCatalog
{
    /// <summary>Edited liquid transfer bytes; null selects the stock layout formula for that effect.</summary>
    internal readonly byte[]? lava, acid, water;
    /// <summary>Compressed representation of the authored spore page.</summary>
    internal readonly RoomFxSporeTilemap spores;
    /// <summary>Representations of the rain and fog atmosphere pages.</summary>
    internal readonly RoomFxAtmosphereTilemap rain, fog;

    /// <summary>Retains edited liquid payloads while replacing exact stock pages with their calculated forms.</summary>
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
        Ensure.NotNull(json);
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
        Ensure.NotNull(json);
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
        RoomFxType.Lava => lava ?? RoomFxLiquidTilemapDefinitions.ComposePage(type),
        RoomFxType.Acid => acid ?? RoomFxLiquidTilemapDefinitions.ComposePage(type),
        RoomFxType.Water or RoomFxType.TourianEntranceStatue => water ?? RoomFxLiquidTilemapDefinitions.ComposePage(RoomFxType.Water),
        RoomFxType.Spores => spores.ComposePage(),
        RoomFxType.Rain => rain.ComposePage(),
        RoomFxType.Fog => fog.ComposePage(),
        RoomFxType.None or
            RoomFxType.ScrollingSky or
            RoomFxType.UnusedScrollingSky or
            RoomFxType.Fireflea or
            RoomFxType.CeresRidley or
            RoomFxType.CeresElevator or
            RoomFxType.CeresHaze => throw new InvalidDataException($"Room-FX BG3 tilemap {type} is not an authored page."),
        _ => throw new InvalidDataException($"Room-FX BG3 tilemap {type} is not an authored page."),
    };

    /// <summary>Uses camel-case editable fields, rejects unknown properties, and writes readable page documents.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
