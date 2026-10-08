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

    /// <summary>Horizontal pixel displacement from each pair's tens-digit origin to its ones-digit origin; stock spacing is eight pixels.</summary>
    public int DigitSpacing { get; }
    /// <summary>The shared OBJ palette selector packed into attribute bits 9 through 11, applied only to parts that inherit their palette.</summary>
    public ushort PaletteBits { get; }

    /// <summary>Draws the timer using installed visual data while retaining live BCD/countdown state.</summary>
    /// <param name="timer">Live timer supplying packed-BCD minutes, seconds and centiseconds and the integer pixel origin of its moving position.</param>
    /// <param name="oam">Destination sprite buffer; label parts and the three digit pairs are appended in that order without clearing existing sprites.</param>
    /// <remarks>The caller controls visibility. This method neither checks whether the timer is active nor advances its countdown.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="timer"/> or <paramref name="oam"/> is null.</exception>
    /// <exception cref="InvalidOperationException">A timer byte contains a nondecimal BCD nibble; earlier sprite parts may already have been emitted.</exception>
    public void Draw(EscapeTimer timer, OamBuffer oam)
    {
        ArgumentNullException.ThrowIfNull(timer);
        ArgumentNullException.ThrowIfNull(oam);

        DrawFrame(EscapeTimerPresentationDefinitions.LabelFrame, Anchor("Label"));
        DrawPair(timer.MinutesBcd, Anchor("Minutes"));
        DrawPair(timer.SecondsBcd, Anchor("Seconds"));
        DrawPair(timer.CentisecondsBcd, Anchor("Centiseconds"));

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
            (frames.TryGetValue(name, out var supplied) ? supplied : EscapeTimerPresentationDefinitions.DefaultFrame(name))
                .DrawOnScreen(oam, x, y, PaletteBits);
        }
    }

    private MapLabelPoint Anchor(string name) => anchors.TryGetValue(name, out var supplied)
        ? supplied : EscapeTimerPresentationDefinitions.DefaultAnchor(name);

    /// <summary>Validates an authored timer layout and compiles its ordered sprite parts and pixel anchors independently of the mutable document.</summary>
    /// <param name="json">UTF-8 JSON read from the current position to the end; the caller retains ownership of the stream.</param>
    /// <returns>A presentation that retains edited compositions and anchors, using calculated native geometry for entries that exactly match the stock layout.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed or ambiguous, required fields or exact frame/anchor names are missing, or values exceed the supported schema and OAM ranges.</exception>
    public static EscapeTimerPresentation Load(Stream json)
    {
        EscapeTimerPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<EscapeTimerPresentationDocument>(json,
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
            if (!compiled.SequenceEqual(EscapeTimerPresentationDefinitions.DefaultParts(name)))
                frames.Add(name, new SpriteComposition(compiled));
        }

        var anchors = new Dictionary<string, MapLabelPoint>(StringComparer.Ordinal);
        foreach (string name in EscapeTimerPresentationDefinitions.AnchorNames)
        {
            MapLabelPoint point = document.Anchors[name] ??
                throw new InvalidDataException($"Escape timer anchor {name} is null.");
            if (point.X is < -256 or > 255 || point.Y is < -224 or > 223)
                throw new InvalidDataException($"Escape timer anchor {name} is outside the supported screen-relative range.");
            if (point != EscapeTimerPresentationDefinitions.DefaultAnchor(name)) anchors.Add(name, point);
        }
        return new(frames, anchors, document.DigitSpacing, document.Palette);
    }

    /// <summary>Serializes and validates a timer document before writing any of its UTF-8 JSON bytes.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored layout whose collections are read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails the same schema, composition and anchor checks as <see cref="Load"/>.</exception>
    public static void Write(Stream output, EscapeTimerPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

/// <summary>Authoring schema for the timer's palette, digit placement and eleven named sprite compositions; its dictionary and array contents remain caller-mutable.</summary>
public sealed record EscapeTimerPresentationDocument
{
    /// <summary>Schema revision; loading requires <see cref="EscapeTimerPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Shared OBJ palette selector, 0 through 7, inherited by parts with a null palette; the cartridge timer uses selector 5.</summary>
    public required int Palette { get; init; }
    /// <summary>Signed horizontal pixel displacement from tens to ones in every digit pair, from -256 through 255; stock value is 8.</summary>
    public required int DigitSpacing { get; init; }
    /// <summary>Exactly Label, Minutes, Seconds and Centiseconds pixel offsets relative to the live timer origin; X is -256..255 and Y is -224..223.</summary>
    public required Dictionary<string, MapLabelPoint> Anchors { get; init; }
    /// <summary>Exactly Label and Digit.0 through Digit.9; each nonnull array contains zero to 128 ordered, nonnull OAM parts. The stock Label also supplies both digit separators.</summary>
    public required Dictionary<string, EscapeTimerVisualPart[]> Frames { get; init; }
}

/// <summary>One ordered OAM part using the timer's full nine-bit gameplay OBJ tile number.</summary>
public sealed record EscapeTimerVisualPart
{
    /// <summary>Signed horizontal pixel offset from the composition's anchor, from -256 through 255, encoded in the native nine-bit spritemap X field.</summary>
    public required int OffsetX { get; init; }
    /// <summary>Signed vertical pixel offset from the composition's anchor, from -128 through 127, encoded as a signed byte.</summary>
    public required int OffsetY { get; init; }
    /// <summary>Gameplay OBJ character number, 0 through 511 including its tile-page bit; this is not a column or row in the extracted timer artwork.</summary>
    public required int TileNumber { get; init; }
    /// <summary>Square sprite side length in pixels, either 8 or 16, selecting the small or large gameplay OBJ size.</summary>
    public required int Size { get; init; }
    /// <summary>OBJ priority tier from 0 through 3; stock timer parts use 3.</summary>
    public required int Priority { get; init; }
    /// <summary>Explicit OBJ palette selector from 0 through 7, or null to inherit the presentation's shared palette.</summary>
    public required int? Palette { get; init; }
    /// <summary>Whether the sprite's pixels are reflected horizontally without changing its authored anchor offset.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether the sprite's pixels are reflected vertically without changing its authored anchor offset.</summary>
    public required bool FlipY { get; init; }
}
