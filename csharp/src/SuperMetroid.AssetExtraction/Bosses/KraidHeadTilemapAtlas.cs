using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>One editable 32-by-11 Kraid head frame of placed BG2 tiles.</summary>
public sealed class KraidHeadTilemapAtlas
{
    /// <summary>Owned 32-column BG2 tiles the Kraid head-copy operation places.</summary>
    private readonly BackgroundTile[] tiles;
    /// <summary>Retains the validated frame compiled from JSON.</summary>
    private KraidHeadTilemapAtlas(BackgroundTile[] tiles) => this.tiles = tiles;

    /// <summary>Read-only view of all 352 placed BG2 tiles in 32-column row-major order, with each tile's colour variant, priority and mirroring, for the head-copy operation at $A7:AF3D.</summary>
    public ReadOnlyMemory<BackgroundTile> Tiles => tiles;

    /// <summary>Compiles one supported 32x11 JSON head frame into placed tiles, rejecting ambiguous or unknown properties and invalid artwork references.</summary>
    /// <param name="json">JSON stream read from its current position to the end and left open.</param>
    /// <returns>An installed head frame independent of the boss's animation timing, attack instructions, and hitboxes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, version, dimensions, cell count, atlas coordinates, or palette selector is invalid.</exception>
    public static KraidHeadTilemapAtlas Load(Stream json)
    {
        KraidHeadTilemapDocument document = JsonAssetDocument.Read<KraidHeadTilemapDocument>(
            json, JsonOptions, "Kraid head tilemap");
        if (document.Version != KraidHeadTilemapFormat.Version ||
            document.Width != KraidHeadTilemapFormat.Width ||
            document.Height != KraidHeadTilemapFormat.Height ||
            document.Cells is null ||
            document.Cells.Length != KraidWorkingMapDefinitions.HeadImageCells)
            throw new InvalidDataException("Kraid head tilemap must have 32 by 11 ordered cells.");

        var tiles = new BackgroundTile[document.Cells.Length];
        for (int index = 0; index < tiles.Length; index++)
            tiles[index] = document.Cells[index]?.Placement()
                ?? throw new InvalidDataException($"Kraid head tilemap cell {index} is invalid.");
        return new KraidHeadTilemapAtlas(tiles);
    }

    /// <summary>Strict camel-case artwork schema settings.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
