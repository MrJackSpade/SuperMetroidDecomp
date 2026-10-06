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
    private readonly ushort[] animationSeeds;
    private readonly Dictionary<int, ushort> animationPhaseResiduals = [];
    private readonly Dictionary<int, ushort> animationRowEdits = [];
    private readonly Dictionary<int, byte> platformEdits = [];

    private CeresDoorVisualCatalog(RoomCharacterAtlas tiles, ushort[] normal,
        ushort[] escape, ushort[][] animation, byte[][] mode7DoorFrames)
    {
        this.tiles = tiles;
        this.normal = new(normal);
        this.escape = new(escape, this.normal);
        animationSeeds = animation[1];
        for (int phase = 0; phase < CeresDoorVisualRomData.AnimationRowCount / 2; phase++)
        for (int color = 0; color < CeresDoorVisualRomData.AnimationColorCount; color++)
            if (animation[phase][color] != AnimationRamp(phase, color))
                animationPhaseResiduals.Add(phase * CeresDoorVisualRomData.AnimationColorCount + color, animation[phase][color]);
        for (int row = CeresDoorVisualRomData.AnimationRowCount / 2; row < CeresDoorVisualRomData.AnimationRowCount; row++)
        for (int color = 0; color < CeresDoorVisualRomData.AnimationColorCount; color++)
            if (animation[row][color] != AnimationColor(row, color))
                animationRowEdits.Add(row * CeresDoorVisualRomData.AnimationColorCount + color, animation[row][color]);
        for (int frame = 0; frame < mode7DoorFrames.Length; frame++)
        for (int index = 0; index < CeresDoorVisualRomData.Mode7FrameByteCount; index++)
            if (mode7DoorFrames[frame][index] != PlatformTile(frame, index))
                platformEdits.Add(frame * CeresDoorVisualRomData.Mode7FrameByteCount + index, mode7DoorFrames[frame][index]);
    }

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

    public void LoadTiles(SnesVram vram) =>
        tiles.LoadTo(vram, CeresDoorVisualRomData.TileVramDestination);

    public void LoadNormalColors(SnesCgram cgram, int destination)
    {
        Ensure.NotNull(cgram);
        if (destination < 0 || destination + CeresDoorVisualRomData.SetupColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int color = 0; color < CeresDoorVisualRomData.SetupColorCount; color++)
            cgram.SetColor(destination + color, normal.ColorAt(color));
    }

    public void LoadEscapeColors(SnesCgram cgram, int destination)
    {
        Ensure.NotNull(cgram);
        if (destination < 0 || destination + CeresDoorVisualRomData.SetupColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int color = 0; color < CeresDoorVisualRomData.SetupColorCount; color++)
            cgram.SetColor(destination + color, EscapeColor(color));
    }

    private ushort EscapeColor(int color) => escape.ColorAt(color);

    public void LoadAnimationColors(SnesCgram cgram, int row)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)row >= CeresDoorVisualRomData.AnimationRowCount) throw new IndexOutOfRangeException();
        for (int color = 0; color < CeresDoorVisualRomData.AnimationColorCount; color++)
            cgram.SetColor(CeresDoorVisualRomData.AnimationTargetColor + color, AnimationColor(row, color));
    }

    private ushort AnimationColor(int row, int color)
    {
        int key = row * CeresDoorVisualRomData.AnimationColorCount + color;
        if (animationRowEdits.TryGetValue(key, out ushort edited)) return edited;
        int phase = Math.Min(row, CeresDoorVisualRomData.AnimationRowCount - 1 - row);
        int phaseKey = phase * CeresDoorVisualRomData.AnimationColorCount + color;
        return animationPhaseResiduals.TryGetValue(phaseKey, out ushort residual) ? residual : AnimationRamp(phase, color);
    }

    /// <summary>
    /// $A6:F871-$F8EC: the eight rows mirror four phases. RGB5 channels change by five
    /// per phase relative to phase1, clamped to0..31. Six seed choices and four stock
    /// deviations remain unresolved; custom rows retain all independent differences.
    /// </summary>
    private ushort AnimationRamp(int phase, int color)
    {
        ushort seed = animationSeeds[color];
        int delta = 5 * (1 - phase);
        int red = Math.Clamp((seed & 31) + delta, 0, 31);
        int green = Math.Clamp(((seed >> 5) & 31) + delta, 0, 31);
        int blue = Math.Clamp(((seed >> 10) & 31) + delta, 0, 31);
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>$A6:F918/F91C: resolve shared canonical platform imagery or an independent supplied cell edit.</summary>
    private byte PlatformTile(int frame, int index)
    {
        byte stock = CeresMode7TransferDefinitions.PlatformTile(frame, index);
        return platformEdits.TryGetValue(frame * CeresDoorVisualRomData.Mode7FrameByteCount + index, out byte edit) ? edit : stock;
    }

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

public sealed record CeresDoorVisualDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Normal { get; init; }
    public required PaletteRgb5[] Escape { get; init; }
    public required PaletteRgb5[][] Animation { get; init; }
    public required int[][] Mode7DoorFrames { get; init; }
}

/// <summary>Stable host filenames for Ceres-door graphics and palettes.</summary>
public static class CeresDoorVisualFormat
{
    public const string TilesFileName = "ceres-door-tiles.png";
    public const string ColorsFileName = "ceres-door-colors.json";
}
