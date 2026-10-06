using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Four editable 3x2 Samus-eye BG2 rectangles; blink timing stays in code.</summary>
public sealed class IntroEyeTilemapPresentation
{
    private readonly EyeRectangle[] frames;

    private IntroEyeTilemapPresentation(ushort[][] frames) => this.frames = frames.Select((words, index) => new EyeRectangle(words, index)).ToArray();

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
        private readonly int frame;
        private readonly ushort[]? supplied;
        internal EyeRectangle(ushort[] words, int frame)
        {
            this.frame = frame;
            for (int cell = 0; cell < words.Length; cell++)
            {
                if (words[cell] == Calculate(cell)) continue;
                supplied = words;
                break;
            }
        }
        private ushort Calculate(int cell) => unchecked((ushort)(IntroEyeTilemapFormat.FirstWord(frame) +
            cell % IntroEyeTilemapFormat.Columns + cell / IntroEyeTilemapFormat.Columns * IntroEyeTilemapFormat.NativeRowStride));
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

/// <summary>Stable identities and calculated atlas layout for the four portrait eye drawings. The three selected patch anchors and display design identify authored portrait content; changing them invents different drawings. Timing and pixels are outside this retained scope.</summary>
public static class IntroEyeTilemapFormat
{
    public const int Version = 1;
    public const int FrameCount = 4;
    public const int Columns = 3;
    public const int Rows = 2;
    public const int CellsPerFrame = Columns * Rows;
    /// <summary>$8C:D785/D795/D7A5/D7B5: second tile row begins16 characters after the first; each selected patch uses the native 16-column artwork atlas.</summary>
    internal const int NativeRowStride = 16;
    /// <summary>$8C:D785: open-eye patch starts at tile389 of the installed95:F90E atlas.</summary>
    private const int OpenTile = 0x389;
    /// <summary>$8C:D795: half-open-eye patch starts at tile31D of the same atlas.</summary>
    private const int HalfOpenTile = 0x31d;
    /// <summary>$8C:D7A5: closed-eye patch starts at tile33A; deadpan patch atD7B5 lies immediately to its right.</summary>
    private const int ClosedTile = 0x33a;
    /// <summary>$8C:D785-D7BF: selected portrait palette three, no priority or flips.</summary>
    private const int PortraitPalette = 3;
    internal static ushort FirstWord(int frame)
    {
        int tile = frame switch
        {
            0 => OpenTile,
            1 => HalfOpenTile,
            2 => ClosedTile,
            3 => ClosedTile + Columns,
            _ => throw new ArgumentOutOfRangeException(nameof(frame)),
        };
        return SnesBgTilemapWord.Create(tile, PortraitPalette, false, default).Raw;
    }
    public const string FileName = "intro-samus-eye-frames.json";

    public static string FrameId(int index)
    {
        if ((uint)index >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return $"eye-frame-{index}";
    }
}
