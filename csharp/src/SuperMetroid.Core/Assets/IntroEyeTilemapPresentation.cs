using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Four editable 3x2 Samus-eye BG2 rectangles; blink timing stays in code.</summary>
public sealed class IntroEyeTilemapPresentation
{
    private readonly ushort[][] frames;

    private IntroEyeTilemapPresentation(ushort[][] frames) => this.frames = frames;

    public ReadOnlySpan<ushort> FrameWords(int index)
    {
        if ((uint)index >= frames.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return frames[index];
    }

    public static IntroEyeTilemapPresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroEyeTilemapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<IntroEyeTilemapDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Opening eye tilemap JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid opening eye tilemap JSON.", error);
        }
        if (document.Version != IntroEyeTilemapFormat.Version ||
            document.Frames is not { Length: IntroEyeTilemapFormat.FrameCount })
            throw new InvalidDataException("Opening eye tilemap requires four ordered frames.");

        var frames = new ushort[IntroEyeTilemapFormat.FrameCount][];
        for (int frame = 0; frame < frames.Length; frame++)
        {
            IntroEyeTilemapFrame? source = document.Frames[frame];
            if (source is null || source.Id != IntroEyeTilemapFormat.FrameId(frame) ||
                source.Cells is not { Length: IntroEyeTilemapFormat.CellsPerFrame })
                throw new InvalidDataException($"Opening eye frame {frame} needs its stable ID and six cells.");
            frames[frame] = new ushort[IntroEyeTilemapFormat.CellsPerFrame];
            for (int cellIndex = 0; cellIndex < frames[frame].Length; cellIndex++)
            {
                RoomBackgroundTilemapCell? cell = source.Cells[cellIndex];
                if (cell is null ||
                    (uint)cell.TileColumn >= RoomBackgroundTilemapFormat.TileColumns ||
                    (uint)cell.TileRow >= RoomBackgroundTilemapFormat.TileRows ||
                    (uint)cell.Palette >= RoomBackgroundTilemapFormat.PaletteCount)
                    throw new InvalidDataException(
                        $"Opening eye frame {frame} cell {cellIndex} has an invalid tile or palette.");
                SnesTileFlipFlags flips =
                    (cell.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (cell.FlipY ? SnesTileFlipFlags.Vertical : 0);
                frames[frame][cellIndex] = SnesBgTilemapWord.Create(
                    cell.TileRow * RoomBackgroundTilemapFormat.TileColumns + cell.TileColumn,
                    cell.Palette, cell.Priority, flips).Raw;
            }
        }
        return new IntroEyeTilemapPresentation(frames);
    }

    public static void Write(Stream json, IntroEyeTilemapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record IntroEyeTilemapDocument
{
    public required int Version { get; init; }
    public required IntroEyeTilemapFrame[] Frames { get; init; }
}

public sealed record IntroEyeTilemapFrame
{
    public required string Id { get; init; }
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}

/// <summary>Stable names and native dimensions for the four portrait eye frames.</summary>
public static class IntroEyeTilemapFormat
{
    public const int Version = 1;
    public const int FrameCount = 4;
    public const int Columns = 3;
    public const int Rows = 2;
    public const int CellsPerFrame = Columns * Rows;
    public const string FileName = "intro-samus-eye-frames.json";

    public static string FrameId(int index)
    {
        if ((uint)index >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return $"eye-frame-{index}";
    }
}
