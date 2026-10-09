using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Four editable 3x2 Samus-eye BG2 rectangles; blink timing stays in code.</summary>
public sealed class IntroEyeTilemapPresentation
{
    /// <summary>Validated eye rectangles in the stable blink-selector order.</summary>
    private readonly EyeRectangle[] frames;

    /// <summary>Creates a presentation from the four packed frames after JSON validation and tilemap-word encoding.</summary>
    /// <param name="frames">Six-word arrays in open, half-open, closed, and deadpan selector order.</param>
    private IntroEyeTilemapPresentation(ushort[][] frames) => this.frames = frames.Select((words, index) => new EyeRectangle(words, index)).ToArray();

    /// <summary>Identity of every selected eye rectangle in its compiled blink-selector order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(IntroEyeTilemapPresentation), content =>
    {
        content.Append("frames", frames.Length);
        foreach (EyeRectangle frame in frames)
            content.AppendWords("frame", frame.Words);
    });

    /// <summary>Gets the six packed BG tilemap words for one blink frame.</summary>
    public ReadOnlySpan<ushort> FrameWords(int index)
    {
        if ((uint)index >= frames.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return frames[index].Words;
    }

    /// <summary>Provides one 3-by-2 eye frame, calculating atlas-default words unless imported content overrides them.</summary>
    private sealed class EyeRectangle
    {
        /// <summary>Selector index determining the frame's native tilemap anchor.</summary>
        private readonly int frame;
        /// <summary>Imported words retained when at least one cell differs from the calculated native rectangle.</summary>
        private readonly ushort[]? supplied;

        /// <summary>Retains an edited word array only when it differs from the native atlas-derived layout.</summary>
        /// <param name="words">Six row-major BG tilemap words for this rectangle.</param>
        /// <param name="frame">Zero-based blink-selector index used to calculate default tile identities.</param>
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
        /// <summary>Calculates one default tilemap word by offsetting from this frame's anchor using the native row stride.</summary>
        /// <param name="cell">Zero-based row-major cell index within the 3-by-2 rectangle.</param>
        /// <returns>The packed BG tilemap word for that cell.</returns>
        private ushort Calculate(int cell) => unchecked((ushort)(IntroEyeTilemapFormat.FirstWord(frame) +
            cell % IntroEyeTilemapFormat.Columns + cell / IntroEyeTilemapFormat.Columns * IntroEyeTilemapFormat.NativeRowStride));

        /// <summary>Gets imported words when the frame contains edits, otherwise materializes the calculated native rectangle.</summary>
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
    /// <summary>Loads and validates the four ordered Samus portrait eye rectangles.</summary>
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

    /// <summary>Validates and writes an opening-eye tilemap document as JSON.</summary>
    public static void Write(Stream json, IntroEyeTilemapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Defines the complete ordered set of opening portrait eye frames.</summary>
public sealed record IntroEyeTilemapDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets exactly four frames in blink-selector order.</summary>
    public required IntroEyeTilemapFrame[] Frames { get; init; }
}

/// <summary>Defines one stable 3-by-2 portrait eye tilemap rectangle.</summary>
public sealed record IntroEyeTilemapFrame
{
    /// <summary>Gets the stable <c>eye-frame-N</c> identity for this array position.</summary>
    public required string Id { get; init; }
    /// <summary>Gets six row-major packed-tile components.</summary>
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}

/// <summary>Stable identities and calculated atlas layout for the four portrait eye drawings. The three selected patch anchors and display design identify authored portrait content; changing them invents different drawings. Timing and pixels are outside this retained scope.</summary>
public static class IntroEyeTilemapFormat
{
    /// <summary>Supported eye-frame document schema revision.</summary>
    public const int Version = 1;
    /// <summary>Number of ordered eye drawings in the blink selector.</summary>
    public const int FrameCount = 4;
    /// <summary>Number of BG cells across each eye rectangle.</summary>
    public const int Columns = 3;
    /// <summary>Number of BG cells down each eye rectangle.</summary>
    public const int Rows = 2;
    /// <summary>Number of row-major tilemap cells in each frame.</summary>
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
    /// <summary>Returns the packed tilemap anchor for the open, half-open, closed, or deadpan portrait drawing.</summary>
    /// <param name="frame">Zero-based selector index from zero through three.</param>
    /// <returns>The anchor word using the selected portrait palette and native atlas tile.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The selector index is outside the four authored drawings.</exception>
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
    /// <summary>JSON filename containing the four editable eye rectangles.</summary>
    public const string FileName = "intro-samus-eye-frames.json";

    /// <summary>Builds the stable frame identity for a zero-based selector index.</summary>
    public static string FrameId(int index)
    {
        if ((uint)index >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return $"eye-frame-{index}";
    }
}
