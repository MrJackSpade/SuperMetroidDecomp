using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Editable suit, suitless, and whiteout colors for Samus's death sequence.</summary>
/// <remarks>The suited base steps are the same logical artwork as the independently reviewed full-body bases. Their 25
/// shared inputs comprise 21 painted inks, Gravity 1/12 using existing Power dim tint red/green targets, and two unused
/// transparent payloads. These are categorical pixel colours, not samples indexed by a quantitative lighting input.
/// Fitting or reciting ink-to-colour choices would encode the artwork; colour zero is never drawn. This is the specific
/// #1165 nonsense disposition of those aliased base inputs. Shared suits and fade shades are calculated; independent
/// asset edits stay local.
///
/// The suitless base's colour zero is likewise an unused transparent payload: the death setup installs it as the death
/// colour set's first colour, which sprite drawing never shows. Its colour is not a visible shade or an input to another
/// opaque ink. Temporal fades preserve the selected value but do not make it visible. A numeric case or fitted
/// expression for that arbitrary unused payload would only recite the data; it is retained under the same specific
/// nonsense exception.
///
/// The remaining 13 visible suitless base components are artwork inputs: ink 1 red/green/blue, ink 4 red/green, ink 6
/// red/green/blue, inks 7 and 9 blue, ink 10 red/blue and ink 11 intensity. They choose the warm and tinted endpoint
/// colours and the local shade contrast of categorical sprite inks. The death setup and explosion install selected
/// death-flash steps; pixel indices select painted classes, not a measured illumination, material or hue coordinate
/// that determines those choices. Linear warm, tint-balance, midpoint, gray and temporal shade relationships are
/// calculated separately. Fitting or reciting the remaining colour anchors would merely encode the painting in another
/// form, the specific #1165 nonsense exception.
///
/// The final step chooses one uniform gray intensity 29 for all inks and suits. It replaces their different preceding
/// shades; it is not another eighth step, which would produce full white 31. This chosen foreground colour is likewise
/// retained as artwork, with neutral channels and all aliases derived.
/// The separately indexed whiteout curve is calculated by DefaultWhiteoutColor.
/// Explosion durations have their own independently documented disposition.</remarks>
public sealed class InstalledSamusDeathPalettes
{
    /// <summary>Three suited families in pointer-table order: Power, Varia, then Gravity.</summary>
    public const int SuitCount = 3;
    /// <summary>Sixteen RGB5 words per death-flash step, one OBJ colour set, including the transparent sprite ink at index zero.</summary>
    public const int ColorCount = SamusPaletteDefinitions.Common.ColoursPerSet;

    /// <summary>The yellow flash step: full red and green, zero blue.</summary>
    private static readonly RgbColor YellowFlashColor = new(31, 31, 0);
    /// <summary>The suitless death colours' ink 5: full red, green and blue.</summary>
    private static readonly RgbColor WhiteInkColor = new(31, 31, 31);

    /// <summary>Independent edits to selected suited RGB5 ink words.</summary>
    internal readonly Dictionary<TableColourPlace, RgbColor> suited = new();
    /// <summary>Channel overrides for suited whitening steps derived from base inks.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> suitedFadeInputs = new();
    /// <summary>Independent edits to selected suitless RGB5 ink words.</summary>
    internal readonly Dictionary<TableColourPlace, RgbColor> suitless = new();
    /// <summary>Channel overrides for suitless whitening steps derived from base inks.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> suitlessFadeInputs = new();
    /// <summary>Edits to neutral shades shared by death palette derivations.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> neutralInputs = new();
    /// <summary>Overrides to calculated warm shades without changing neighboring inks.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> warmShadeInputs = new();
    /// <summary>Overrides to calculated tint shades without changing neighboring inks.</summary>
    internal readonly Dictionary<int, LoadingPaletteInputView.Channels> tintShadeInputs = new();
    /// <summary>Owns the separately indexed two-rate whiteout shade edits.</summary>
    internal readonly WhiteoutInputs whiteout;
    /// <summary>Overrides to the death explosion's frame-to-death-flash-step selection.</summary>
    internal readonly Dictionary<int, ushort> explosionPaletteIndices = new();

