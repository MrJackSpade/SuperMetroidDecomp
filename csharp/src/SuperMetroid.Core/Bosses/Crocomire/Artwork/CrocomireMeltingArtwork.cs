using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Indexed four-bit artwork for Crocomire's two melting images. The native erase order,
/// distortion, timing and VRAM destinations remain in the enemy mechanics.
/// </summary>
public sealed class CrocomireMeltingArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-crocomire-melt-v1", content =>
        {
            content.Append("first", first.Transfer.Span);
            content.Append("second", second.Transfer.Span);
            content.AppendWords("first-map", firstTilemap.Words());
            content.AppendWords("second-map", secondTilemap.Words());
        });

    /// <summary>Installed planar character data for the first overlapping melt pass.</summary>
    internal readonly RoomCharacterAtlas first;
    /// <summary>Installed planar character data for the second, eroded melt pass.</summary>
    internal readonly RoomCharacterAtlas second;
    /// <summary>Validated BG2 references for the first illustration.</summary>
    private readonly CrocomireMeltingTilemap firstTilemap;
    /// <summary>Validated BG2 references for the second illustration.</summary>
    private readonly CrocomireMeltingTilemap secondTilemap;

    /// <summary>Retains compiled sheets and tilemaps, distinguishing the second illustration's layout.</summary>
    private CrocomireMeltingArtwork(RoomCharacterAtlas first, RoomCharacterAtlas second,
        ushort[] firstTilemap, ushort[] secondTilemap)
    {
        this.first = first;
        this.second = second;
        this.firstTilemap = new(false, firstTilemap);
        this.secondTilemap = new(true, secondTilemap);
    }

    /// <summary>Compiles both PNG sheets and tile layouts into one installed snapshot.</summary>
    public static CrocomireMeltingArtwork Load(Stream firstPng, Stream secondPng,
        Stream firstTilemapJson, Stream secondTilemapJson)
    {
        Ensure.NotNull(firstPng);
        Ensure.NotNull(secondPng);
        Ensure.NotNull(firstTilemapJson);
        Ensure.NotNull(secondTilemapJson);
        RoomCharacterAtlas first = RoomCharacterAtlas.Load(firstPng,
            CrocomireMeltingArtworkFormat.FirstByteCount);
        RoomCharacterAtlas second = RoomCharacterAtlas.Load(secondPng,
            CrocomireMeltingArtworkFormat.SecondByteCount);
        ValidatePadding(first, CrocomireMeltingTransferDefinitions.Passes[0], "first");
        ValidatePadding(second, CrocomireMeltingTransferDefinitions.Passes[1], "second");
        return new(first, second, ReadTilemap(firstTilemapJson),
            ReadTilemap(secondTilemapJson));
    }

    /// <summary>
    /// Replaces only the byte range written by the native overlapping copies. The rest of
    /// the scratch allocation retains its prior value, including across the second pass.
    /// </summary>
    internal void CopyPassTo(int headerOffset, Span<byte> scratch)
    {
        CrocomireMeltingPass pass = CrocomireMeltingTransferDefinitions.Header(headerOffset);
        ReadOnlySpan<byte> image = (headerOffset ==
            (ushort)CrocomireMeltingHeader.First ? first : second).Transfer.Span;
        int used = UsedByteCount(pass);
        if (scratch.Length < used)
            throw new InvalidDataException("Crocomire melting scratch image is too small.");
        image[..used].CopyTo(scratch);
    }

    /// <summary>Returns only the visual tile references, never melt control or hitboxes.</summary>
    internal ReadOnlySpan<ushort> Tilemap(CrocomireMeltingTilemapAddress sourceAddress) => sourceAddress switch
    {
        CrocomireMeltingTilemapAddress.FirstTilemap => firstTilemap.Words(),
        CrocomireMeltingTilemapAddress.SecondTilemap => secondTilemap.Words(),
        _ => throw new InvalidDataException(
            $"Crocomire melt tilemap ${(int)sourceAddress:X6} is not installed."),
    };

    /// <summary>Writes a validated, human-editable tile-layout document.</summary>
    public static void WriteTilemap(Stream json, CrocomireMeltingTilemapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, TilemapJsonOptions);
        _ = ReadTilemap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Validates a 16-by-16 tile-layout document and packs each cell into a native BG word.</summary>
    private static ushort[] ReadTilemap(Stream json)
    {
        CrocomireMeltingTilemapDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            // Preserve the schema's existing case-insensitive spelling, but reject aliases
            // of the same property before deserialization can silently overwrite them.
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement, StringComparer.OrdinalIgnoreCase);
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
                "Crocomire melt tilemap requires a version-one 16x16 ordered cell array.");

        var words = new ushort[document.Cells.Length];
        for (int index = 0; index < words.Length; index++)
        {
            CrocomireMeltingTilemapCell? cell = document.Cells[index];
            if (cell is null || (uint)cell.TileIndex > 1023 ||
                (uint)cell.Palette > 7)
                throw new InvalidDataException(
                    $"Crocomire melt tilemap cell {index} has an invalid tile or palette.");
            SnesTileFlipFlags flips =
                (cell.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (cell.FlipY ? SnesTileFlipFlags.Vertical : 0);
            words[index] = SnesBgTilemapWord.Create(
                cell.TileIndex, cell.Palette, cell.Priority, flips).Raw;
        }
        return words;
    }

    /// <summary>Uses the published camel-case schema while rejecting unknown and duplicate properties.</summary>
    private static readonly JsonSerializerOptions TilemapJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Finds the last scratch byte written by a pass's overlapping native copies.</summary>
    internal static int UsedByteCount(CrocomireMeltingPass pass)
    {
        int end = 0;
        foreach (CrocomireMeltingCopy copy in pass.Copies)
            end = Math.Max(end, copy.DestinationWord - CrocomireMeltingTransferDefinitions.ScratchStart + (pass.WordsToCopy + 1) * 2);
        return end;
    }

    /// <summary>Rejects nonzero PNG data outside the scratch region touched by native melt copies.</summary>
    private static void ValidatePadding(RoomCharacterAtlas atlas, CrocomireMeltingPass pass,
        string name)
    {
        ReadOnlySpan<byte> bytes = atlas.Transfer.Span;
        int used = UsedByteCount(pass);
        if (bytes[used..].IndexOfAnyExcept((byte)0) >= 0)
            throw new InvalidDataException(
                $"Crocomire {name} melt PNG contains pixels outside the native scratch image.");
    }
}
