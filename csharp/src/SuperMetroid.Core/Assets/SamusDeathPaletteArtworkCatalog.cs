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
/// shades are calculated; independent asset edits stay local. It does not
/// settle suitless base colors,flash/final root channels,whiteout intensity
/// choices,explosion timing or any other separately indexed field.</remarks>
public sealed class SamusDeathPaletteArtworkCatalog
{
    public const int SuitCount = 3;
    public const int ColorCount = SamusPaletteRomData.Common.ColorsPerObjPalette;

    private readonly Dictionary<int, ushort> suited = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> suitedFadeInputs = new();
    private readonly Dictionary<int, ushort> suitless = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> suitlessFadeInputs = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> neutralInputs = new();
    private readonly WhiteoutInputs whiteout;
    private readonly Dictionary<int, ushort> explosionPaletteIndices = new();

    public SamusDeathPaletteArtworkCatalog(ushort[][][] suited, ushort[][] suitless,
        ushort[] whiteout, ushort[] explosionPaletteIndices)
    {
        ArgumentNullException.ThrowIfNull(suited);
        ArgumentNullException.ThrowIfNull(suitless);
        ArgumentNullException.ThrowIfNull(whiteout);
        ArgumentNullException.ThrowIfNull(explosionPaletteIndices);
        if (suited.Length != SuitCount ||
            suited.Any(family => family is null ||
                family.Length != SamusPaletteRomData.Death.PaletteCount ||
                family.Any(row => row is null || row.Length != ColorCount ||
                    row.Any(color => color > 0x7fff))) ||
            suitless.Length != SamusPaletteRomData.Death.PaletteCount ||
            suitless.Any(row => row is null || row.Length != ColorCount ||
                row.Any(color => color > 0x7fff)) ||
            whiteout.Length != SamusPaletteRomData.Death.WhiteoutShadeCount ||
            whiteout.Any(color => color > 0x7fff) ||
            explosionPaletteIndices.Length != SamusDeathExplosionTimingDefinitions.RecordCount ||
            explosionPaletteIndices.Any(index => index >= SamusPaletteRomData.Death.PaletteCount))
            throw new InvalidDataException("Samus death-palette artwork has an invalid shape or RGB5 color.");
        for (int suit = 0; suit < SuitCount; suit++)
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        for (int color = 0; color < ColorCount; color++)
        {
            int key = (suit * SamusPaletteRomData.Death.PaletteCount + palette) * ColorCount + color;
            ushort value = suited[suit][palette][color];
            if (palette == 0 && suit != 0 && value == suited[0][0][color]) continue;
            if (palette == 1 && (suit != 0 || color != 0) && value == suited[0][1][0]) continue;
            if (palette == 9 && value == suitless[9][0]) continue;
            if (palette is >= 2 and <= 8)
            {
                ushort expected = SamusPaletteFade.EighthTowardWhite(suited[suit][0][color], palette - 1);
                if (value != expected) suitedFadeInputs.Add(key, new(value, expected));
            }
            else this.suited.Add(key, value);
        }
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        for (int color = 0; color < ColorCount; color++)
        {
            int key = palette * ColorCount + color;
            ushort value = suitless[palette][color];
            if (palette == 9 && color != 0 && value == suitless[9][0]) continue;
            if (palette == 1 && value == suitless[0][color]) continue;
            if (palette == 0 && (color == 5 || color >= 11) || palette == 9 && color == 0)
            {
                neutralInputs.Add(key, new(value, NeutralFromRed(value), independentMask: 1));
                continue;
            }
            if (palette is >= 2 and <= 8)
            {
                ushort expected = SamusPaletteFade.EighthTowardWhite(suitless[0][color], palette - 1);
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
        Span<ushort> suitedRow = stackalloc ushort[ColorCount];
        for (int suit = 0; suit < SuitCount; suit++)
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        {
            for (int color = 0; color < ColorCount; color++) suitedRow[color] = SuitedColor(suit, palette, color);
            content.AppendWords("suited row", suitedRow);
        }
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        {
            for (int color = 0; color < ColorCount; color++) suitedRow[color] = SuitlessColor(palette, color);
            content.AppendWords("suitless row", suitedRow);
        }
        Span<ushort> whiteoutColors = stackalloc ushort[SamusPaletteRomData.Death.WhiteoutShadeCount];
        for (int index = 0; index < whiteoutColors.Length; index++) whiteoutColors[index] = WhiteoutColor(index);
        content.AppendWords("whiteout", whiteoutColors);
        Span<ushort> selectors = stackalloc ushort[SamusDeathExplosionTimingDefinitions.RecordCount];
        for (int frame = 0; frame < selectors.Length; frame++) selectors[frame] = ExplosionPaletteIndex(frame);
        content.AppendWords("explosion palette indices", selectors);
    });

    /// <summary>Resolves the native suited palette view, calculating its seven whitening steps.</summary>
    /// <remarks>Native9B:B7D3..B80D selects base at0, yellow flash at1,
    /// eighth-step shades1..7 at indices2..8 and the final shared row at9.
    /// Original flash9B9420 repeats one yellow color across all sixteen inks
    /// and suits; final9BA220 repeats one gray color shared with suitless.
    /// They each use one editable color input, with independent edits overriding
    /// sharing. Base rows9820/9920/9A20 share the common Power Suit inks;
    /// Varia differs at2/10/11,Gravity at0/1/2/10/11/12. Independently supplied
    /// differences remain inputs,including source-only edits. Differing fade channels
    /// override shared arithmetic. Preserve the former array bounds exception.</remarks>
    public ushort SuitedColor(int suit, int palette, int color)
    {
        if ((uint)suit >= SuitCount || (uint)palette >= SamusPaletteRomData.Death.PaletteCount || (uint)color >= ColorCount)
            throw new IndexOutOfRangeException();
        int key = (suit * SamusPaletteRomData.Death.PaletteCount + palette) * ColorCount + color;
        if (suited.TryGetValue(key, out ushort value)) return value;
        if (palette == 0) return SuitedColor(0, 0, color);
        if (palette == 1) return SuitedColor(0, 1, 0);
        if (palette == 9) return SuitlessColor(9, 0);
        ushort expected = SamusPaletteFade.EighthTowardWhite(SuitedColor(suit, 0, color), palette - 1);
        return suitedFadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }
    /// <summary>Resolves the same eighth-step whitening for suitless Samus.</summary>
    /// <remarks>Original9B:B80F..B822 repeats the base at indices0/1,
    /// selects shade1..7 at2..8 and a distinct final row at9. Independent
    /// edits, including the repeated base row, retain their supplied values.</remarks>
    public ushort SuitlessColor(int palette, int color)
    {
        if ((uint)palette >= SamusPaletteRomData.Death.PaletteCount || (uint)color >= ColorCount)
            throw new IndexOutOfRangeException();
        int key = palette * ColorCount + color;
        if (suitless.TryGetValue(key, out ushort value)) return value;
        if (neutralInputs.TryGetValue(key, out var neutral))
            return neutral.Apply(NeutralFromRed(neutral.Apply(0)));
        if (palette == 9) return SuitlessColor(9, 0);
        if (palette == 1) return SuitlessColor(0, color);
        ushort expected = SamusPaletteFade.EighthTowardWhite(SuitlessColor(0, color), palette - 1);
        return suitlessFadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }
    /// <summary>Shares the intensity across neutral RGB5 channels.</summary>
    /// <remarks>Original suitless base9BA120 inks5/11..15 and final9BA220
    /// all have red=green=blue. Their intensity remains an independent input;
    /// green/blue edits override the shared value. The supplied red channel
    /// is bounded0..31; bit replication needs no rounding or saturation.
    /// This converts channel duplication,not the separate intensity choices.</remarks>
    private static ushort NeutralFromRed(ushort color) => (ushort)((color & 31) * 0x421);
    public ushort WhiteoutColor(int index) => whiteout.Resolve(index);

    /// <summary>Two linear grayscale segments and an independently supplied transition shade.</summary>
    /// <remarks>Original9BB835..B860 has a linear intensity ramp at indices0..6,
    /// a separate shade at7 and a second linear ramp at8..21. Each RGB word is
    /// neutral. Five endpoint/transition intensities remain independent inputs;
    /// their own disposition is not established by this interpolation conversion.
    /// Differing edited channels override the calculated grayscale. Interpolation
    /// stays within RGB5 even when endpoints are independently edited.</remarks>
    private sealed class WhiteoutInputs
    {
        private readonly int earlyStart, earlyEnd, transition, lateStart, lateEnd;
        private readonly Dictionary<int, LoadingPaletteInputView.Channels> inputs = new();

        internal WhiteoutInputs(ushort[] source)
        {
            earlyStart = source[0] & 31;
            earlyEnd = source[6] & 31;
            transition = source[7] & 31;
            lateStart = source[8] & 31;
            lateEnd = source[21] & 31;
            for (int index = 0; index < source.Length; index++)
            {
                ushort expected = Expected(index);
                if (source[index] != expected) inputs.Add(index, new(source[index], expected));
            }
        }

        internal ushort Resolve(int index)
        {
            if ((uint)index >= SamusPaletteRomData.Death.WhiteoutShadeCount) throw new IndexOutOfRangeException();
            ushort expected = Expected(index);
            return inputs.TryGetValue(index, out var channels) ? channels.Apply(expected) : expected;
        }

        private ushort Expected(int index)
        {
            int intensity = index <= 6 ? InterpolateWhiteoutIntensity(earlyStart, earlyEnd, index, 6) :
                index == 7 ? transition : InterpolateWhiteoutIntensity(lateStart, lateEnd, index - 8, 13);
            return (ushort)(intensity | intensity << 5 | intensity << 10);
        }
    }

    /// <summary>Interpolates RGB5 intensity between whiteout segment endpoints, rounding down.</summary>
    /// <remarks>Only native six-step and thirteen-step spans are supported.
    /// Nonnegative weighted sums are bounded by403. Descending edited endpoints
    /// remain valid; no extrapolation, saturation or signed division is required.</remarks>
    internal static int InterpolateWhiteoutIntensity(int first, int last, int step, int steps)
    {
        if ((uint)first > 31 || (uint)last > 31) throw new ArgumentOutOfRangeException(nameof(first));
        if (steps is not (6 or 13)) throw new ArgumentOutOfRangeException(nameof(steps));
        if ((uint)step > steps) throw new ArgumentOutOfRangeException(nameof(step));
        return (first * (steps - step) + last * step) / steps;
    }
    /// <summary>Advances through death palettes while skipping the flash-only row.</summary>
    /// <remarks>Original odd bytes9BB824..B834 select0,2..9 for frame0..8.
    /// Index1 belongs to the preceding yellow-flash stage, not the explosion.
    /// The native caller advances the explosion frame before selecting its
    /// next palette and terminates at9 without another lookup. Edited asset
    /// selectors remain independent. Preserve the original array bounds exception.</remarks>
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
