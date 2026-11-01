using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>The escape timer's editable layout: where its label and digit pairs sit and how far apart a pair's digits are.</summary>
public sealed class EscapeTimerPresentation
{
    /// <summary>The base of the decimal digits each pair shows.</summary>
    private const int DigitBase = 10;

    /// <summary>Only edited screen-relative offsets; stock anchors are calculated when absent.</summary>
    internal readonly Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>> anchors;

    /// <summary>Retains the anchor overrides and digit spacing the import pipeline validated.</summary>
    internal EscapeTimerPresentation(Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>> anchors, int digitSpacing)
    {
        this.anchors = anchors;
        DigitSpacing = digitSpacing;
    }

    /// <summary>Horizontal pixel displacement from each pair's tens-digit origin to its ones-digit origin; stock spacing is eight pixels.</summary>
    public int DigitSpacing { get; }

    /// <summary>Publishes the timer as image draws from its sheet: its label, then two decimal digits for each of its minutes, seconds and centiseconds.</summary>
    /// <param name="timer">Live timer supplying its readout and the integer pixel origin of its moving position.</param>
    /// <param name="sprites">Frame receiving the TIME label and then the tens and ones digit of each pair, after any sprites already emitted.</param>
    /// <param name="sheet">The installed timer sheet, one frame per glyph.</param>
    /// <remarks>The caller controls visibility. This method neither checks whether the timer is active nor advances its countdown.</remarks>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public void Draw(EscapeTimer timer, SpriteFrameBuffer sprites, SpriteSheet sheet)
    {
        Ensure.NotNull(timer);
        Ensure.NotNull(sprites);
        Ensure.NotNull(sheet);
        EscapeTimeReadout readout = timer.Readout;
        DrawFrame(EscapeTimerGlyph.Label, Anchor(EscapeTimerAnchor.Label));
        DrawPair(readout.Minutes, Anchor(EscapeTimerAnchor.Minutes));
        DrawPair(readout.Seconds, Anchor(EscapeTimerAnchor.Seconds));
        DrawPair(readout.Centiseconds, Anchor(EscapeTimerAnchor.Centiseconds));

        void DrawPair(int value, Offset16<SpritePixels> anchor)
        {
            DrawFrame(EscapeTimerComposition.DigitGlyph(value / DigitBase), anchor);
            DrawFrame(EscapeTimerComposition.DigitGlyph(value % DigitBase),
                anchor + new Offset16<SpritePixels>(DigitSpacing, 0));
        }

        void DrawFrame(EscapeTimerGlyph glyph, Offset16<SpritePixels> anchor) =>
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.EscapeTimer),
                sheet.Frame(EscapeTimerComposition.FrameName(glyph)),
                timer.PixelPosition + new Offset16<ScreenPixels>(anchor.X, anchor.Y), SpriteMirror.None));
    }

    /// <summary>Resolves an edited timer anchor or its calculated stock position.</summary>
    private Offset16<SpritePixels> Anchor(EscapeTimerAnchor anchor) => anchors.TryGetValue(anchor, out var supplied)
        ? supplied : EscapeTimerComposition.DefaultAnchor(anchor);
}
