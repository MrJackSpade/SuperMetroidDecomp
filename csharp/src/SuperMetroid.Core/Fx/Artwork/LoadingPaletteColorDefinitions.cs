using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Calculated repeated-row identities in the three suit-loading palettes.</summary>
/// <remarks>Each suit's loading program contains nine
/// sixteen-color records. Every even record repeats the normal palette; record3
/// repeats record1. Records5 and7 contain distinct shades. The mapping is checked
/// against complete original rows, including transparent entries, for #1165.
/// The remaining25 whole words are22 distinct normal sprite inks plus three
/// transparent payloads ($3800/$0000/$2003). Opaque indices select categorical
/// painted pixels, not a numerical brightness/time input; native $91:DD5B
/// copies the same normal art from $9B:9400/9520/9800. An index formula would
/// only recite those chosen inks, the #1165 nonsense exception. OBJ rendering
/// skips index zero before palette lookup, so its differing source payloads
/// have no functional color-generation basis either. All432 original loading
/// words independently match their normal/speed-boost source art. Repeated
/// rows, suit sharing, shade transforms and derived endpoint channels are
/// calculated separately; LoadingPaletteInputView documents the nine retained
/// tint components. No complete generated palette row is cached. Other
/// full-body palette families still require their own review.</remarks>
public static class LoadingPaletteColorDefinitions
{
    /// <summary>The final record, which repeats the normal palette.</summary>
    private const int FinalRow = 8;

    /// <summary>The nine records of <paramref name="suit"/>'s loading program.</summary>
    internal static PaletteFxColorRows Rows(PaletteFxHeatSuit suit) => suit switch
    {
        PaletteFxHeatSuit.Power => SamusLoadingSuitPaletteFxColorDefinitions.All[0].Rows,
        PaletteFxHeatSuit.Varia => SamusLoadingSuitPaletteFxColorDefinitions.All[1].Rows,
        PaletteFxHeatSuit.Gravity => SamusLoadingSuitPaletteFxColorDefinitions.All[2].Rows,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown loading suit."),
    };

    /// <summary>The suit whose loading program writes <paramref name="key"/>, with its row and slot.</summary>
    private static bool TryLocate(PaletteFxColorKey key, out PaletteFxHeatSuit suit, out int row, out int slot)
    {
        foreach (PaletteFxHeatSuit candidate in (ReadOnlySpan<PaletteFxHeatSuit>)[PaletteFxHeatSuit.Power, PaletteFxHeatSuit.Varia, PaletteFxHeatSuit.Gravity])
        {
            if (Rows(candidate).TryLocate(key, out row, out slot))
            {
                suit = candidate;
                return true;
            }
        }
        suit = default;
        row = slot = 0;
        return false;
    }

    /// <summary>Maps a loading color to its earliest identical native row.</summary>
    /// <remarks>Rows zero through seven are four counted groups of a normal record and a shade record; row eight is
    /// the final record. Even rows and the final row repeat the normal row, row three repeats row one, and rows five
    /// and seven are the distinct shades.</remarks>
    public static bool TryCanonical(PaletteFxColorKey key, out PaletteFxColorKey canonical)
    {
        canonical = default;
        if (!TryLocate(key, out PaletteFxHeatSuit suit, out int row, out int slot)) return false;
        int group = row / 2;
        int canonicalRow = row == FinalRow || row % 2 == 0 ? 0 : group < 2 ? 1 : row;
        canonical = ShareSuit(suit, canonicalRow, slot);
        return true;
    }

