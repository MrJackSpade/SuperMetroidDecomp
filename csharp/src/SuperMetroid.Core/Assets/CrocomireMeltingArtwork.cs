using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

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

    private readonly RoomCharacterAtlas first;
    private readonly RoomCharacterAtlas second;
    private readonly CrocomireMeltingTilemap firstTilemap;
    private readonly CrocomireMeltingTilemap secondTilemap;

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
        ArgumentNullException.ThrowIfNull(firstPng);
        ArgumentNullException.ThrowIfNull(secondPng);
        ArgumentNullException.ThrowIfNull(firstTilemapJson);
        ArgumentNullException.ThrowIfNull(secondTilemapJson);
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
    internal void CopyPassTo(ushort headerOffset, Span<byte> scratch)
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
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, TilemapJsonOptions);
        _ = ReadTilemap(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

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
            if (cell is null || (uint)cell.TileIndex > 0x03ff ||
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

    private static readonly JsonSerializerOptions TilemapJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    internal static int UsedByteCount(CrocomireMeltingPass pass)
    {
        int end = 0;
        foreach (CrocomireMeltingCopy copy in pass.Copies)
            end = Math.Max(end, copy.DestinationWord - 0x4000 + (pass.WordsToCopy + 1) * 2);
        return end;
    }

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

/// <summary>Stable filenames and exact four-bit PNG transfer geometry for both melts.</summary>
public static class CrocomireMeltingArtworkFormat
{
    /// <summary>Installed indexed PNG filename for the first dissolve image, assembled from the native bank-$A4 copies beginning at $A07D.</summary>
    public const string FirstFileName = "crocomire-melt-first.png";
    /// <summary>Installed indexed PNG filename for the second, eroded dissolve image, assembled from the native bank-$A4 copies beginning at $AC7D.</summary>
    public const string SecondFileName = "crocomire-melt-second.png";
    /// <summary>Installed editable JSON filename for the first image's 256 BG2 references, corresponding to $A4:9C79.</summary>
    public const string FirstTilemapFileName = "crocomire-melt-first-tiles.json";
    /// <summary>Installed editable JSON filename for the second image's 256 BG2 references, corresponding to $A4:9E7B.</summary>
    public const string SecondTilemapFileName = "crocomire-melt-second-tiles.json";
    /// <summary>$0E20 compiled 4bpp bytes, or 113 characters, for the first scratch image; bytes beyond the native overlapping-copy extent must remain zero.</summary>
    public const int FirstByteCount = 0x0e20;
    /// <summary>$1020 compiled 4bpp bytes, or 129 characters, for the second scratch image; bytes beyond the native overlapping-copy extent must remain zero.</summary>
    public const int SecondByteCount = 0x1020;
    /// <summary>Supported visual tile-layout schema revision, checked before compiling packed SNES BG words.</summary>
    public const int TilemapVersion = 1;
    /// <summary>Required JSON width declaration of 16; cells retain native linear order rather than being rearranged into 16-column runtime BG rows.</summary>
    public const int TilemapWidth = 16;
    /// <summary>Required JSON height declaration of 16, combining with the width to bound the native 256-word illustration payload.</summary>
    public const int TilemapHeight = 16;
    /// <summary>256 ordered BG2 references, representing eight 32-cell native rows despite the editable document's 16-by-16 dimension declaration.</summary>
    public const int TilemapCellCount = TilemapWidth * TilemapHeight;
}

/// <summary>One ordered 16x16 visual tile layout; mechanics are not configurable.</summary>
public sealed record CrocomireMeltingTilemapDocument
{
    /// <summary>Visual tile-layout revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Required schema width declaration of 16; this does not replace the consuming BG tilemap's 32-cell row stride.</summary>
    public required int Width { get; init; }
    /// <summary>Required schema height declaration of 16, establishing a 256-cell payload with the width.</summary>
    public required int Height { get; init; }
    /// <summary>Exactly 256 cells in native linear source order, including blank margins; runtime placement consumes them as eight 32-cell BG2 rows.</summary>
    public required CrocomireMeltingTilemapCell[] Cells { get; init; }
}

/// <summary>One BG2 tile reference, palette and visual flip/priority selection.</summary>
public sealed record CrocomireMeltingTilemapCell
{
    /// <summary>BG character selector from 0 through 1023, encoded in the low ten bits; blank glyphs and repeated references remain independent presentation choices.</summary>
    public required int TileIndex { get; init; }
    /// <summary>SNES BG palette index from 0 through 7, encoded in bits 10–12; stock illustrated fragments use palette 7.</summary>
    public required int Palette { get; init; }
    /// <summary>Whether to set packed BG priority bit $2000; this changes visual layering, not dissolve timing or collision.</summary>
    public required bool Priority { get; init; }
    /// <summary>Whether to mirror the referenced character horizontally, setting packed BG word bit $4000.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether to mirror the referenced character vertically, setting packed BG word bit $8000.</summary>
    public required bool FlipY { get; init; }
}
