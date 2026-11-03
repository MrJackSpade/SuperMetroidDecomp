using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;


namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>Ceres Ridley's channel fades. Eye and body channels share the same biased integer quantizer.</summary>
internal sealed class CeresRidleyFade
{
    /// <summary>Chooses eye or body shade counts and their opposing fade phase directions.</summary>
    private readonly CeresRidleyFadeKind kind;
    /// <summary>Independent warm eye endpoint colors used when the eye fade is selected.</summary>
    private readonly EyePaint eyePaint;
    /// <summary>Selected body endpoint paints used only for body fade shades.</summary>
    private readonly CeresRidleyBodyPaint? bodyPaint;

    /// <summary>Source eye paint (also Ridley palette slots 12..14). The selected warm highlight/middle/shadow paint uses saturated red, a shared red step, three green levels and no blue. Regenerating these categorical material choices without the source inputs would select different eye paint. Initial/explosion palette aliases are evidence only; their independent containers are not exempted.</summary>
    internal readonly record struct EyePaint(int RedStep, int HighlightGreen, int MiddleGreen, int ShadowGreen)
    {
        /// <summary>Highest five-bit color channel value used to derive the eye reds.</summary>
        internal const int MaximumRgb5 = (1 << 5) - 1;

        /// <summary>Calculates one of the three warm eye shades from the selected material inputs.</summary>
        internal RgbColor Color(int shade)
        {
            int red = Math.Clamp(MaximumRgb5 - RedStep * shade, 0, MaximumRgb5);
            int green = shade switch { 0 => HighlightGreen, 1 => MiddleGreen, 2 => ShadowGreen, _ => throw new ArgumentOutOfRangeException(nameof(shade)) };
            return new(red, green, 0);
        }
    }
    /// <summary>Imported colors that differ from the common quantized fade formula.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> overrides;
    /// <summary>Number of colors in a shade of the selected fade table.</summary>
    private int Colors => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteDefinitions.EyeFadeColorCount : CeresRidleyPaletteDefinitions.BodyFadeColorCount;
    /// <summary>Number of fade steps for the selected table.</summary>
    private int Shades => kind == CeresRidleyFadeKind.Eyes
        ? CeresRidleyPaletteDefinitions.EyeFadeShadeCount : CeresRidleyPaletteDefinitions.BodyFadeShadeCount;

    /// <summary>Retains the selected endpoint paint and the exceptions to the shared fade formula.</summary>
    /// <param name="kind">Whether this is the eye or the body fade.</param>
    /// <param name="eyePaint">The eye endpoint paint; read only by an eye fade.</param>
    /// <param name="bodyPaint">The body endpoint paint of a body fade, or null for an eye fade.</param>
    /// <param name="overrides">The supplied colors the import found differing from the fade formula.</param>
    internal CeresRidleyFade(CeresRidleyFadeKind kind, EyePaint eyePaint, CeresRidleyBodyPaint? bodyPaint,
        Dictionary<ColourCell, RgbColor> overrides)
    {
        this.kind = kind;
        this.eyePaint = eyePaint;
        this.bodyPaint = bodyPaint;
        this.overrides = overrides;
    }

    /// <summary>Returns a supplied exception color when present, otherwise the calculated fade shade.</summary>
    internal RgbColor Resolve(int shade, int color)
    {
        Validate(shade, color);
        var key = new ColourCell(shade, color);
        return overrides.TryGetValue(key, out RgbColor edited) ? edited : Calculate(shade, color);
    }

    /// <summary>Scale each RGB5 channel through fifteen intervals. Both select a one-unit numerator rounding bias; this is a common fade composition rule, not a claim about the original authoring tool.</summary>
    private RgbColor Calculate(int shade, int color)
    {
        RgbColor endpoint = kind == CeresRidleyFadeKind.Eyes ? eyePaint.Color(color)
            : (bodyPaint ?? throw new InvalidOperationException("A body fade requires its body paint.")).Color(color);
        int phase = kind == CeresRidleyFadeKind.Eyes ? Shades - 1 - shade : shade;
        return Scale(endpoint, phase);
    }

    /// <summary>Shared fifteen-interval channel quantizer used by eye/body, retreat and zoom colors.</summary>
    internal static RgbColor Scale(RgbColor endpoint, int phase)
    {
        const int intervals = 15;
        const int bias = 1;
        return endpoint.Map((_, channel) => (channel * phase + bias) / intervals);
    }

    /// <summary>Rejects fade shades beyond the selected eye or body table.</summary>
    internal void ValidateShade(int shade)
    {
        if ((uint)shade >= Shades) throw new ArgumentOutOfRangeException(nameof(shade));
    }

    /// <summary>Checks both the table-specific shade range and the color count within each shade.</summary>
    private void Validate(int shade, int color)
    {
        ValidateShade(shade);
        if ((uint)color >= Colors) throw new ArgumentOutOfRangeException(nameof(color));
    }
}