    /// <summary>Captures selected death artwork as owned inputs and independent channel overrides; no caller arrays are retained or modified.</summary>
    /// <param name="suited">Three suit families, each with ten death-flash steps of sixteen RGB5 words, in pointer-table order.</param>
    /// <param name="suitless">Ten death-flash steps of sixteen RGB5 words in pointer-table order, separate from suited artwork.</param>
    /// <param name="whiteout">Twenty-two RGB5 shades corresponding to ShadesOfWhite, indexed by whiteout counter rather than explosion frame.</param>
    /// <param name="explosionPaletteIndices">Nine death-flash step selectors 0..9 corresponding to the odd bytes; animation durations are not part of this input.</param>
    /// <exception cref="ArgumentNullException">A top-level input array is null.</exception>
    /// <exception cref="InvalidDataException">An input has the wrong dimensions, a nested family or step is null, a color exceeds 32767, or an explosion selector exceeds 9.</exception>
    public InstalledSamusDeathPalettes(RgbColor[][][] suited, RgbColor[][] suitless,
        RgbColor[] whiteout, ushort[] explosionPaletteIndices)
    {
        Ensure.NotNull(suited);
        Ensure.NotNull(suitless);
        Ensure.NotNull(whiteout);
        Ensure.NotNull(explosionPaletteIndices);
        FixedGridShape deathSteps = new(SamusPaletteDefinitions.Death.PaletteCount, ColorCount);
        if (suited.Length != SuitCount ||
            suited.Any(family => !deathSteps.Fits(family)) ||
            !deathSteps.Fits(suitless) ||
            whiteout.Length != SamusPaletteDefinitions.Death.WhiteoutShadeCount ||
            explosionPaletteIndices.Length != SamusDeathExplosionTimingDefinitions.RecordCount ||
            explosionPaletteIndices.Any(index => index >= SamusPaletteDefinitions.Death.PaletteCount))
            throw new InvalidDataException("Samus death-palette artwork has an invalid shape or RGB5 color.");
        for (int suit = 0; suit < SuitCount; suit++)
        for (int palette = 0; palette < SamusPaletteDefinitions.Death.PaletteCount; palette++)
        for (int color = 0; color < ColorCount; color++)
        {
            int key = (suit * SamusPaletteDefinitions.Death.PaletteCount + palette) * ColorCount + color;
            var keyCell = new TableColourPlace((suit * SamusPaletteDefinitions.Death.PaletteCount + palette), color);
            RgbColor value = suited[suit][palette][color];
            if (palette == 0 && suit != 0 && value == suited[0][0][color]) continue;
            if (palette == 1 && (suit != 0 || color != 0) && value == suited[0][1][0]) continue;
            if (suit == 0 && palette == 1 && color == 0 && value == YellowFlashColor) continue;
            if (palette == 9 && value == suitless[9][0]) continue;
            if (palette is >= 2 and <= 8)
            {
                RgbColor expected = SamusPaletteFade.EighthTowardWhite(suited[suit][0][color], palette - 1);
                if (value != expected) suitedFadeInputs.Add(key, new(value, expected));
            }
            else this.suited.Add(keyCell, value);
        }
        for (int palette = 0; palette < SamusPaletteDefinitions.Death.PaletteCount; palette++)
        for (int color = 0; color < ColorCount; color++)
        {
            int key = palette * ColorCount + color;
            var keyCell = new TableColourPlace(palette, color);
            RgbColor value = suitless[palette][color];
            if (palette == 9 && color != 0 && value == suitless[9][0]) continue;
            if (palette == 1 && value == suitless[0][color]) continue;
            if (palette == 0 && color == 4)
            {
                RgbColor expected = SuitlessWarmEndpoint(suitless[0][1], value);
                warmShadeInputs.Add(key, new(value, expected, independentMask: 3));
                continue;
            }
            if (palette == 0 && color == 10)
            {
                if (TrySuitlessTintEndpoint(suitless[0][6], value, out RgbColor expected))
                    tintShadeInputs.Add(key, new(value, expected, independentMask: 5));
                else this.suitless.Add(keyCell, value);
                continue;
            }
            if (palette == 0 && color == 8)
            {
                RgbColor expected = SuitlessTintMidpoint(suitless[0][7], suitless[0][9]);
                if (value != expected) tintShadeInputs.Add(key, new(value, expected));
                continue;
            }
            if (palette == 0 && color is 7 or 9)
            {
                if (TrySuitlessTintShade(suitless[0][6], suitless[0][10], value.Blue, color, out RgbColor expected))
                    tintShadeInputs.Add(key, new(value, expected, independentMask: 4));
                else this.suitless.Add(keyCell, value);
                continue;
            }
            if (palette == 0 && color is 2 or 3)
            {
                RgbColor expected = SuitlessWarmShade(suitless[0][1], suitless[0][4], color);
                if (value != expected) warmShadeInputs.Add(key, new(value, expected));
                continue;
            }
            if (palette == 0 && color is >= 12 and <= 15)
            {
                RgbColor expected = Neutral(SuitlessGrayIntensity(suitless[0][11].Red, color));
                if (value != expected) neutralInputs.Add(key, new(value, expected));
                continue;
            }
            if (palette == 0 && color == 5)
            {
                if (value != WhiteInkColor) neutralInputs.Add(key, new(value, WhiteInkColor));
                continue;
            }
            if (palette == 0 && color == 11 || palette == 9 && color == 0)
            {
                neutralInputs.Add(key, new(value, Neutral(value.Red), independentMask: 1));
                continue;
            }
            if (palette is >= 2 and <= 8)
            {
                RgbColor expected = SamusPaletteFade.EighthTowardWhite(suitless[0][color], palette - 1);
                if (value != expected) suitlessFadeInputs.Add(key, new(value, expected));
            }
            else this.suitless.Add(keyCell, value);
        }
        this.whiteout = new(whiteout);
        for (int frame = 0; frame < explosionPaletteIndices.Length; frame++)
            if (explosionPaletteIndices[frame] != DefaultExplosionPaletteIndex(frame))
                this.explosionPaletteIndices.Add(frame, explosionPaletteIndices[frame]);
    }

