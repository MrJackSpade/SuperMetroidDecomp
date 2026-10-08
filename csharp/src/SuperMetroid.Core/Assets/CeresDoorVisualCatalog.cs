using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Ceres-door tile DMA and RGB5 palettes; actor timing remains engine-owned.</summary>
public sealed class CeresDoorVisualCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
        {
            content.Append("tiles", tiles.Transfer.Span);
            Span<ushort> normalColors = stackalloc ushort[CeresDoorVisualRomData.SetupColorCount];
            for (int color = 0; color < normalColors.Length; color++) normalColors[color] = normal.ColorAt(color);
            content.AppendWords("normal", normalColors);
            Span<ushort> escapeColors = stackalloc ushort[CeresDoorVisualRomData.SetupColorCount];
            for (int color = 0; color < escapeColors.Length; color++) escapeColors[color] = EscapeColor(color);
            content.AppendWords("escape", escapeColors);
            content.Append("animation", CeresDoorVisualRomData.AnimationRowCount);
            Span<ushort> rowColors = stackalloc ushort[CeresDoorVisualRomData.AnimationColorCount];
            for (int row = 0; row < CeresDoorVisualRomData.AnimationRowCount; row++)
            {
                for (int color = 0; color < rowColors.Length; color++) rowColors[color] = AnimationColor(row, color);
                content.AppendWords("row", rowColors);
            }
            content.Append("mode7-frames", CeresDoorVisualRomData.Mode7FrameCount);
            Span<byte> platform = stackalloc byte[CeresDoorVisualRomData.Mode7FrameByteCount];
            for (int frame = 0; frame < CeresDoorVisualRomData.Mode7FrameCount; frame++)
            {
                for (int index = 0; index < platform.Length; index++) platform[index] = PlatformTile(frame, index);
                content.Append("mode7-frame", platform);
            }
        });

    private readonly RoomCharacterAtlas tiles;
    private readonly CeresDoorNormalPaintDefinitions normal;
    private readonly CeresDoorEscapePaintDefinitions escape;
    private readonly CeresDoorAnimationPaintDefinitions animation;
    private readonly Dictionary<int, byte> platformEdits = [];

    private CeresDoorVisualCatalog(RoomCharacterAtlas tiles, ushort[] normal,
        ushort[] escape, ushort[][] animation, byte[][] mode7DoorFrames)
    {
        this.tiles = tiles;
        this.normal = new(normal);
        this.escape = new(escape, this.normal);
        this.animation = new(animation, this.normal);
        for (int frame = 0; frame < mode7DoorFrames.Length; frame++)
        for (int index = 0; index < CeresDoorVisualRomData.Mode7FrameByteCount; index++)
            if (mode7DoorFrames[frame][index] != PlatformTile(frame, index))
                platformEdits.Add(frame * CeresDoorVisualRomData.Mode7FrameByteCount + index, mode7DoorFrames[frame][index]);
    }

    /// <summary>Loads the indexed 4bpp door sheet and version-1 visual JSON, validating native transfer lengths, RGB5 channels, and eight-bit Mode 7 character selectors without importing door timing or variant logic.</summary>
    /// <param name="tilePng">Caller-owned <c>ceres-door-tiles.png</c> stream compiling to $0400 bytes.</param>
    /// <param name="paletteJson">Caller-owned <c>ceres-door-colors.json</c> stream containing setup colors, animation rows, and Mode 7 strip cells.</param>
    /// <returns>The compiled visual snapshot; streams are consumed from their current positions and left open.</returns>
    public static CeresDoorVisualCatalog Load(Stream tilePng, Stream paletteJson)
    {
        ArgumentNullException.ThrowIfNull(tilePng);
        ArgumentNullException.ThrowIfNull(paletteJson);
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            CeresDoorVisualRomData.TileByteCount);
        CeresDoorVisualDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresDoorVisualDocument>(paletteJson, Options)
                ?? throw new InvalidDataException("Ceres-door visual palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres-door visual palette JSON.", error);
        }
        if (document.Version != 1 || document.Animation is null ||
            document.Animation.Length != CeresDoorVisualRomData.AnimationRowCount)
            throw new InvalidDataException("Ceres-door visuals require version 1 and eight animation rows.");
        ushort[] normal = Compile(document.Normal, CeresDoorVisualRomData.SetupColorCount,
            "normal");
        ushort[] escape = Compile(document.Escape, CeresDoorVisualRomData.SetupColorCount,
            "escape");
        ushort[][] animation = document.Animation.Select((row, index) =>
            Compile(row, CeresDoorVisualRomData.AnimationColorCount,
                $"animation row {index}")).ToArray();
        byte[][] mode7DoorFrames = CompileMode7Frames(document.Mode7DoorFrames);
        return new CeresDoorVisualCatalog(tiles, normal, escape, animation,
            mode7DoorFrames);
    }

    /// <summary>Serializes visual JSON as indented camel-case UTF-8, validating version, palette dimensions, RGB5 bounds, and Mode 7 frame bytes before returning it; PNG admission is a separate boundary.</summary>
    public static byte[] Write(CeresDoorVisualDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        // The PNG is validated independently; validate color structure here.
        if (document.Version != 1 || document.Animation is null ||
            document.Animation.Length != CeresDoorVisualRomData.AnimationRowCount)
            throw new InvalidDataException("Ceres-door visuals require version 1 and eight animation rows.");
        _ = Compile(document.Normal, CeresDoorVisualRomData.SetupColorCount, "normal");
        _ = Compile(document.Escape, CeresDoorVisualRomData.SetupColorCount, "escape");
        for (int row = 0; row < document.Animation.Length; row++)
            _ = Compile(document.Animation[row], CeresDoorVisualRomData.AnimationColorCount,
                $"animation row {row}");
        _ = CompileMode7Frames(document.Mode7DoorFrames);
        return json;
    }

    /// <summary>Loads $0400 selected 4bpp bytes, or 32 characters, at VRAM byte $E000, replacing variant two's native $B0:C400 DMA source.</summary>
    public void LoadTiles(SnesVram vram) =>
        tiles.LoadTo(vram, CeresDoorVisualRomData.TileVramDestination);

    /// <summary>Installs fifteen normal nontransparent inks corresponding to $A6:F4EE in current CGRAM, exposing the native completed setup-fade target directly.</summary>
    /// <param name="cgram">Current palette receiving the setup image.</param>
    /// <param name="destination">First CGRAM color index, not a byte offset; the actor uses 161 for the inactive variant-three target or 241 for the active door palette.</param>
    public void LoadNormalColors(SnesCgram cgram, int destination)
    {
        Ensure.NotNull(cgram);
        if (destination < 0 || destination + CeresDoorVisualRomData.SetupColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int color = 0; color < CeresDoorVisualRomData.SetupColorCount; color++)
            cgram.SetColor(destination + color, normal.ColorAt(color));
    }

    /// <summary>Installs fifteen escape-state nontransparent inks corresponding to $A6:F50E in current CGRAM; escape-state selection remains actor-owned.</summary>
    /// <param name="cgram">Current palette receiving the setup image.</param>
    /// <param name="destination">First CGRAM color index, not a byte offset; the active door uses 241, leaving its transparent slot unchanged.</param>
    public void LoadEscapeColors(SnesCgram cgram, int destination)
    {
        Ensure.NotNull(cgram);
        if (destination < 0 || destination + CeresDoorVisualRomData.SetupColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int color = 0; color < CeresDoorVisualRomData.SetupColorCount; color++)
            cgram.SetColor(destination + color, EscapeColor(color));
    }

    private ushort EscapeColor(int color) => escape.ColorAt(color);

    /// <summary>Installs six selected inks at CGRAM 41–46 from animation row 0–7 corresponding to $A6:F871; the actor derives the row from its frame bits 3–5.</summary>
    public void LoadAnimationColors(SnesCgram cgram, int row)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)row >= CeresDoorVisualRomData.AnimationRowCount) throw new IndexOutOfRangeException();
        for (int color = 0; color < CeresDoorVisualRomData.AnimationColorCount; color++)
            cgram.SetColor(CeresDoorVisualRomData.AnimationTargetColor + color, AnimationColor(row, color));
    }

    private ushort AnimationColor(int row, int color) => animation.ColorAt(row, color);

    /// <summary>$A6:F918/F91C: resolve shared canonical platform imagery or an independent supplied cell edit.</summary>
    private byte PlatformTile(int frame, int index)
    {
        byte stock = CeresMode7TransferDefinitions.PlatformTile(frame, index);
        return platformEdits.TryGetValue(frame * CeresDoorVisualRomData.Mode7FrameByteCount + index, out byte edit) ? edit : stock;
    }

    /// <summary>Writes selected light/dark platform frame 0 or 1 as four eight-bit Mode 7 map-lane cells at VRAM word $060E, preserving the native $A6:F918/$F91C strip width.</summary>
    public void LoadMode7DoorFrame(SnesVram vram, int frame)
    {
        ArgumentNullException.ThrowIfNull(vram);
        Span<byte> transfer = stackalloc byte[CeresDoorVisualRomData.Mode7FrameByteCount];
        for (int index = 0; index < transfer.Length; index++) transfer[index] = PlatformTile(frame, index);
        vram.LoadMode7MapBytes(transfer, CeresDoorVisualRomData.Mode7DestinationWord);
    }

    private static byte[][] CompileMode7Frames(int[][]? source)
    {
        if (source is null || source.Length != CeresDoorVisualRomData.Mode7FrameCount)
            throw new InvalidDataException("Ceres-door visuals require two Mode-7 tilemap frames.");
        var result = new byte[source.Length][];
        for (int frame = 0; frame < source.Length; frame++)
        {
            int[]? values = source[frame];
            if (values is null || values.Length != CeresDoorVisualRomData.Mode7FrameByteCount)
                throw new InvalidDataException($"Ceres-door Mode-7 frame {frame} needs four bytes.");
            result[frame] = new byte[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                if ((uint)values[index] > byte.MaxValue)
                    throw new InvalidDataException(
                        $"Ceres-door Mode-7 frame {frame} value {index} is not a byte.");
                result[frame][index] = (byte)values[index];
            }
        }
        return result;
    }

    private static ushort[] Compile(PaletteRgb5[]? source, int expectedCount, string name)
    {
        if (source is null || source.Length != expectedCount)
            throw new InvalidDataException($"Ceres-door {name} needs {expectedCount} RGB5 colors.");
        var result = new ushort[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Ceres-door {name} color {index} must be RGB5.");
            result[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return result;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

/// <summary>Editable door palette and Mode 7 platform-strip schema, separate from the indexed 4bpp character PNG and engine-owned door/escape timing.</summary>
public sealed record CeresDoorVisualDocument
{
    /// <summary>Visual schema revision; loading and serialization currently require version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Fifteen ordered nontransparent RGB5 inks corresponding to $A6:F4EE; every red, green, and blue channel must be 0–31.</summary>
    public required PaletteRgb5[] Normal { get; init; }
    /// <summary>Fifteen ordered nontransparent escape RGB5 inks corresponding to $A6:F50E, independent of the normal setup image.</summary>
    public required PaletteRgb5[] Escape { get; init; }
    /// <summary>Eight rows of six RGB5 inks corresponding to the $A6:F871 animation table, installed at CGRAM 41–46 in actor-selected frame order.</summary>
    public required PaletteRgb5[][] Animation { get; init; }
    /// <summary>Two ordered four-cell strips corresponding to $A6:F918/$F91C, each containing byte-valued Mode 7 character indices rather than ordinary BG palette/flip words.</summary>
    public required int[][] Mode7DoorFrames { get; init; }
}

/// <summary>Stable host filenames for Ceres-door graphics and palettes.</summary>
public static class CeresDoorVisualFormat
{
    /// <summary>Installed indexed PNG filename for the 32 4bpp door characters used by variant two's OBJ tile upload.</summary>
    public const string TilesFileName = "ceres-door-tiles.png";
    /// <summary>Installed editable JSON filename for normal/escape setup colors, animation inks, and the two Mode 7 platform strips.</summary>
    public const string ColorsFileName = "ceres-door-colors.json";
}
