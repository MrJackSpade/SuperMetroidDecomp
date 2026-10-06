using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Four editable 3x2 Samus-eye BG2 rectangles; blink timing stays in code.</summary>
public sealed class IntroEyeTilemapPresentation
{
    private readonly EyeRectangle[] frames;

    private IntroEyeTilemapPresentation(ushort[][] frames) => this.frames = frames.Select(words => new EyeRectangle(words)).ToArray();

    /// <summary>Identity of every selected eye rectangle in its compiled blink-selector order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(IntroEyeTilemapPresentation), content =>
    {
        content.Append("frames", frames.Length);
        foreach (EyeRectangle frame in frames)
            content.AppendWords("frame", frame.Words);
    });

    public ReadOnlySpan<ushort> FrameWords(int index)
    {
        if ((uint)index >= frames.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return frames[index].Words;
    }

    private sealed class EyeRectangle
    {
        private readonly ushort origin;
        private readonly ushort[]? supplied;
        internal EyeRectangle(ushort[] words)
        {
            origin = words[0];
            for (int cell = 0; cell < words.Length; cell++)
            {
                if (words[cell] == Calculate(cell)) continue;
                supplied = words;
                break;
            }
        }
        private ushort Calculate(int cell) => unchecked((ushort)(origin +
            cell % IntroEyeTilemapFormat.Columns + cell / IntroEyeTilemapFormat.Columns * IntroEyeTilemapFormat.UnresolvedNativeRowStride));
        internal ReadOnlySpan<ushort> Words
        {
            get
            {
                if (supplied is not null) return supplied;
                var output = new ushort[IntroEyeTilemapFormat.CellsPerFrame];
                for (int cell = 0; cell < output.Length; cell++) output[cell] = Calculate(cell);
                return output;
            }
        }
    }
    public static IntroEyeTilemapPresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroEyeTilemapDocument document = JsonAssetDocument.Read<IntroEyeTilemapDocument>(
            json, MapPresentationFormat.JsonOptions, "opening eye tilemap");
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
    /// <summary>$8C:D785/D795/D7A5/D7B5: second tile row begins16 characters after the first; this selected atlas geometry remains required.</summary>
    internal const int UnresolvedNativeRowStride = 16;
    public const string FileName = "intro-samus-eye-frames.json";

    public static string FrameId(int index)
    {
        if ((uint)index >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return $"eye-frame-{index}";
    }
}
