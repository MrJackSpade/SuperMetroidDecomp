using SuperMetroid.Core.Hardware;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Four editable 3x2 Samus-eye BG2 rectangles; blink timing stays in code.</summary>
public sealed class IntroEyeTilemapPresentation
{
    /// <summary>Four compiled 3-by-2 eye patches in blink selector order.</summary>
    internal readonly EyeRectangle[] frames;

    /// <summary>Retains edited eye words only when they differ from the calculated native atlas grid.</summary>
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

    /// <summary>One eye patch whose stock BG words can be calculated from its first tile.</summary>
    internal sealed class EyeRectangle
    {
        /// <summary>Blink frame selecting the patch's first tile.</summary>
        private readonly int frame;
        /// <summary>Complete authored words when any cell differs from the native grid.</summary>
        internal readonly ushort[]? supplied;
        /// <summary>Detects whether supplied words match the six-cell native patch.</summary>
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
        /// <summary>Calculates a cell's BG word from the frame anchor and 16-tile row stride.</summary>
        private ushort Calculate(int cell) => unchecked((ushort)(IntroEyeTilemapFormat.FirstWord(frame) +
            cell % IntroEyeTilemapFormat.Columns + cell / IntroEyeTilemapFormat.Columns * IntroEyeTilemapFormat.RowStride));
        /// <summary>Supplied override words or a reconstructed native rectangle.</summary>
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
        Ensure.NotNull(json);
        IntroEyeTilemapDocument document = JsonAssetDocument.Read<IntroEyeTilemapDocument>(
            json, ArtworkJson.Options, "opening eye tilemap");
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
                TileFlip flips =
                    (cell.FlipX ? TileFlip.Horizontal : 0) |
                    (cell.FlipY ? TileFlip.Vertical : 0);
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
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
