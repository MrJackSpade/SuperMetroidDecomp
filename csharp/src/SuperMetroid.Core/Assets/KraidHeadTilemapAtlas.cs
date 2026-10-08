using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One editable 32-by-11 Kraid head frame of ordered SNES BG2 tile words.</summary>
public sealed class KraidHeadTilemapAtlas
{
    private readonly ushort[] words;
    private KraidHeadTilemapAtlas(ushort[] words) => this.words = words;

    /// <summary>Read-only view of all 352 compiled BG2 tile words in 32-column row-major order, preserving character, palette, priority, and flip bits for the native head-copy operation at $A7:AF3D.</summary>
    public ReadOnlyMemory<ushort> Words => words;

    /// <summary>Compiles one supported 32x11 JSON head frame into SNES BG tile words, rejecting ambiguous or unknown properties and invalid artwork references.</summary>
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
            document.Cells.Length != KraidBackgroundRomData.HeadTilemapWords)
            throw new InvalidDataException("Kraid head tilemap must have 32 by 11 ordered cells.");

        var words = new ushort[document.Cells.Length];
        for (int index = 0; index < words.Length; index++)
        {
            RoomBackgroundTilemapCell? cell = document.Cells[index];
            if (cell is null ||
                (uint)cell.TileColumn >= RoomBackgroundTilemapFormat.TileColumns ||
                (uint)cell.TileRow >= RoomBackgroundTilemapFormat.TileRows ||
                (uint)cell.Palette >= RoomBackgroundTilemapFormat.PaletteCount)
                throw new InvalidDataException($"Kraid head tilemap cell {index} is invalid.");
            SnesTileFlipFlags flips =
                (cell.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (cell.FlipY ? SnesTileFlipFlags.Vertical : 0);
            words[index] = SnesBgTilemapWord.Create(
                cell.TileRow * RoomBackgroundTilemapFormat.TileColumns + cell.TileColumn,
                cell.Palette, cell.Priority, flips).Raw;
        }
        return new KraidHeadTilemapAtlas(words);
    }

    /// <summary>Converts one native head frame to indented camel-case UTF-8 JSON, then reloads it and verifies that every tile word round-trips unchanged.</summary>
    /// <param name="native">Exactly 704 bytes containing 352 little-endian BG tile words in 32-column row-major order.</param>
    /// <returns>A new JSON byte array expressing character coordinates, palette, priority, and horizontal/vertical flips for every cell.</returns>
    /// <exception cref="InvalidDataException">The source length is incorrect or the JSON round-trip changes a native tile word.</exception>
    public static byte[] Encode(ReadOnlySpan<byte> native)
    {
        if (native.Length != KraidBackgroundRomData.HeadTilemapWords * sizeof(ushort))
            throw new InvalidDataException("Kraid head tilemap source has wrong length.");
        var cells = new RoomBackgroundTilemapCell[KraidBackgroundRomData.HeadTilemapWords];
        for (int index = 0; index < cells.Length; index++)
        {
            var word = new SnesBgTilemapWord(
                BinaryPrimitives.ReadUInt16LittleEndian(native[(index * 2)..]));
            cells[index] = new RoomBackgroundTilemapCell
            {
                TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                Palette = word.PaletteIndex,
                Priority = word.HasPriority,
                FlipX = word.FlipHorizontally,
                FlipY = word.FlipVertically,
            };
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new KraidHeadTilemapDocument
        {
            Version = KraidHeadTilemapFormat.Version,
            Width = KraidHeadTilemapFormat.Width,
            Height = KraidHeadTilemapFormat.Height,
            Cells = cells,
        }, JsonOptions);
        KraidHeadTilemapAtlas roundtrip = Load(new MemoryStream(json, writable: false));
        for (int index = 0; index < roundtrip.words.Length; index++)
            if (roundtrip.words[index] != BinaryPrimitives.ReadUInt16LittleEndian(native[(index * 2)..]))
                throw new InvalidDataException("Kraid head JSON changed a native tile word.");
        return json;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

/// <summary>Editable JSON representation of a single ordered Kraid BG2 head frame; cells reference artwork rather than encoding animation or collision behavior.</summary>
public sealed record KraidHeadTilemapDocument
{
    /// <summary>Schema revision required to equal <see cref="KraidHeadTilemapFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Tilemap width in eight-pixel cells, required to be 32 rather than a pixel width or character-atlas stride.</summary>
    public required int Width { get; init; }
    /// <summary>Tilemap height in eight-pixel cells, required to be eleven.</summary>
    public required int Height { get; init; }
    /// <summary>Exactly 352 nonnull cells in 32-column row-major order, each retaining its character-atlas coordinates, BG palette, priority, and flip attributes.</summary>
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}

/// <summary>Supported JSON version and fixed head-frame dimensions matching the $0160-word native copy at $A7:AF3D.</summary>
public static class KraidHeadTilemapFormat
{
    /// <summary>Supported Kraid head tilemap schema revision one.</summary>
    public const int Version = 1;
    /// <summary>Thirty-two tile-reference columns in each head-frame row.</summary>
    public const int Width = 32;
    /// <summary>Eleven tile-reference rows in each head frame; with <see cref="Width"/> this yields 352 native words.</summary>
    public const int Height = 11;
}