    /// <summary>Resolves the suited palette view, calculating its seven whitening steps.</summary>
    /// <remarks>The suited steps are, in order: the base, the yellow flash, eighth-step shades 1 to 7, and the final shared
    /// step. The flash repeats one yellow colour across all sixteen inks and suits; the final step repeats one gray colour
    /// shared with the suitless colours. Yellow is the named primary mixture R=G=31, B=0, not an indexed intensity
    /// sequence. Stock needs no flash input; custom values override that colour and sharing. The final gray still has its
    /// own intensity input. The three suits' base steps share the common Power Suit inks; Varia differs at inks 2, 10 and
    /// 11, Gravity at 0, 1, 2, 10, 11 and 12. Independently supplied differences remain inputs, including source-only
    /// edits. Differing fade channels override shared arithmetic. The former array bounds exception is preserved.</remarks>
    /// <param name="suit">Family index 0=Power, 1=Varia, 2=Gravity; not the byte offsets 0, 2, and 4.</param>
    /// <param name="palette">Death-flash step 0..9: base, flash, seven whitening steps, then the final foreground step.</param>
    /// <param name="color">Ink index 0..15, copied by the consumer to colours 192..207; index-zero pixels remain transparent.</param>
    /// <returns>The selected RGB5 word with red in bits 0..4, green in 5..9, and blue in 10..14; no palette is written.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Any index is outside its documented range.</exception>
    public RgbColor SuitedColor(int suit, int palette, int color)
    {
        if ((uint)suit >= SuitCount)
            throw new ArgumentOutOfRangeException(nameof(suit));
        if ((uint)palette >= SamusPaletteDefinitions.Death.PaletteCount)
            throw new ArgumentOutOfRangeException(nameof(palette));
        if ((uint)color >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        int key = (suit * SamusPaletteDefinitions.Death.PaletteCount + palette) * ColorCount + color;
        var keyCell = new TableColourPlace((suit * SamusPaletteDefinitions.Death.PaletteCount + palette), color);
        if (suited.TryGetValue(keyCell, out RgbColor value)) return value;
        if (palette == 0) return SuitedColor(0, 0, color);
        if (palette == 1) return suit == 0 && color == 0 ? YellowFlashColor : SuitedColor(0, 1, 0);
        if (palette == 9) return SuitlessColor(9, 0);
        RgbColor expected = SamusPaletteFade.EighthTowardWhite(SuitedColor(suit, 0, color), palette - 1);
        return suitedFadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }
    /// <summary>Resolves the same eighth-step whitening for suitless Samus.</summary>
    /// <remarks>The suitless pointer table repeats the base at indices 0/1,
    /// selects shade 1..7 at 2..8 and a distinct final step at 9. Independent
    /// edits, including the repeated base step, retain their supplied values.
    /// Base ink 5 is full RGB5 white; resolve that named color directly and
    /// retain only independently edited channels. White stays white throughout
    /// the seven computed fade steps. No generated color cache is stored.</remarks>
    /// <param name="palette">Death-flash step 0..9; stock steps 0 and 1 repeat the base, 2..8 whiten it, and 9 is the final foreground step.</param>
    /// <param name="color">Ink index 0..15, copied by the consumer to colours 240..255; index-zero pixels remain transparent.</param>
    /// <returns>The selected RGB5 word without changing the colours or advancing the death sequence.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its documented range.</exception>
    public RgbColor SuitlessColor(int palette, int color)
    {
        if ((uint)palette >= SamusPaletteDefinitions.Death.PaletteCount)
            throw new ArgumentOutOfRangeException(nameof(palette));
        if ((uint)color >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        int key = palette * ColorCount + color;
        var keyCell = new TableColourPlace(palette, color);
        if (suitless.TryGetValue(keyCell, out RgbColor value)) return value;
        if (palette == 0 && color == 5)
            return neutralInputs.TryGetValue(key, out var whiteInput) ? whiteInput.Apply(WhiteInkColor) : WhiteInkColor;
        if (palette == 0 && color == 4)
        {
            var endpoint = warmShadeInputs[key];
            return endpoint.Apply(SuitlessWarmEndpoint(SuitlessColor(0, 1), endpoint.Apply(RgbColor.Black)));
        }
        if (palette == 0 && color == 10)
        {
            var endpoint = tintShadeInputs[key];
            if (!TrySuitlessTintEndpoint(SuitlessColor(0, 6), endpoint.Apply(RgbColor.Black), out RgbColor expectedEndpoint))
                throw new InvalidOperationException("Validated suitless tint endpoint exceeds RGB5.");
            return endpoint.Apply(expectedEndpoint);
        }
        if (palette == 0 && color == 8)
        {
            RgbColor middle = SuitlessTintMidpoint(SuitlessColor(0, 7), SuitlessColor(0, 9));
            return tintShadeInputs.TryGetValue(key, out var middleInput) ? middleInput.Apply(middle) : middle;
        }
        if (tintShadeInputs.TryGetValue(key, out var tint))
        {
            if (!TrySuitlessTintShade(SuitlessColor(0, 6), SuitlessColor(0, 10), tint.Apply(RgbColor.Black).Blue, color, out RgbColor expectedTint))
                throw new InvalidOperationException("Validated suitless tint exceeds RGB5.");
            return tint.Apply(expectedTint);
        }
        if (palette == 0 && color is 2 or 3)
        {
            RgbColor warm = SuitlessWarmShade(SuitlessColor(0, 1), SuitlessColor(0, 4), color);
            return warmShadeInputs.TryGetValue(key, out var warmInput) ? warmInput.Apply(warm) : warm;
        }
        if (palette == 0 && color is >= 12 and <= 15)
        {
            RgbColor gray = Neutral(SuitlessGrayIntensity(SuitlessColor(0, 11).Red, color));
            return neutralInputs.TryGetValue(key, out var grayInput) ? grayInput.Apply(gray) : gray;
        }
        if (neutralInputs.TryGetValue(key, out var neutral))
            return neutral.Apply(Neutral(neutral.Apply(RgbColor.Black).Red));
        if (palette == 9) return SuitlessColor(9, 0);
        if (palette == 1) return SuitlessColor(0, color);
        RgbColor expected = SamusPaletteFade.EighthTowardWhite(SuitlessColor(0, color), palette - 1);
        return suitlessFadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }
    /// <summary>Preserves the tint ramp's green-minus-blue balance at its dark endpoint.</summary>
    /// <remarks>All five suitless inks 6..10 have G-B=6.
    /// The bright endpoint supplies that balance; the dark endpoint retains
    /// independent red/blue and calculates green=darkBlue+brightGreen-brightBlue.
    /// No fixed correction is embedded. Edited inputs may yield green outside
    /// RGB5; return false so import preserves the supplied color without clamping.
    /// Inputs are RGB5 words; signed intermediate green spans-31..62.</remarks>
    internal static bool TrySuitlessTintEndpoint(RgbColor bright, RgbColor dark, out RgbColor value)
    {
        int green = (dark.Blue) + (bright.Green) - (bright.Blue);
        if ((uint)green > 31) { value = RgbColor.Black; return false; }
        value = dark.WithGreen(green);
        return true;
    }
    /// <summary>Calculates the central suitless tint between its neighboring shades.</summary>
    /// <remarks>The stock suitless inks 7/8/9 form an evenly spaced RGB run:
    /// red 25/20/15, green 16/13/10, blue 10/7/4. Ink 8 is the exact channel
    /// midpoint of 7/9. For edited endpoints, round each nonnegative half sum
    /// down; sums are at most 62 with no saturation, wrap or cross-channel carry.
    /// Independently supplied middle channels override the result. This removes
    /// the middle intensity input, not the remaining endpoint choices.</remarks>
    internal static RgbColor SuitlessTintMidpoint(RgbColor first, RgbColor last)
    {
        int red = ((first.Red) + (last.Red)) / 2;
        int green = ((first.Green) + (last.Green)) / 2;
        int blue = ((first.Blue) + (last.Blue)) / 2;
        return new RgbColor(red, green, blue);
    }
    /// <summary>Interpolates tint balance while preserving a supplied middle-shade intensity.</summary>
    /// <remarks>The stock suitless inks 6..10 have middle
    /// RGB offsets matching a quarter-step endpoint gradient. Interpolate each
    /// endpoint channel downward, then shift RGB equally so blue equals the
    /// independently supplied middle blue. Thus green-blue remains 6 and
    /// red-blue descends 17, 15, 13, 11, 9. Blue choices at 7/9 are the independently
    /// selected shade anchors covered by the class-level artwork disposition;
    /// ink 8 is independently resolved by SuitlessTintMidpoint. This does not
    /// substitute fixed correction values.
    /// Edited endpoints/intensities may place calculated red/green outside RGB5.
    /// Return false so import preserves that supplied whole color, without
    /// clamping or wrapping. Inputs outside RGB5 or inks 7..9 are rejected.</remarks>
    internal static bool TrySuitlessTintShade(RgbColor first, RgbColor last, int blue, int color, out RgbColor value)
    {
        if ((uint)blue > 31) throw new ArgumentOutOfRangeException(nameof(blue));
        if (color is < 7 or > 9) throw new ArgumentOutOfRangeException(nameof(color));
        int weight = color - 6;
        int interpolatedBlue = ((first.Blue) * (4 - weight) + (last.Blue) * weight) / 4;
        int shift = blue - interpolatedBlue;
        int red = ((first.Red) * (4 - weight) + (last.Red) * weight) / 4 + shift;
        int green = ((first.Green) * (4 - weight) + (last.Green) * weight) / 4 + shift;
        if ((uint)red > 31 || (uint)green > 31) { value = RgbColor.Black; return false; }
        value = new RgbColor(red, green, blue);
        return true;
    }
    /// <summary>Shares the warm ramp's blue level between its endpoints.</summary>
    /// <remarks>The stock suitless inks 1..4 form a red/green shade ramp
    /// with blue zero throughout. The bright endpoint supplies that common
    /// blue; the dark endpoint keeps its independent red/green. Edited blue
    /// differences override sharing. RGB5 words only, no rounding or saturation.</remarks>
    internal static RgbColor SuitlessWarmEndpoint(RgbColor bright, RgbColor dark)
    {
        return dark.WithBlue(bright.Blue);
    }
    /// <summary>Interpolates the two middle warm inks between their supplied endpoints.</summary>
    /// <remarks>The stock warm ramp contains four ordered warm shades.
    /// Ink 1 and 4 are endpoints; ink 2/3 select one-third/two-thirds toward 4,
    /// flooring each RGB5 channel independently. Thus red 31..9 gives 23/16,
    /// green 23..4 gives 16/10, and blue stays zero. Independent edits may set
    /// any RGB5 endpoints; convex interpolation stays bounded without clamping.
    /// Numerator is at most 93. Invalid colors and high-bit words are rejected.</remarks>
    internal static RgbColor SuitlessWarmShade(RgbColor first, RgbColor last, int color)
    {
        if (color is not (2 or 3)) throw new ArgumentOutOfRangeException(nameof(color));
        int weight = color - 1;
        int red = ((first.Red) * (3 - weight) + (last.Red) * weight) / 3;
        int green = ((first.Green) * (3 - weight) + (last.Green) * weight) / 3;
        int blue = ((first.Blue) * (3 - weight) + (last.Blue) * weight) / 3;
        return new RgbColor(red, green, blue);
    }
    /// <summary>Returns a suitless gray ink's evenly spaced intensity toward black.</summary>
    /// <remarks>The stock gray ramp contains five descending neutral shades.
    /// Ink 11 supplies the peak; inks 11..15 take 5/5, 4/5, 3/5, 2/5, 1/5 of it,
    /// rounding upward in RGB5. This gives the stock 19, 16, 12, 8, 4 intensities.
    /// The zero-intensity endpoint is black; no extrapolated sixth ink exists.
    /// Numerator including rounding is at most 159, no saturation or wrap.
    /// Independently edited ink channels override the calculated value.</remarks>
    internal static int SuitlessGrayIntensity(int peak, int color)
    {
        if ((uint)peak > 31) throw new ArgumentOutOfRangeException(nameof(peak));
        if (color is < 11 or > 15) throw new ArgumentOutOfRangeException(nameof(color));
        return (peak * (16 - color) + 4) / 5;
    }
    /// <summary>Shares the intensity across neutral RGB5 channels.</summary>
    /// <remarks>The stock suitless base inks 5/11..15 and the final step
    /// all have red=green=blue. Gray-peak/final intensities remain inputs;
    /// full white resolves directly through WhiteInkColor.
    /// green/blue edits override the shared value. The supplied red channel
    /// is bounded 0..31; bit replication needs no rounding or saturation.
    /// This converts channel duplication, not the separate intensity choices.</remarks>
    private static RgbColor Neutral(int intensity) => new(intensity, intensity, intensity);
    /// <summary>Resolves a ShadesOfWhite entry, retaining independent edits to the calculated two-rate whiteout.</summary>
    /// <param name="index">Whiteout shade 0..21, not an explosion frame; the terminal call selects 21 explicitly.</param>
    /// <returns>The RGB5 word used by the consumer to fill colours 0..191 and 208..239, excluding suited and suitless Samus palettes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside 0..21.</exception>
    public RgbColor WhiteoutColor(int index) => whiteout.Resolve(index);

    /// <summary>Independent edits to the calculated two-rate whiteout.</summary>
    /// <remarks>The stock whiteout ramp needs no stored shade or anchor inputs.
    /// Keep only channels differing from DefaultWhiteoutColor; edits to any
    /// supplied shade remain independent of every other supplied shade.</remarks>
    internal sealed class WhiteoutInputs
    {
        /// <summary>Only whiteout channels that differ from the calculated two-rate ramp.</summary>
        internal readonly Dictionary<int, LoadingPaletteInputView.Channels> inputs = new();

        /// <summary>Captures independent shade edits while leaving stock ramp values implicit.</summary>
        internal WhiteoutInputs(RgbColor[] source)
        {
            for (int index = 0; index < source.Length; index++)
            {
                RgbColor expected = DefaultWhiteoutColor(index);
                if (source[index] != expected) inputs.Add(index, new(source[index], expected));
            }
        }

        /// <summary>Applies an edited shade's channels over the calculated ramp at that index.</summary>
        internal RgbColor Resolve(int index)
        {
            RgbColor expected = DefaultWhiteoutColor(index);
            return inputs.TryGetValue(index, out var channels) ? channels.Apply(expected) : expected;
        }
    }

    /// <summary>Raises the background from the first nonblack shade to full white in two stages.</summary>
    /// <remarks>The stock whiteout ramp contains 22 neutral RGB5 shades.
    /// Intensity rises 1 to 16 over indices 0..7, then 16 to 31 over 7..21:
    /// the first half of the brightness rise takes seven intervals, the second
    /// takes fourteen. First stage floors 15*index/7 before adding 1; second
    /// floors the remaining distance 15*(21-index)/14 before subtracting from 31.
    /// This preserves the quantization at the shared half-brightness
    /// boundary without special sample corrections. Numerators are at most 210;
    /// there is no wrap, saturation, extrapolation or generated lookup cache.
    /// The death sequence skips explosion frame 0, then reads counter 0..20 before
    /// incrementing and holds 20. Its terminal call selects 21 explicitly. Preserve
    /// all 22 supported selectors and reject others with the former bounds exception.
    /// This establishes an exact two-rate ramp, not the identity of its historical tool.</remarks>
    internal static RgbColor DefaultWhiteoutColor(int index)
    {
        if ((uint)index >= SamusPaletteDefinitions.Death.WhiteoutShadeCount) throw new ArgumentOutOfRangeException(nameof(index));
        const int firstLitIntensity = 1, fullWhiteIntensity = 31;
        const int halfBrightnessIndex = 7, completionIndex = 21;
        const int halfRise = (fullWhiteIntensity - firstLitIntensity) / 2;
        int intensity = index <= halfBrightnessIndex
            ? firstLitIntensity + halfRise * index / halfBrightnessIndex
            : fullWhiteIntensity - halfRise * (completionIndex - index) / (completionIndex - halfBrightnessIndex);
        return Neutral(intensity);
    }
    /// <summary>Advances through death palettes while skipping the flash-only step.</summary>
    /// <remarks>The original odd selector bytes select 0, 2..9 for frame 0..8.
    /// Index 1 belongs to the preceding yellow-flash stage, not the explosion.
    /// The caller advances the explosion frame before selecting its
    /// next palette and terminates at 9 without another lookup. Edited asset
    /// selectors remain independent. Preserve the original array bounds exception.</remarks>
    /// <param name="frame">Explosion drawing index 0..8, selecting one of the nine timing/selector records.</param>
    /// <returns>A death-flash step 0..9 for both foreground palette families, not a RGB5 word or countdown duration.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside 0..8.</exception>
    public int ExplosionPaletteIndex(int frame)
    {
        int expected = DefaultExplosionPaletteIndex(frame);
        return Ensure.BetweenInclusive(0, ushort.MaxValue, explosionPaletteIndices.ContainsKey(frame) ? explosionPaletteIndices[frame] : expected);
    }

    /// <summary>
    /// The order the death sequence steps through and the room's whiteout colours, which gameplay reads (#1512); the
    /// suit and suitless colours stay here, where the import pre-colours her death shades from them.
    /// </summary>
    public SamusDeathSteps Steps => new(
        [.. Enumerable.Range(0, SamusDeathExplosionTimingDefinitions.RecordCount).Select(ExplosionPaletteIndex)],
        [.. Enumerable.Range(0, SamusPaletteDefinitions.Death.WhiteoutShadeCount).Select(WhiteoutColor)]);

    /// <summary>Maps frame zero to base step zero and later frames past the flash-only step.</summary>
    private static int DefaultExplosionPaletteIndex(int frame)
    {
        if ((uint)frame >= SamusDeathExplosionTimingDefinitions.RecordCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return WordWrap.Unsigned(frame == 0 ? 0 : frame + 1);
    }
}