    /// <summary>Resolves an explicit edit before following a calculated loading alias.</summary>
    internal static bool TryReadColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value) =>
        colors.TryGetValue(key, out value) ||
        (TryCanonical(key, out PaletteFxColorKey canonical) &&
         (colors.TryGetValue(canonical, out value) || TryCalculatedColor(canonical, colors, out value)));

    /// <summary>Calculates the shared blue/green loading tint from normal suit colors.</summary>
    /// <remarks>Power slots3..8/13..15 and Gravity slots10/11 preserve red.
    /// Bright, middle and dim shades add green15/5/0 and blue20/20/10,
    /// saturating each RGB5 channel at31. All33 original words independently
    /// confirm this channel transformation. Varia slots1/2 use the weaker green
    /// and stronger blue tint; slots10/11 share its middle and dim shades only.
    /// Their brightest blue29 is a separate tint input in LoadingPaletteInputView.
    /// Original Power dim slots10/11 preserve normal red/green and share dim
    /// slot2's blue13. Slot9's dim follows the ordinary blue+10 tint; its middle
    /// preserves brightest blue21 while applying normal green+5. These four
    /// additional words use shared channels. Dim slot0 shares middle slot0.
    /// Independent endpoint components are documented by LoadingPaletteInputView.
    /// Remaining base inks and tint components have the specific dispositions
    /// on this catalog and LoadingPaletteInputView; unrelated palettes are not exempt.</remarks>
    internal static bool TryCalculatedColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
    {
        value = RgbColor.Black;
        if (!TryLocate(key, out PaletteFxHeatSuit suit, out int row, out int slot) || row is not (1 or 5 or 7)) return false;
        PaletteFxColorRows rows = Rows(suit);
        int group = row / 2;
        int shade = group == 0 ? 0 : group - 1;
        bool power = suit == PaletteFxHeatSuit.Power;
        if (power && group == 3 && slot == 0)
            return TryReadColor(rows.Color(5, 0), colors, out value);
        if (power && group == 3 && slot is 10 or 11)
        {
            if (!TryReadColor(rows.Color(0, slot), colors, out RgbColor normal) ||
                !TryReadColor(rows.Color(7, 2), colors, out RgbColor blueSource)) return false;
            value = normal.WithBlue(blueSource.Blue);
            return true;
        }
        if (power && slot == 9 && group != 0)
        {
            if (!TryReadColor(rows.Color(0, slot), colors, out RgbColor normal)) return false;
            RgbColor tint = TintColor(normal, shade);
            if (group == 2)
            {
                if (!TryReadColor(rows.Color(1, slot), colors, out RgbColor bright)) return false;
                tint = tint.WithBlue(bright.Blue);
            }
            value = tint;
            return true;
        }
        // These tracks brighten an independently chosen dim tint, rather than
        // the normal suit color. The dim endpoint remains an editable input.
        if (group != 3 && (power ? slot is 1 or 2 or 10 or 11 or 12 :
            suit == PaletteFxHeatSuit.Varia ? slot == 12 : slot == 2))
        {
            if (!TryReadColor(rows.Color(7, slot), colors, out RgbColor dim)) return false;
            bool plateau = suit == PaletteFxHeatSuit.Gravity || (power && slot is 1 or 12);
            int greenPeak = plateau ? 15 : power && slot is 10 or 11 ? 5 : 0;
            value = BrightenDimColor(dim, shade, greenPeak, plateau);
            return true;
        }
        bool varia = suit == PaletteFxHeatSuit.Varia;
        if (!(power ? slot is >= 3 and <= 8 or >= 13 and <= 15 :
            varia ? slot is 1 or 2 || (group != 0 && slot is 10 or 11) : slot is 10 or 11)) return false;
        if (!TryReadColor(rows.Color(0, slot), colors, out RgbColor original)) return false;
        value = varia ? VariaTintColor(original, shade) : TintColor(original, shade);
        return true;
    }

    /// <summary>Brightens an independently chosen dim loading tint.</summary>
    /// <remarks>Original Power slots1/12 and Gravity slot2 add green15/5 and
    /// blue10/10 for bright/middle. Power slots2/10/11 and Varia slot12 add
    /// blue10/5; only Power slots10/11 also add green5/0. Red is unchanged,
    /// sums saturate at31. These14 words derive from seven editable dim
    /// endpoints whose independent channels are documented by LoadingPaletteInputView.
    /// Shade0/1 is bright/middle; greenPeak is an RGB5 additive amount.</remarks>
    internal static RgbColor BrightenDimColor(RgbColor dim, int shade, int greenPeak, bool bluePlateau)
    {
        if ((uint)shade >= 2) throw new ArgumentOutOfRangeException(nameof(shade));
        if ((uint)greenPeak > 31) throw new ArgumentOutOfRangeException(nameof(greenPeak));
        int green = Math.Min(31, dim.Green + Math.Max(0, greenPeak - 10 * shade));
        int blue = Math.Min(31, dim.Blue + (bluePlateau ? 10 : 10 - 5 * shade));
        return new(dim.Red, green, blue);
    }

    /// <summary>Applies Varia's blue-dominant loading tint to one RGB5 input.</summary>
    /// <remarks>Shade0..2 preserves red and adds green=max(0,5-5*shade),
    /// blue=30-10*shade, saturating at31. Original Varia slots1/2 follow all
    /// three levels; slots10/11 follow levels1/2. The brightest slots10/11
    /// are excluded because their blue is29 rather than30. All ten included
    /// words match the original program; no correction table is used.</remarks>
    internal static RgbColor VariaTintColor(RgbColor original, int shade)
    {
        if ((uint)shade >= 3) throw new ArgumentOutOfRangeException(nameof(shade));
        int green = Math.Min(31, original.Green + Math.Max(0, 5 - 5 * shade));
        int blue = Math.Min(31, original.Blue + 30 - 10 * shade);
        return new(original.Red, green, blue);
    }

    /// <summary>The loading color <paramref name="slot"/> names.</summary>
    internal static PaletteFxColorKey Key(LoadingSuitColor slot) => slot switch
    {
        LoadingSuitColor.PowerDim1 => Rows(PaletteFxHeatSuit.Power).Color(7, 1),
        LoadingSuitColor.PowerDim2 => Rows(PaletteFxHeatSuit.Power).Color(7, 2),
        LoadingSuitColor.PowerBright9 => Rows(PaletteFxHeatSuit.Power).Color(1, 9),
        LoadingSuitColor.PowerDim12 => Rows(PaletteFxHeatSuit.Power).Color(7, 12),
        LoadingSuitColor.VariaBright10 => Rows(PaletteFxHeatSuit.Varia).Color(1, 10),
        LoadingSuitColor.VariaBright11 => Rows(PaletteFxHeatSuit.Varia).Color(1, 11),
        LoadingSuitColor.VariaDim12 => Rows(PaletteFxHeatSuit.Varia).Color(7, 12),
        LoadingSuitColor.GravityDim2 => Rows(PaletteFxHeatSuit.Gravity).Color(7, 2),
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Not a calculated loading tint endpoint."),
    };

    /// <summary>The normal-suit color each calculated loading tint endpoint derives from.</summary>
    internal static PaletteFxColorKey NormalSource(LoadingSuitColor slot) => slot switch
    {
        LoadingSuitColor.PowerDim1 => Rows(PaletteFxHeatSuit.Power).Color(0, 1),
        LoadingSuitColor.PowerDim2 => Rows(PaletteFxHeatSuit.Power).Color(0, 2),
        LoadingSuitColor.PowerBright9 => Rows(PaletteFxHeatSuit.Power).Color(0, 9),
        LoadingSuitColor.PowerDim12 => Rows(PaletteFxHeatSuit.Power).Color(0, 12),
        LoadingSuitColor.VariaBright10 => Rows(PaletteFxHeatSuit.Varia).Color(0, 10),
        LoadingSuitColor.VariaBright11 => Rows(PaletteFxHeatSuit.Varia).Color(0, 11),
        LoadingSuitColor.VariaDim12 => Rows(PaletteFxHeatSuit.Varia).Color(0, 12),
        LoadingSuitColor.GravityDim2 => Rows(PaletteFxHeatSuit.Gravity).Color(0, 2),
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Not a calculated loading tint endpoint."),
    };

    /// <summary>Applies the three saturating loading tint levels to one RGB5 input.</summary>
    /// <remarks>Shade0..2 runs bright to dim. Green=max(0,15-10*shade),
    /// blue=min(20,30-10*shade); channel sums saturate at31, red is unchanged.</remarks>
    internal static RgbColor TintColor(RgbColor original, int shade)
    {
        if ((uint)shade >= 3) throw new ArgumentOutOfRangeException(nameof(shade));
        int green = Math.Min(31, original.Green + Math.Max(0, 15 - 10 * shade));
        int blue = Math.Min(31, original.Blue + Math.Min(20, 30 - 10 * shade));
        return new(original.Red, green, blue);
    }

    /// <summary>Chooses the independent suit ink or its identical Power counterpart; Varia normal slot zero aliases Power bright black rather than Power normal's payload.</summary>
    /// <remarks>Original complete RGB5 words establish each equality. Varia's
    /// normal row differs at0/2/10/11 and bright rows at1/2/10/11/12. Gravity's
    /// normal row differs at2/10/11/12 and bright rows at2/10/11. All other slots
    /// share the Power input, including transparent entries where equal.
    /// Varia normal slot0 also shares Power bright slot0 (both black); the
    /// differing Power normal slot0 payload remains separate.
    /// Differing custom colors remain explicit overrides before this mapping.</remarks>
    private static PaletteFxColorKey ShareSuit(PaletteFxHeatSuit suit, int row, int slot)
    {
        if (suit == PaletteFxHeatSuit.Varia && row == 0 && slot == 0) return Rows(PaletteFxHeatSuit.Power).Color(1, 0);
        bool distinct = suit == PaletteFxHeatSuit.Power || (suit == PaletteFxHeatSuit.Varia
            ? (row == 0 ? slot is 0 or 2 or 10 or 11 : slot is 1 or 2 or 10 or 11 or 12)
            : (row == 0 ? slot is 2 or 10 or 11 or 12 : slot is 2 or 10 or 11));
        return Rows(distinct ? suit : PaletteFxHeatSuit.Power).Color(row, slot);
    }
}
