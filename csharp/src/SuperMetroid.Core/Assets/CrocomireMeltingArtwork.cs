using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Indexed four-bit artwork for Crocomire's two melting images. The native erase order,
/// distortion, timing and VRAM destinations remain in the enemy mechanics.
/// </summary>
public sealed class CrocomireMeltingArtwork
{
    private readonly RoomCharacterAtlas first;
    private readonly RoomCharacterAtlas second;
    private readonly ushort[] firstTilemap;
    private readonly ushort[] secondTilemap;

    private CrocomireMeltingArtwork(RoomCharacterAtlas first, RoomCharacterAtlas second,
        ushort[] firstTilemap, ushort[] secondTilemap)
    {
        this.first = first;
        this.second = second;
        this.firstTilemap = firstTilemap;
        this.secondTilemap = secondTilemap;
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
            CrocomireMeltingTransferDefinitions.FirstHeaderOffset ? first : second).Transfer.Span;
        int used = UsedByteCount(pass);
        if (scratch.Length < used)
            throw new InvalidDataException("Crocomire melting scratch image is too small.");
        image[..used].CopyTo(scratch);
    }

    /// <summary>Returns only the visual tile references, never melt control or hitboxes.</summary>
    internal ReadOnlySpan<ushort> Tilemap(int sourceAddress) => sourceAddress switch
    {
        CrocomireMeltingArtworkAddresses.FirstTilemap => firstTilemap,
        CrocomireMeltingArtworkAddresses.SecondTilemap => secondTilemap,
        _ => throw new InvalidDataException(
            $"Crocomire melt tilemap ${sourceAddress:X6} is not installed."),
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
            document = JsonSerializer.Deserialize<CrocomireMeltingTilemapDocument>(
                json, TilemapJsonOptions)
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
        WriteIndented = true,
    };

    internal static int UsedByteCount(CrocomireMeltingPass pass)
    {
        int end = 0;
        foreach (CrocomireMeltingCopy copy in pass.Copies.Span)
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
    public const string FirstFileName = "crocomire-melt-first.png";
    public const string SecondFileName = "crocomire-melt-second.png";
    public const string FirstTilemapFileName = "crocomire-melt-first-tiles.json";
    public const string SecondTilemapFileName = "crocomire-melt-second-tiles.json";
    public const int FirstByteCount = 0x0e20;
    public const int SecondByteCount = 0x1020;
    public const int TilemapVersion = 1;
    public const int TilemapWidth = 16;
    public const int TilemapHeight = 16;
    public const int TilemapCellCount = TilemapWidth * TilemapHeight;
}

/// <summary>One ordered 16x16 visual tile layout; mechanics are not configurable.</summary>
public sealed record CrocomireMeltingTilemapDocument
{
    public required int Version { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required CrocomireMeltingTilemapCell[] Cells { get; init; }
}

/// <summary>One BG2 tile reference, palette and visual flip/priority selection.</summary>
public sealed record CrocomireMeltingTilemapCell
{
    public required int TileIndex { get; init; }
    public required int Palette { get; init; }
    public required bool Priority { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}
