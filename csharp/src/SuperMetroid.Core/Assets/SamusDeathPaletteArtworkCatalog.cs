using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable suit, suitless, and whiteout colors for Samus's death sequence.</summary>
/// <remarks>The suited base rows9B9820/9920/9A20 are the same logical
/// artwork as the independently reviewed full-body bases9B9B20/9D20/9F20.
/// Their25 shared inputs comprise21 painted inks,Gravity1/12 using existing
/// Power dim tint RG targets,and two unused transparent payloads. These are
/// categorical pixel colors,not samples indexed by a quantitative lighting
/// input. Fitting or reciting ink-to-RGB choices would encode the artwork;
/// index-zero RGB is ignored by OBJ rendering. This is the specific1165
/// nonsense disposition of those aliased base inputs. Shared suits and fade
/// shades are calculated; independent asset edits stay local.
///
/// Suitless base ink0 at9BA120 is likewise an unused transparent payload:
/// native9BB53A copies it to OBJ palette7 slot0,while OBJ rendering discards
/// index-zero pixels before reading CGRAM. Its RGB is not a visible shade
/// or an input to another opaque ink. Temporal fades preserve the selected
/// word but do not make it visible. A numeric case or fitted expression for
/// that arbitrary unused payload would only recite the data; retain this one
/// word under the same specific nonsense exception.
///
/// The remaining13 visible suitless base components are artwork inputs:
/// ink1 RGB,ink4 RG,ink6 RGB,ink7/9 blue,ink10 red/blue and ink11 intensity.
/// They choose the warm/tinted endpoint colors and local shade contrast of
/// categorical sprite inks. Native9BB53A and9BB5CE copy selected palette
/// rows; pixel indices select painted classes,not a measured illumination,
/// material or hue coordinate that determines those choices. Linear warm,
/// tint-balance,midpoint,gray and temporal shade relationships are calculated
/// separately. Fitting or reciting the remaining color anchors would merely
/// encode the painting in another form,the specific1165 nonsense exception.
///
/// Final9BA220 chooses one uniform gray intensity29 for all inks and suits.
/// It replaces their different preceding shades; it is not another eighth
/// step,which would produce full white31. This chosen foreground color is
/// likewise retained as artwork,with neutral channels and all aliases derived.
/// The separately indexed whiteout curve is calculated by DefaultWhiteoutColor.
/// Explosion durations have their own independently documented disposition.</remarks>
public sealed class SamusDeathPaletteArtworkCatalog
{
    /// <summary>Three suited families in $9B:B7D3's pointer-table order: Power, Varia, then Gravity.</summary>
    public const int SuitCount = 3;
    /// <summary>Sixteen RGB5 words per OBJ palette row, including the transparent sprite ink at index zero.</summary>
    public const int ColorCount = SamusPaletteRomData.Common.ColorsPerObjPalette;

    /// <summary>Native9B9420 yellow flash: full red and green,zero blue in RGB5.</summary>
    private static readonly Bgr555 YellowFlashColor = new(31, 31, 0);
    /// <summary>Native9BA12A suitless ink5: full red,green and blue in RGB5.</summary>
    private static readonly Bgr555 WhiteInkColor = new(31, 31, 31);

    private readonly Dictionary<int, Bgr555> suited = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> suitedFadeInputs = new();
    private readonly Dictionary<int, Bgr555> suitless = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> suitlessFadeInputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> neutralInputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> warmShadeInputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> tintShadeInputs = new();
    private readonly WhiteoutInputs whiteout;
    private readonly Dictionary<int, ushort> explosionPaletteIndices = new();

