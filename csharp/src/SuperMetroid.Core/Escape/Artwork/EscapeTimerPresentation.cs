using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>The escape timer's editable layout: where its label and digit pairs sit and how far apart a pair's digits are.</summary>
public sealed class EscapeTimerPresentation
{
    /// <summary>Only edited screen-relative offsets; stock anchors are calculated when absent.</summary>
    internal readonly Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>> anchors;

    /// <summary>Retains validated anchor overrides and the digit spacing.</summary>
    private EscapeTimerPresentation(Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>> anchors, int digitSpacing)
    {
        this.anchors = anchors;
        DigitSpacing = digitSpacing;
    }

    /// <summary>Horizontal pixel displacement from each pair's tens-digit origin to its ones-digit origin; stock spacing is eight pixels.</summary>
    public int DigitSpacing { get; }

    /// <summary>Publishes the timer as image draws from its sheet while retaining live BCD/countdown state.</summary>
    /// <param name="timer">Live timer supplying packed-BCD minutes, seconds and centiseconds and the integer pixel origin of its moving position.</param>
    /// <param name="sprites">Frame receiving the TIME label and then the tens and ones digit of each pair, after any sprites already emitted.</param>
    /// <param name="sheet">The installed timer sheet, one frame per glyph.</param>
    /// <remarks>The caller controls visibility. This method neither checks whether the timer is active nor advances its countdown.</remarks>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidOperationException">A timer byte contains a nondecimal BCD nibble; earlier draws may already have been added.</exception>
    public void Draw(EscapeTimer timer, SpriteFrameBuffer sprites, SpriteSheet sheet)
    {
        Ensure.NotNull(timer);
        Ensure.NotNull(sprites);
        Ensure.NotNull(sheet);
        DrawFrame(EscapeTimerGlyph.Label, Anchor(EscapeTimerAnchor.Label));
        DrawPair(timer.Minutes, Anchor(EscapeTimerAnchor.Minutes));
        DrawPair(timer.Seconds, Anchor(EscapeTimerAnchor.Seconds));
        DrawPair(timer.Centiseconds, Anchor(EscapeTimerAnchor.Centiseconds));

        void DrawPair(PackedDecimal pair, Offset16<SpritePixels> anchor)
        {
            if (!pair.IsValid)
                throw new InvalidOperationException($"Cannot draw invalid packed-BCD timer byte ${pair.Raw:X2}.");
            DrawFrame(EscapeTimerPresentationDefinitions.DigitGlyph(pair.Tens), anchor);
            DrawFrame(EscapeTimerPresentationDefinitions.DigitGlyph(pair.Ones),
                anchor + new Offset16<SpritePixels>(checked((short)DigitSpacing), 0));
        }

        void DrawFrame(EscapeTimerGlyph glyph, Offset16<SpritePixels> anchor) =>
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.EscapeTimer),
                sheet.Frame(EscapeTimerPresentationDefinitions.FrameName(glyph)),
                timer.PixelPosition + new Offset16<ScreenPixels>(anchor.X, anchor.Y), SpriteMirror.None));
    }

    /// <summary>Resolves an edited timer anchor or its calculated stock position.</summary>
    private Offset16<SpritePixels> Anchor(EscapeTimerAnchor anchor) => anchors.TryGetValue(anchor, out var supplied)
        ? supplied : EscapeTimerPresentationDefinitions.DefaultAnchor(anchor);

    /// <summary>Validates an authored timer layout.</summary>
    /// <param name="json">UTF-8 JSON read from the current position to the end; the caller retains ownership of the stream.</param>
    /// <returns>A presentation that retains edited anchors, using calculated stock anchors for entries that match them.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, of another revision, or has missing or out-of-range anchors or spacing.</exception>
    public static EscapeTimerPresentation Load(Stream json)
    {
        EscapeTimerPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<EscapeTimerPresentationDocument>(json,
                ArtworkJson.Options) ??
                throw new InvalidDataException("Escape timer presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid escape timer presentation JSON.", error);
        }

        if (document.Version != EscapeTimerPresentationDefinitions.Version || document.Anchors is null ||
            document.DigitSpacing is < -256 or > 255)
            throw new InvalidDataException($"Escape timer presentation requires version {EscapeTimerPresentationDefinitions.Version}, valid anchors and spacing.");
        if (document.Anchors.Count != EscapeTimerPresentationDefinitions.Anchors.Count ||
            EscapeTimerPresentationDefinitions.Anchors.Any(anchor => !document.Anchors.ContainsKey(EscapeTimerPresentationDefinitions.AnchorName(anchor))))
            throw new InvalidDataException("Escape timer presentation requires Label, Minutes, Seconds and Centiseconds anchors exactly once.");

        var anchors = new Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>>();
        foreach (EscapeTimerAnchor anchor in EscapeTimerPresentationDefinitions.Anchors)
        {
            Offset16<SpritePixels> point = document.Anchors[EscapeTimerPresentationDefinitions.AnchorName(anchor)];
            if (point.X is < -256 or > 255 || point.Y is < -224 or > 223)
                throw new InvalidDataException($"Escape timer anchor {anchor} is outside the supported screen-relative range.");
            if (point != EscapeTimerPresentationDefinitions.DefaultAnchor(anchor)) anchors.Add(anchor, point);
        }
        return new(anchors, document.DigitSpacing);
    }

    /// <summary>Serializes and validates a timer document before writing any of its UTF-8 JSON bytes.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored layout whose collections are read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails the same schema and anchor checks as <see cref="Load"/>.</exception>
    public static void Write(Stream output, EscapeTimerPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}
