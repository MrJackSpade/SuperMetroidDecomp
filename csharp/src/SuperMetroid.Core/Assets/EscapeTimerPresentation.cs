using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable escape-timer spritemap compositions, palette and screen-relative anchors.</summary>
public sealed class EscapeTimerPresentation
{
    private readonly Dictionary<string, SpriteComposition> frames;
    private readonly Dictionary<string, MapLabelPoint> anchors;

    private EscapeTimerPresentation(Dictionary<string, SpriteComposition> frames,
        Dictionary<string, MapLabelPoint> anchors, int digitSpacing, int palette)
    {
        this.frames = frames;
        this.anchors = anchors;
        DigitSpacing = digitSpacing;
        PaletteBits = SnesObjAttributeWord.Create(0, palette, 0).PaletteBits;
    }

    public int DigitSpacing { get; }
    public ushort PaletteBits { get; }

    /// <summary>Draws the timer using installed visual data while retaining live BCD/countdown state.</summary>
    public void Draw(EscapeTimer timer, OamBuffer oam)
    {
        ArgumentNullException.ThrowIfNull(timer);
        ArgumentNullException.ThrowIfNull(oam);

        DrawFrame(EscapeTimerPresentationDefinitions.LabelFrame, anchors["Label"]);
        DrawPair(timer.MinutesBcd, anchors["Minutes"]);
        DrawPair(timer.SecondsBcd, anchors["Seconds"]);
        DrawPair(timer.CentisecondsBcd, anchors["Centiseconds"]);

        void DrawPair(byte packedBcd, MapLabelPoint anchor)
        {
            int tens = packedBcd >> 4;
            int ones = packedBcd & 0x0f;
            if (tens > 9 || ones > 9)
                throw new InvalidOperationException($"Cannot draw invalid packed-BCD timer byte ${packedBcd:X2}.");
            DrawFrame(EscapeTimerPresentationDefinitions.DigitFrame(tens), anchor);
            DrawFrame(EscapeTimerPresentationDefinitions.DigitFrame(ones),
                anchor with { X = anchor.X + DigitSpacing });
        }

        void DrawFrame(string name, MapLabelPoint anchor)
        {
            ushort x = unchecked((ushort)(timer.XPixel + anchor.X));
            ushort y = unchecked((ushort)(timer.YPixel + anchor.Y));
            frames[name].DrawOnScreen(oam, x, y, PaletteBits);
        }
    }

    public static EscapeTimerPresentation Load(Stream json)
    {
        EscapeTimerPresentationDocument document;
        try
        {
            document = JsonSerializer.Deserialize<EscapeTimerPresentationDocument>(json,
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Escape timer presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid escape timer presentation JSON.", error);
        }

        if (document.Version != EscapeTimerPresentationDefinitions.Version ||
            document.Frames is null || document.Anchors is null ||
            document.DigitSpacing is < -256 or > 255 || (uint)document.Palette > 7)
            throw new InvalidDataException("Escape timer presentation requires version 1, valid anchors, frames, spacing and palette.");

        string[] expectedFrames = [EscapeTimerPresentationDefinitions.LabelFrame,
            .. Enumerable.Range(0, 10).Select(EscapeTimerPresentationDefinitions.DigitFrame)];
        if (document.Frames.Count != expectedFrames.Length ||
            expectedFrames.Any(name => !document.Frames.ContainsKey(name)))
            throw new InvalidDataException("Escape timer presentation requires Label and Digit.0 through Digit.9 exactly once.");
        if (document.Anchors.Count != EscapeTimerPresentationDefinitions.AnchorNames.Length ||
            EscapeTimerPresentationDefinitions.AnchorNames.Any(name => !document.Anchors.ContainsKey(name)))
            throw new InvalidDataException("Escape timer presentation requires Label, Minutes, Seconds and Centiseconds anchors exactly once.");

        var frames = new Dictionary<string, SpriteComposition>(StringComparer.Ordinal);
        foreach (string name in expectedFrames)
        {
            EscapeTimerVisualPart[] parts = document.Frames[name] ??
                throw new InvalidDataException($"Escape timer frame {name} is null.");
            if (parts.Length > EscapeTimerPresentationDefinitions.MaximumParts)
                throw new InvalidDataException($"Escape timer frame {name} exceeds OAM capacity.");
            var compiled = new CompiledSpritePart[parts.Length];
            for (int index = 0; index < parts.Length; index++)
            {
                EscapeTimerVisualPart part = parts[index] ??
                    throw new InvalidDataException($"Escape timer frame {name} part {index} is null.");
                if (part.OffsetX is < -256 or > 255 || part.OffsetY is < -128 or > 127 ||
                    part.TileNumber is < 0 or > 511 || part.Size is not (8 or 16) ||
                    (uint)part.Priority > 3 || part.Palette is < 0 or > 7)
                    throw new InvalidDataException($"Escape timer frame {name} part {index} has invalid OAM presentation data.");
                var attributes = SnesObjAttributeWord.Create(part.TileNumber, part.Palette ?? 0,
                    part.Priority, (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                    (part.FlipY ? SnesTileFlipFlags.Vertical : 0));
                compiled[index] = new(SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                    unchecked((byte)(sbyte)part.OffsetY), attributes, part.Palette is null);
            }
            frames.Add(name, new SpriteComposition(compiled));
        }

        var anchors = new Dictionary<string, MapLabelPoint>(StringComparer.Ordinal);
        foreach (string name in EscapeTimerPresentationDefinitions.AnchorNames)
        {
            MapLabelPoint point = document.Anchors[name] ??
                throw new InvalidDataException($"Escape timer anchor {name} is null.");
            if (point.X is < -256 or > 255 || point.Y is < -224 or > 223)
                throw new InvalidDataException($"Escape timer anchor {name} is outside the supported screen-relative range.");
            anchors.Add(name, point);
        }
        return new(frames, anchors, document.DigitSpacing, document.Palette);
    }

    public static void Write(Stream output, EscapeTimerPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

public sealed record EscapeTimerPresentationDocument
{
    public required int Version { get; init; }
    public required int Palette { get; init; }
    public required int DigitSpacing { get; init; }
    public required Dictionary<string, MapLabelPoint> Anchors { get; init; }
    public required Dictionary<string, EscapeTimerVisualPart[]> Frames { get; init; }
}

/// <summary>One ordered OAM part using the timer's full nine-bit gameplay OBJ tile number.</summary>
public sealed record EscapeTimerVisualPart
{
    public required int OffsetX { get; init; }
    public required int OffsetY { get; init; }
    public required int TileNumber { get; init; }
    public required int Size { get; init; }
    public required int Priority { get; init; }
    public required int? Palette { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}