    /// <summary>Captures selected death artwork as owned inputs and independent channel overrides; no caller arrays are retained or modified.</summary>
    /// <param name="suited">Three suit families, each with ten rows of sixteen RGB5 words, in $9B:B7D3 pointer-table order.</param>
    /// <param name="suitless">Ten rows of sixteen RGB5 words in $9B:B80F pointer-table order, separate from suited artwork.</param>
    /// <param name="whiteout">Twenty-two RGB5 shades corresponding to ShadesOfWhite at $9B:B835, indexed by whiteout counter rather than explosion frame.</param>
    /// <param name="explosionPaletteIndices">Nine row selectors 0..9 corresponding to the odd bytes at $9B:B824..B834; animation durations are not part of this input.</param>
    /// <exception cref="ArgumentNullException">A top-level input array is null.</exception>
    /// <exception cref="InvalidDataException">An input has the wrong dimensions, a nested family or row is null, a color exceeds $7FFF, or an explosion selector exceeds 9.</exception>
    public SamusDeathPaletteArtworkCatalog(Bgr555[][][] suited, Bgr555[][] suitless,
        Bgr555[] whiteout, ushort[] explosionPaletteIndices)
    {
        ArgumentNullException.ThrowIfNull(suited);
        ArgumentNullException.ThrowIfNull(suitless);
        ArgumentNullException.ThrowIfNull(whiteout);
        ArgumentNullException.ThrowIfNull(explosionPaletteIndices);
        FixedGridShape paletteRows = new(SamusPaletteRomData.Death.PaletteCount, ColorCount);
        if (suited.Length != SuitCount ||
            suited.Any(family => !paletteRows.Fits(family)) ||
            !paletteRows.Fits(suitless) ||
            whiteout.Length != SamusPaletteRomData.Death.WhiteoutShadeCount ||
            explosionPaletteIndices.Length != SamusDeathExplosionTimingDefinitions.RecordCount ||
            explosionPaletteIndices.Any(index => index >= SamusPaletteRomData.Death.PaletteCount))
            throw new InvalidDataException("Samus death-palette artwork has an invalid shape or RGB5 color.");
        for (int suit = 0; suit < SuitCount; suit++)
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        for (int color = 0; color < ColorCount; color++)
        {
            int key = (suit * SamusPaletteRomData.Death.PaletteCount + palette) * ColorCount + color;
            Bgr555 value = suited[suit][palette][color];
            if (palette == 0 && suit != 0 && value == suited[0][0][color]) continue;
            if (palette == 1 && (suit != 0 || color != 0) && value == suited[0][1][0]) continue;
            if (suit == 0 && palette == 1 && color == 0 && value == YellowFlashColor) continue;
            if (palette == 9 && value == suitless[9][0]) continue;
            if (palette is >= 2 and <= 8)
            {
                Bgr555 expected = SamusPaletteFade.EighthTowardWhite(suited[suit][0][color], palette - 1);
                if (value != expected) suitedFadeInputs.Add(key, new(value, expected));
            }
            else this.suited.Add(key, value);
        }
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        for (int color = 0; color < ColorCount; color++)
        {
            int key = palette * ColorCount + color;
            Bgr555 value = suitless[palette][color];
            if (palette == 9 && color != 0 && value == suitless[9][0]) continue;
            if (palette == 1 && value == suitless[0][color]) continue;
            if (palette == 0 && color == 4)
            {
                Bgr555 expected = SuitlessWarmEndpoint(suitless[0][1], value);
                warmShadeInputs.Add(key, new(value, expected, independentMask: 3));
                continue;
            }
            if (palette == 0 && color == 10)
            {
                if (TrySuitlessTintEndpoint(suitless[0][6], value, out Bgr555 expected))
                    tintShadeInputs.Add(key, new(value, expected, independentMask: 5));
                else this.suitless.Add(key, value);
                continue;
            }
            if (palette == 0 && color == 8)
            {
                Bgr555 expected = SuitlessTintMidpoint(suitless[0][7], suitless[0][9]);
                if (value != expected) tintShadeInputs.Add(key, new(value, expected));
                continue;
            }
            if (palette == 0 && color is 7 or 9)
            {
                if (TrySuitlessTintShade(suitless[0][6], suitless[0][10], value.Blue, color, out Bgr555 expected))
                    tintShadeInputs.Add(key, new(value, expected, independentMask: 4));
                else this.suitless.Add(key, value);
                continue;
            }
            if (palette == 0 && color is 2 or 3)
            {
                Bgr555 expected = SuitlessWarmShade(suitless[0][1], suitless[0][4], color);
                if (value != expected) warmShadeInputs.Add(key, new(value, expected));
                continue;
            }
            if (palette == 0 && color is >= 12 and <= 15)
            {
                Bgr555 expected = Neutral(SuitlessGrayIntensity(suitless[0][11].Red, color));
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
                Bgr555 expected = SamusPaletteFade.EighthTowardWhite(suitless[0][color], palette - 1);
                if (value != expected) suitlessFadeInputs.Add(key, new(value, expected));
            }
            else this.suitless.Add(key, value);
        }
        this.whiteout = new(whiteout);
        for (int frame = 0; frame < explosionPaletteIndices.Length; frame++)
            if (explosionPaletteIndices[frame] != DefaultExplosionPaletteIndex(frame))
                this.explosionPaletteIndices.Add(frame, explosionPaletteIndices[frame]);
    }

