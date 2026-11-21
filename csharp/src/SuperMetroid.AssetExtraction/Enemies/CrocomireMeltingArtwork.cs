using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Indexed four-bit artwork for Crocomire's two melting images and the tilemaps that lay them out. The extraction pictures
/// each band of an image at every dissolve step; the death sequence selects which picture each band shows.
/// </summary>
public sealed class CrocomireMeltingArtwork
{

    /// <summary>The import data's planar character data for the flesh's overlapping melt pass.</summary>
    internal readonly RoomCharacterAtlas flesh;
    /// <summary>The import data's planar character data for the bare skull's eroded melt pass.</summary>
    internal readonly RoomCharacterAtlas bareSkull;
    /// <summary>The import data's tile placements for the flesh illustration.</summary>
    internal readonly BackgroundTile[] fleshTilemap;
    /// <summary>The import data's tile placements for the bare skull illustration.</summary>
    internal readonly BackgroundTile[] bareSkullTilemap;

    /// <summary>Keeps both character sheets and both complete layouts the import composes the melt pictures from.</summary>
    private CrocomireMeltingArtwork(RoomCharacterAtlas flesh, RoomCharacterAtlas bareSkull,
        BackgroundTile[] fleshTilemap, BackgroundTile[] bareSkullTilemap)
    {
        this.flesh = flesh;
        this.bareSkull = bareSkull;
        this.fleshTilemap = fleshTilemap;
        this.bareSkullTilemap = bareSkullTilemap;
    }

    /// <summary>Combines both of the import data's character sheets and tile layouts into the melt's import snapshot (#1638).</summary>
    /// <param name="flesh">The flesh illustration's installed characters.</param>
    /// <param name="bareSkull">The bare skull illustration's installed characters.</param>
    /// <param name="fleshTilemapJson">The flesh illustration's tile layout.</param>
    /// <param name="bareSkullTilemapJson">The bare skull illustration's tile layout.</param>
    /// <exception cref="InvalidDataException">A sheet is not its illustration's length or holds pixels outside it, or a layout is invalid.</exception>
    public static CrocomireMeltingArtwork Load(RoomCharacterAtlas flesh, RoomCharacterAtlas bareSkull,
        Stream fleshTilemapJson, Stream bareSkullTilemapJson)
    {
        Ensure.NotNull(flesh);
        Ensure.NotNull(bareSkull);
        Ensure.NotNull(fleshTilemapJson);
        Ensure.NotNull(bareSkullTilemapJson);
        RequireLength(flesh, CrocomireMeltingArtworkFormat.FleshByteCount, CrocomireMeltingIllustration.Flesh);
        RequireLength(bareSkull, CrocomireMeltingArtworkFormat.BareSkullByteCount, CrocomireMeltingIllustration.BareSkull);
        ValidatePadding(flesh, CrocomireMeltingIllustration.Flesh);
        ValidatePadding(bareSkull, CrocomireMeltingIllustration.BareSkull);
        return new(flesh, bareSkull, ReadTilemap(fleshTilemapJson),
            ReadTilemap(bareSkullTilemapJson));
    }

    /// <summary>The four-bit characters of <paramref name="illustration"/>'s image, sixteen to a row, a band per row.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="illustration"/> is not a melt image.</exception>
    internal ReadOnlyMemory<byte> Characters(CrocomireMeltingIllustration illustration) => illustration switch
    {
        CrocomireMeltingIllustration.Flesh => flesh.Characters,
        CrocomireMeltingIllustration.BareSkull => bareSkull.Characters,
        _ => throw new ArgumentOutOfRangeException(nameof(illustration), illustration, $"Undefined {nameof(CrocomireMeltingIllustration)}."),
    };

    /// <summary>Returns only the visual tile references, never melt control or hitboxes.</summary>
    internal ReadOnlySpan<BackgroundTile> Tilemap(CrocomireMeltingIllustration illustration) => illustration switch
    {
        CrocomireMeltingIllustration.Flesh => fleshTilemap,
        CrocomireMeltingIllustration.BareSkull => bareSkullTilemap,
        _ => throw new InvalidDataException(
            $"Crocomire melt tilemap {illustration} is not a melt image."),
    };

