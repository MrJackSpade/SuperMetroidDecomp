using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Ceres-door tile DMA and RGB5 palettes; actor timing remains engine-owned.</summary>
public sealed class CeresDoorVisualCatalog
{
    private readonly RoomCharacterAtlas tiles;
    private readonly ushort[] normal;
    private readonly ushort[] escape;
    private readonly ushort[][] animation;
    private readonly byte[][] mode7DoorFrames;

    private CeresDoorVisualCatalog(RoomCharacterAtlas tiles, ushort[] normal,
        ushort[] escape, ushort[][] animation, byte[][] mode7DoorFrames)
    {
        this.tiles = tiles;
        this.normal = normal;
        this.escape = escape;
        this.animation = animation;
        this.mode7DoorFrames = mode7DoorFrames;
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

    public void LoadNormalColors(SnesCgram cgram, int destination) =>
        LoadColors(cgram, normal, destination);

    public void LoadEscapeColors(SnesCgram cgram, int destination) =>
        LoadColors(cgram, escape, destination);

    public void LoadAnimationColors(SnesCgram cgram, int row) =>
        LoadColors(cgram, animation[row], CeresDoorVisualRomData.AnimationTargetColor);

    public void LoadMode7DoorFrame(SnesVram vram, int frame)
    {
        ArgumentNullException.ThrowIfNull(vram);
        vram.LoadMode7MapBytes(mode7DoorFrames[frame],
            CeresDoorVisualRomData.Mode7DestinationWord);
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

    private static void LoadColors(SnesCgram cgram, ushort[] colors, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (destination < 0 || destination + colors.Length > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int index = 0; index < colors.Length; index++)
            cgram.SetColor(destination + index, colors[index]);
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