    /// <summary>SHA-256 of selected suited/suitless colors, whiteout shades and explosion selectors.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusDeathPaletteArtworkCatalog), content =>
    {
        Span<Bgr555> suitedRow = stackalloc Bgr555[ColorCount];
        for (int suit = 0; suit < SuitCount; suit++)
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        {
            for (int color = 0; color < ColorCount; color++) suitedRow[color] = SuitedColor(suit, palette, color);
            content.AppendColors("suited row", suitedRow);
        }
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        {
            for (int color = 0; color < ColorCount; color++) suitedRow[color] = SuitlessColor(palette, color);
            content.AppendColors("suitless row", suitedRow);
        }
        Span<Bgr555> whiteoutColors = stackalloc Bgr555[SamusPaletteRomData.Death.WhiteoutShadeCount];
        for (int index = 0; index < whiteoutColors.Length; index++) whiteoutColors[index] = WhiteoutColor(index);
        content.AppendColors("whiteout", whiteoutColors);
        Span<ushort> selectors = stackalloc ushort[SamusDeathExplosionTimingDefinitions.RecordCount];
        for (int frame = 0; frame < selectors.Length; frame++) selectors[frame] = ExplosionPaletteIndex(frame);
        content.AppendWords("explosion palette indices", selectors);
    });

    /// <summary>Resolves the native suited palette view, calculating its seven whitening steps.</summary>
    /// <remarks>Native9B:B7D3..B80D selects base at0, yellow flash at1,
    /// eighth-step shades1..7 at indices2..8 and the final shared row at9.
    /// Original flash9B9420 repeats one yellow color across all sixteen inks
    /// and suits; final9BA220 repeats one gray color shared with suitless.
    /// Yellow is the named RGB5 primary mixture R=G=31,B=0,not an indexed
    /// intensity sequence. Stock needs no flash input; custom values override
    /// that color and sharing. The final gray still has its own intensity input.
    /// Base rows9820/9920/9A20 share the common Power Suit inks;
    /// Varia differs at2/10/11,Gravity at0/1/2/10/11/12. Independently supplied
    /// differences remain inputs,including source-only edits. Differing fade channels
    /// override shared arithmetic. Preserve the former array bounds exception.</remarks>
    /// <param name="suit">Family index 0=Power, 1=Varia, 2=Gravity; not the native byte offsets 0, 2, and 4.</param>
    /// <param name="palette">Pointer-table row 0..9: base, flash, seven whitening steps, then final foreground row.</param>
    /// <param name="color">Ink index 0..15, copied by the consumer to CGRAM 192..207; index-zero pixels remain transparent.</param>
    /// <returns>The selected RGB5 word with red in bits 0..4, green in 5..9, and blue in 10..14; no palette is written.</returns>
    /// <exception cref="IndexOutOfRangeException">Any index is outside its documented range.</exception>
    public Bgr555 SuitedColor(int suit, int palette, int color)
    {
        if ((uint)suit >= SuitCount || (uint)palette >= SamusPaletteRomData.Death.PaletteCount || (uint)color >= ColorCount)
            throw new IndexOutOfRangeException();
        int key = (suit * SamusPaletteRomData.Death.PaletteCount + palette) * ColorCount + color;
        if (suited.TryGetValue(key, out Bgr555 value)) return value;
        if (palette == 0) return SuitedColor(0, 0, color);
        if (palette == 1) return suit == 0 && color == 0 ? YellowFlashColor : SuitedColor(0, 1, 0);
        if (palette == 9) return SuitlessColor(9, 0);
        Bgr555 expected = SamusPaletteFade.EighthTowardWhite(SuitedColor(suit, 0, color), palette - 1);
        return suitedFadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }
    /// <summary>Resolves the same eighth-step whitening for suitless Samus.</summary>
    /// <remarks>Original9B:B80F..B822 repeats the base at indices0/1,
    /// selects shade1..7 at2..8 and a distinct final row at9. Independent
    /// edits, including the repeated base row, retain their supplied values.
    /// Base ink5 is full RGB5 white; resolve that named color directly and
    /// retain only independently edited channels. White stays white throughout
    /// the seven computed fade steps. No generated color cache is stored.</remarks>
    /// <param name="palette">Pointer-table row 0..9; stock rows 0 and 1 repeat the base, 2..8 whiten it, and 9 is the final foreground row.</param>
    /// <param name="color">Ink index 0..15, copied by the consumer to CGRAM 240..255; index-zero pixels remain transparent.</param>
    /// <returns>The selected RGB5 word without changing CGRAM or advancing the death sequence.</returns>
    /// <exception cref="IndexOutOfRangeException">Either index is outside its documented range.</exception>
    public Bgr555 SuitlessColor(int palette, int color)
    {
        if ((uint)palette >= SamusPaletteRomData.Death.PaletteCount || (uint)color >= ColorCount)
            throw new IndexOutOfRangeException();
        int key = palette * ColorCount + color;
        if (suitless.TryGetValue(key, out Bgr555 value)) return value;
        if (palette == 0 && color == 5)
            return neutralInputs.TryGetValue(key, out var whiteInput) ? whiteInput.Apply(WhiteInkColor) : WhiteInkColor;
        if (palette == 0 && color == 4)
        {
            var endpoint = warmShadeInputs[key];
            return endpoint.Apply(SuitlessWarmEndpoint(SuitlessColor(0, 1), endpoint.Apply(Bgr555.Black)));
        }
        if (palette == 0 && color == 10)
        {
            var endpoint = tintShadeInputs[key];
            if (!TrySuitlessTintEndpoint(SuitlessColor(0, 6), endpoint.Apply(Bgr555.Black), out Bgr555 expectedEndpoint))
                throw new InvalidOperationException("Validated suitless tint endpoint exceeds RGB5.");
            return endpoint.Apply(expectedEndpoint);
        }
        if (palette == 0 && color == 8)
        {
            Bgr555 middle = SuitlessTintMidpoint(SuitlessColor(0, 7), SuitlessColor(0, 9));
            return tintShadeInputs.TryGetValue(key, out var middleInput) ? middleInput.Apply(middle) : middle;
        }
        if (tintShadeInputs.TryGetValue(key, out var tint))
        {
            if (!TrySuitlessTintShade(SuitlessColor(0, 6), SuitlessColor(0, 10), tint.Apply(Bgr555.Black).Blue, color, out Bgr555 expectedTint))
                throw new InvalidOperationException("Validated suitless tint exceeds RGB5.");
            return tint.Apply(expectedTint);
        }
        if (palette == 0 && color is 2 or 3)
        {
            Bgr555 warm = SuitlessWarmShade(SuitlessColor(0, 1), SuitlessColor(0, 4), color);
            return warmShadeInputs.TryGetValue(key, out var warmInput) ? warmInput.Apply(warm) : warm;
        }
        if (palette == 0 && color is >= 12 and <= 15)
        {
            Bgr555 gray = Neutral(SuitlessGrayIntensity(SuitlessColor(0, 11).Red, color));
            return neutralInputs.TryGetValue(key, out var grayInput) ? grayInput.Apply(gray) : gray;
        }
        if (neutralInputs.TryGetValue(key, out var neutral))
            return neutral.Apply(Neutral(neutral.Apply(Bgr555.Black).Red));
        if (palette == 9) return SuitlessColor(9, 0);
        if (palette == 1) return SuitlessColor(0, color);
        Bgr555 expected = SamusPaletteFade.EighthTowardWhite(SuitlessColor(0, color), palette - 1);
        return suitlessFadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }
    /// <summary>Preserves the tint ramp's green-minus-blue balance at its dark endpoint.</summary>
    /// <remarks>All five native inks6..10 at9BA12C..A135 have G-B=6.
    /// The bright endpoint supplies that balance; the dark endpoint retains
    /// independent red/blue and calculates green=darkBlue+brightGreen-brightBlue.
    /// No fixed correction is embedded. Edited inputs may yield green outside
    /// RGB5; return false so import preserves the supplied color without clamping.
    /// Inputs are RGB5 words; signed intermediate green spans-31..62.</remarks>
    internal static bool TrySuitlessTintEndpoint(Bgr555 bright, Bgr555 dark, out Bgr555 value)
    {
        int green = (dark.Blue) + (bright.Green) - (bright.Blue);
        if ((uint)green > 31) { value = Bgr555.Black; return false; }
        value = dark.WithGreen(green);
        return true;
    }
    /// <summary>Calculates the central suitless tint between its neighboring shades.</summary>
    /// <remarks>Native9BA12E..A133 inks7/8/9 form an evenly spaced RGB run:
    /// red25/20/15,green16/13/10,blue10/7/4. Ink8 is the exact channel
    /// midpoint of7/9. For edited endpoints,round each nonnegative half sum
    /// down; sums are at most62 with no saturation,wrap or cross-channel carry.
    /// Independently supplied middle channels override the result. This removes
    /// the middle intensity input,not the remaining endpoint choices.</remarks>
    internal static Bgr555 SuitlessTintMidpoint(Bgr555 first, Bgr555 last)
    {
        int red = ((first.Red) + (last.Red)) / 2;
        int green = ((first.Green) + (last.Green)) / 2;
        int blue = ((first.Blue) + (last.Blue)) / 2;
        return new Bgr555(red, green, blue);
    }
    /// <summary>Interpolates tint balance while preserving a supplied middle-shade intensity.</summary>
    /// <remarks>Original suitless inks6..10 at9BA12C..A135 have middle
    /// RGB offsets matching a quarter-step endpoint gradient. Interpolate each
    /// endpoint channel downward,then shift RGB equally so blue equals the
    /// independently supplied middle blue. Thus green-blue remains6 and
    /// red-blue descends17,15,13,11,9. Blue choices at7/9 are the independently
    /// selected shade anchors covered by the class-level artwork disposition;
    /// ink8 is independently resolved by SuitlessTintMidpoint. This does not
    /// substitute fixed correction values.
    /// Edited endpoints/intensities may place calculated red/green outside RGB5.
    /// Return false so import preserves that supplied whole color,without
    /// clamping or wrapping. Inputs outside RGB5 or inks7..9 are rejected.</remarks>
    internal static bool TrySuitlessTintShade(Bgr555 first, Bgr555 last, int blue, int color, out Bgr555 value)
    {
        if ((uint)blue > 31) throw new ArgumentOutOfRangeException(nameof(blue));
        if (color is < 7 or > 9) throw new ArgumentOutOfRangeException(nameof(color));
        int weight = color - 6;
        int interpolatedBlue = ((first.Blue) * (4 - weight) + (last.Blue) * weight) / 4;
        int shift = blue - interpolatedBlue;
        int red = ((first.Red) * (4 - weight) + (last.Red) * weight) / 4 + shift;
        int green = ((first.Green) * (4 - weight) + (last.Green) * weight) / 4 + shift;
        if ((uint)red > 31 || (uint)green > 31) { value = Bgr555.Black; return false; }
        value = new Bgr555(red, green, blue);
        return true;
    }
    /// <summary>Shares the warm ramp's blue level between its endpoints.</summary>
    /// <remarks>Native9BA122..A129 inks1..4 form a red/green shade ramp
    /// with blue zero throughout. The bright endpoint supplies that common
    /// blue; the dark endpoint keeps its independent red/green. Edited blue
    /// differences override sharing. RGB5 words only,no rounding or saturation.</remarks>
    internal static Bgr555 SuitlessWarmEndpoint(Bgr555 bright, Bgr555 dark)
    {
        return dark.WithBlue(bright.Blue);
    }
    /// <summary>Interpolates the two middle warm inks between their supplied endpoints.</summary>
    /// <remarks>Original9BA122..A129 contains four ordered warm shades.
    /// Ink1 and4 are endpoints; ink2/3 select one-third/two-thirds toward4,
    /// flooring each RGB5 channel independently. Thus red31..9 gives23/16,
    /// green23..4 gives16/10,and blue stays zero. Independent edits may set
    /// any RGB5 endpoints; convex interpolation stays bounded without clamping.
    /// Numerator is at most93. Invalid colors and high-bit words are rejected.</remarks>
    internal static Bgr555 SuitlessWarmShade(Bgr555 first, Bgr555 last, int color)
    {
        if (color is not (2 or 3)) throw new ArgumentOutOfRangeException(nameof(color));
        int weight = color - 1;
        int red = ((first.Red) * (3 - weight) + (last.Red) * weight) / 3;
        int green = ((first.Green) * (3 - weight) + (last.Green) * weight) / 3;
        int blue = ((first.Blue) * (3 - weight) + (last.Blue) * weight) / 3;
        return new Bgr555(red, green, blue);
    }
    /// <summary>Returns a suitless gray ink's evenly spaced intensity toward black.</summary>
    /// <remarks>Original9BA136..A13F contains five descending neutral shades.
    /// Ink11 supplies the peak; inks11..15 take5/5,4/5,3/5,2/5,1/5 of it,
    /// rounding upward in RGB5. This gives the native19,16,12,8,4 intensities.
    /// The zero-intensity endpoint is black; no extrapolated sixth ink exists.
    /// Numerator including rounding is at most159,no saturation or wrap.
    /// Independently edited ink channels override the calculated value.</remarks>
    internal static int SuitlessGrayIntensity(int peak, int color)
    {
        if ((uint)peak > 31) throw new ArgumentOutOfRangeException(nameof(peak));
        if (color is < 11 or > 15) throw new ArgumentOutOfRangeException(nameof(color));
        return (peak * (16 - color) + 4) / 5;
    }
    /// <summary>Shares the intensity across neutral RGB5 channels.</summary>
    /// <remarks>Original suitless base9BA120 inks5/11..15 and final9BA220
    /// all have red=green=blue. Gray-peak/final intensities remain inputs;
    /// full white resolves directly through WhiteInkColor.
    /// green/blue edits override the shared value. The supplied red channel
    /// is bounded0..31; bit replication needs no rounding or saturation.
    /// This converts channel duplication,not the separate intensity choices.</remarks>
    private static Bgr555 Neutral(int intensity) => new(intensity, intensity, intensity);
    /// <summary>Resolves a $9B:B835 ShadesOfWhite entry, retaining independent edits to the calculated two-rate whiteout.</summary>
    /// <param name="index">Whiteout shade 0..21, not an explosion frame; the terminal call selects 21 explicitly.</param>
    /// <returns>The RGB5 word used by the consumer to fill CGRAM 0..191 and 208..239, excluding suited and suitless Samus palettes.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside 0..21.</exception>
    public Bgr555 WhiteoutColor(int index) => whiteout.Resolve(index);

    /// <summary>Independent edits to the calculated two-rate whiteout.</summary>
    /// <remarks>Original9BB835..B860 needs no stored shade or anchor inputs.
    /// Keep only channels differing from DefaultWhiteoutColor; edits to any
    /// supplied shade remain independent of every other supplied shade.</remarks>
    private sealed class WhiteoutInputs
    {
        private readonly Dictionary<int, LoadingPaletteInputView.Channels> inputs = new();

        internal WhiteoutInputs(Bgr555[] source)
        {
            for (int index = 0; index < source.Length; index++)
            {
                Bgr555 expected = DefaultWhiteoutColor(index);
                if (source[index] != expected) inputs.Add(index, new(source[index], expected));
            }
        }

        internal Bgr555 Resolve(int index)
        {
            Bgr555 expected = DefaultWhiteoutColor(index);
            return inputs.TryGetValue(index, out var channels) ? channels.Apply(expected) : expected;
        }
    }

    /// <summary>Raises the background from the first nonblack shade to full white in two stages.</summary>
    /// <remarks>Original9BB835..B860 contains22 neutral RGB5 shades.
    /// Intensity rises1 to16 over indices0..7,then16 to31 over7..21:
    /// the first half of the brightness rise takes seven intervals,the second
    /// takes fourteen. First stage floors15*index/7 before adding1; second
    /// floors the remaining distance15*(21-index)/14 before subtracting from31.
    /// This preserves the native quantization at the shared half-brightness
    /// boundary without special sample corrections. Numerators are at most210;
    /// there is no wrap,saturation,extrapolation or generated lookup cache.
    /// Native9BB710 skips explosion frame0,then reads counter0..20 before
    /// incrementing and holds20. Terminal9BB772 selects21 explicitly. Preserve
    /// all22 supported selectors and reject others with the former bounds exception.
    /// This establishes an exact two-rate ramp,not the identity of its historical tool.</remarks>
    internal static Bgr555 DefaultWhiteoutColor(int index)
    {
        if ((uint)index >= SamusPaletteRomData.Death.WhiteoutShadeCount) throw new IndexOutOfRangeException();
        const int firstLitIntensity = 1, fullWhiteIntensity = 31;
        const int halfBrightnessIndex = 7, completionIndex = 21;
        const int halfRise = (fullWhiteIntensity - firstLitIntensity) / 2;
        int intensity = index <= halfBrightnessIndex
            ? firstLitIntensity + halfRise * index / halfBrightnessIndex
            : fullWhiteIntensity - halfRise * (completionIndex - index) / (completionIndex - halfBrightnessIndex);
        return Neutral(intensity);
    }
    /// <summary>Advances through death palettes while skipping the flash-only row.</summary>
    /// <remarks>Original odd bytes9BB824..B834 select0,2..9 for frame0..8.
    /// Index1 belongs to the preceding yellow-flash stage, not the explosion.
    /// The native caller advances the explosion frame before selecting its
    /// next palette and terminates at9 without another lookup. Edited asset
    /// selectors remain independent. Preserve the original array bounds exception.</remarks>
    /// <param name="frame">Explosion drawing index 0..8, selecting one of the nine native timing/selector records.</param>
    /// <returns>A row index 0..9 for both foreground palette families, not a RGB5 word or countdown duration.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="frame"/> is outside 0..8.</exception>
    public ushort ExplosionPaletteIndex(int frame)
    {
        ushort expected = DefaultExplosionPaletteIndex(frame);
        return explosionPaletteIndices.TryGetValue(frame, out ushort value) ? value : expected;
    }

    private static ushort DefaultExplosionPaletteIndex(int frame)
    {
        if ((uint)frame >= SamusDeathExplosionTimingDefinitions.RecordCount) throw new IndexOutOfRangeException();
        return (ushort)(frame == 0 ? 0 : frame + 1);
    }
}