    /// <summary>Writes a human-editable tile-layout document.</summary>
    public static void WriteTilemap(Stream json, CrocomireMeltingTilemapDocument document)
    {
        Ensure.NotNull(json);
        json.Write(JsonSerializer.SerializeToUtf8Bytes(document, TilemapJsonOptions));
    }

    /// <summary>
    /// Validates a 32-by-8 tile-layout document of the import data and decodes each cell's tile placement, which only the
    /// import reads to compose the melt pictures (#1638).
    /// </summary>
    private static BackgroundTile[] ReadTilemap(Stream json)
    {
        CrocomireMeltingTilemapDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            // Preserve the schema's existing case-insensitive spelling, but reject aliases
            // of the same property before deserialization can silently overwrite them.
            JsonAssetDocument.RejectDuplicateProperties(parsed.RootElement, StringComparer.OrdinalIgnoreCase,
                name => new InvalidDataException($"Duplicate enemy composition property {name}."));
            document = parsed.RootElement.Deserialize<CrocomireMeltingTilemapDocument>(TilemapJsonOptions)
                ?? throw new InvalidDataException("Crocomire melt tilemap JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Crocomire melt tilemap JSON.", error);
        }
        if (document.Version != CrocomireMeltingArtworkFormat.TilemapVersion ||
            document.Width != CrocomireMeltingArtworkFormat.TilemapWidth ||
            document.Height != CrocomireMeltingArtworkFormat.TilemapHeight ||
            document.Cells is not { Length: CrocomireMeltingArtworkFormat.TilemapCellCount })
            throw new InvalidDataException(
                $"Crocomire melt tilemap requires version {CrocomireMeltingArtworkFormat.TilemapVersion} and a {CrocomireMeltingArtworkFormat.TilemapWidth}x{CrocomireMeltingArtworkFormat.TilemapHeight} cell array.");

        var tiles = new BackgroundTile[document.Cells.Length];
        for (int index = 0; index < tiles.Length; index++)
        {
            CrocomireMeltingTilemapCell? cell = document.Cells[index];
            if (cell is null || (uint)cell.TileIndex >= BackgroundTile.TileCount ||
                (uint)cell.Palette >= BackgroundTile.ColourVariants)
                throw new InvalidDataException(
                    $"Crocomire melt tilemap cell {index} has an invalid tile or palette.");
            tiles[index] = new BackgroundTile(cell.TileIndex, cell.Palette, cell.FlipX, cell.FlipY, cell.Priority);
        }
        return tiles;
    }

    /// <summary>Uses the published camel-case schema while rejecting unknown and duplicate properties.</summary>
    private static readonly JsonSerializerOptions TilemapJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Bytes of one row of a melt image's characters: sixteen four-bit characters.</summary>
    private const int RowByteCount = CrocomireMeltDissolve.BandRowCharacters * RoomCharacterAtlasFormat.BytesPerTile;

    /// <summary>
    /// The bytes of <paramref name="illustration"/>'s image the import assembles: a row per band of the image, and the row
    /// and word after the last, which the last band's copy carries past the image (#2573).
    /// </summary>
    internal static int UsedByteCount(CrocomireMeltingIllustration illustration) =>
        Enum.IsDefined(illustration) ? (CrocomireMeltingDefinitions.MeltImageBandCount + 1) * RowByteCount + sizeof(ushort)
            : throw new ArgumentOutOfRangeException(nameof(illustration), illustration, "Crocomire has two melt images.");

    /// <summary>Rejects a sheet that is not <paramref name="illustration"/>'s installed length.</summary>
    private static void RequireLength(RoomCharacterAtlas atlas, int byteCount, CrocomireMeltingIllustration illustration)
    {
        if (atlas.Characters.Length != byteCount)
            throw new InvalidDataException($"Crocomire {illustration} melt sheet holds {atlas.Characters.Length} bytes; expected {byteCount}.");
    }

    /// <summary>Rejects nonzero PNG data outside the bytes the cartridge assembles <paramref name="illustration"/>'s image from.</summary>
    private static void ValidatePadding(RoomCharacterAtlas atlas, CrocomireMeltingIllustration illustration)
    {
        ReadOnlySpan<byte> bytes = atlas.Characters.Span;
        if (bytes[UsedByteCount(illustration)..].IndexOfAnyExcept((byte)0) >= 0)
            throw new InvalidDataException(
                $"Crocomire {illustration} melt PNG contains pixels outside the image the melt assembles.");
    }
}
