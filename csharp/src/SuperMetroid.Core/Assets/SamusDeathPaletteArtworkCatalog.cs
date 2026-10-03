using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable suit, suitless, and whiteout colors for Samus's death sequence.</summary>
/// <remarks>Palette choices are visual; the fatal-damage phases and frame durations remain compiled.</remarks>
public sealed class SamusDeathPaletteArtworkCatalog
{
    public const int SuitCount = 3;
    public const int ColorCount = SamusPaletteRomData.Common.ColorsPerObjPalette;

    private readonly Dictionary<int, ushort> suited = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> suitedFadeInputs = new();
    private readonly Dictionary<int, ushort> suitless = new();
    private readonly Dictionary<int, LoadingPaletteInputView.Channels> suitlessFadeInputs = new();
    private readonly ushort[] whiteout;
    private readonly ushort[] explosionPaletteIndices;

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
            if (palette == 1 && value == suitless[0][color]) continue;
            if (palette is >= 2 and <= 8)
            {
                ushort expected = SamusPaletteFade.EighthTowardWhite(suitless[0][color], palette - 1);
                if (value != expected) suitlessFadeInputs.Add(key, new(value, expected));
            }
            else this.suitless.Add(key, value);
        }
        this.whiteout = (ushort[])whiteout.Clone();
        this.explosionPaletteIndices = (ushort[])explosionPaletteIndices.Clone();
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
        content.AppendWords("whiteout", this.whiteout);
        content.AppendWords("explosion palette indices", this.explosionPaletteIndices);
    });

    /// <summary>Resolves the native suited palette view, calculating its seven whitening steps.</summary>
    /// <remarks>Native9B:B7D3..B80D selects base at0, yellow flash at1,
    /// eighth-step shades1..7 at indices2..8 and the final shared row at9.
    /// Flash/final/base inputs remain separate; differing edited fade channels
    /// override shared arithmetic. Preserve the former array bounds exception.</remarks>
    public ushort SuitedColor(int suit, int palette, int color)
    {
        if ((uint)suit >= SuitCount || (uint)palette >= SamusPaletteRomData.Death.PaletteCount || (uint)color >= ColorCount)
            throw new IndexOutOfRangeException();
        int key = (suit * SamusPaletteRomData.Death.PaletteCount + palette) * ColorCount + color;
        if (suited.TryGetValue(key, out ushort value)) return value;
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
        if (palette == 1) return SuitlessColor(0, color);
        ushort expected = SamusPaletteFade.EighthTowardWhite(SuitlessColor(0, color), palette - 1);
        return suitlessFadeInputs.TryGetValue(key, out var channels) ? channels.Apply(expected) : expected;
    }
    public ushort WhiteoutColor(int index) => whiteout[index];
    public ushort ExplosionPaletteIndex(int frame) => explosionPaletteIndices[frame];
}
