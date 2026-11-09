using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Fx.Artwork.LoadingPaletteColorDefinitions;

using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// Calculates the Samus loading palette effect's colours (<see cref="LoadingPaletteColorDefinitions"/>): the canonical frames, the
/// brightened and tinted suit colours, and the colours shared between suits.
/// </summary>
internal static class LoadingPaletteColors
{
    /// <summary>The nine records of <paramref name="suit"/>'s loading program.</summary>
    internal static PaletteFxColorFrames ColorFrames(PaletteFxHeatSuit suit) => suit switch
    {
        PaletteFxHeatSuit.Power => SamusLoadingSuitPaletteFxColorDefinitions.All[0].ColorFrames,
        PaletteFxHeatSuit.Varia => SamusLoadingSuitPaletteFxColorDefinitions.All[1].ColorFrames,
        PaletteFxHeatSuit.Gravity => SamusLoadingSuitPaletteFxColorDefinitions.All[2].ColorFrames,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown loading suit."),
    };

    /// <summary>The suit whose loading program writes <paramref name="key"/>, with its frame and slot.</summary>
    private static bool TryLocate(PaletteFxColorKey key, out PaletteFxHeatSuit suit, out int frame, out int slot)
    {
        foreach (PaletteFxHeatSuit candidate in (ReadOnlySpan<PaletteFxHeatSuit>)[PaletteFxHeatSuit.Power, PaletteFxHeatSuit.Varia, PaletteFxHeatSuit.Gravity])
        {
            if (ColorFrames(candidate).TryLocate(key, out frame, out slot))
            {
                suit = candidate;
                return true;
            }
        }
        suit = default;
        frame = slot = 0;
        return false;
    }

    /// <summary>Maps a loading color to its earliest identical frame.</summary>
    /// <remarks>Frames zero through seven are four counted groups of a normal record and a shade record; frame eight is
    /// the final record. Even frames and the final frame repeat the normal frame, frame three repeats frame one, and
    /// frames five and seven are the distinct shades.</remarks>
    public static bool TryCanonical(PaletteFxColorKey key, out PaletteFxColorKey canonical)
    {
        canonical = default;
        if (!TryLocate(key, out PaletteFxHeatSuit suit, out int frame, out int slot)) return false;
        int group = frame / 2;
        int canonicalFrame = frame == FinalFrame || frame % 2 == 0 ? 0 : group < 2 ? 1 : frame;
        canonical = ShareSuit(suit, canonicalFrame, slot);
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
    /// saturating each level at31. All33 original words independently
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
        if (!TryLocate(key, out PaletteFxHeatSuit suit, out int frame, out int slot) || frame is not (1 or 5 or 7)) return false;
        PaletteFxColorFrames frames = ColorFrames(suit);
        int group = frame / 2;
        int shade = group == 0 ? 0 : group - 1;
        bool power = suit == PaletteFxHeatSuit.Power;
        if (power && group == 3 && slot == 0)
            return TryReadColor(frames.Color(5, 0), colors, out value);
        if (power && group == 3 && slot is 10 or 11)
        {
            if (!TryReadColor(frames.Color(0, slot), colors, out RgbColor normal) ||
                !TryReadColor(frames.Color(7, 2), colors, out RgbColor blueSource)) return false;
            value = normal.FadeLevels.WithBlue(blueSource.FadeLevels.Blue).ToColour();
            return true;
        }
        if (power && slot == 9 && group != 0)
        {
            if (!TryReadColor(frames.Color(0, slot), colors, out RgbColor normal)) return false;
            RgbColor tint = TintColor(normal, shade);
            if (group == 2)
            {
                if (!TryReadColor(frames.Color(1, slot), colors, out RgbColor bright)) return false;
                tint = tint.FadeLevels.WithBlue(bright.FadeLevels.Blue).ToColour();
            }
            value = tint;
            return true;
        }
        // These tracks brighten an independently chosen dim tint, rather than
        // the normal suit color. The dim endpoint remains an editable input.
        if (group != 3 && (power ? slot is 1 or 2 or 10 or 11 or 12 :
            suit == PaletteFxHeatSuit.Varia ? slot == 12 : slot == 2))
        {
            if (!TryReadColor(frames.Color(7, slot), colors, out RgbColor dim)) return false;
            bool plateau = suit == PaletteFxHeatSuit.Gravity || (power && slot is 1 or 12);
            int greenPeak = plateau ? 15 : power && slot is 10 or 11 ? 5 : 0;
            value = BrightenDimColor(dim, shade, greenPeak, plateau);
            return true;
        }
        bool varia = suit == PaletteFxHeatSuit.Varia;
        if (!(power ? slot is >= 3 and <= 8 or >= 13 and <= 15 :
            varia ? slot is 1 or 2 || (group != 0 && slot is 10 or 11) : slot is 10 or 11)) return false;
        if (!TryReadColor(frames.Color(0, slot), colors, out RgbColor original)) return false;
        value = varia ? VariaTintColor(original, shade) : TintColor(original, shade);
        return true;
    }

    /// <summary>Brightens an independently chosen dim loading tint.</summary>
    /// <remarks>Original Power slots1/12 and Gravity slot2 add green15/5 and
    /// blue10/10 for bright/middle. Power slots2/10/11 and Varia slot12 add
    /// blue10/5; only Power slots10/11 also add green5/0. Red is unchanged,
    /// sums saturate at31. These14 words derive from seven editable dim
    /// endpoints whose independent channels are documented by LoadingPaletteInputView.
    /// Shade0/1 is bright/middle; greenPeak is an additive number of levels.</remarks>
    internal static RgbColor BrightenDimColor(RgbColor dim, int shade, int greenPeak, bool bluePlateau)
    {
        if ((uint)shade >= 2) throw new ArgumentOutOfRangeException(nameof(shade));
        if ((uint)greenPeak > 31) throw new ArgumentOutOfRangeException(nameof(greenPeak));
        FadeLevelColour dimLevels = dim.FadeLevels;
        int green = Math.Min(31, dimLevels.Green + Math.Max(0, greenPeak - 10 * shade));
        int blue = Math.Min(31, dimLevels.Blue + (bluePlateau ? 10 : 10 - 5 * shade));
        return new FadeLevelColour(dimLevels.Red, green, blue).ToColour();
    }

    /// <summary>Applies Varia's blue-dominant loading tint to one colour.</summary>
    /// <remarks>Shade0..2 preserves red and adds green=max(0,5-5*shade),
    /// blue=30-10*shade, saturating at31. Original Varia slots1/2 follow all
    /// three levels; slots10/11 follow levels1/2. The brightest slots10/11
    /// are excluded because their blue is29 rather than30. All ten included
    /// words match the original program; no correction table is used.</remarks>
    internal static RgbColor VariaTintColor(RgbColor original, int shade)
    {
        if ((uint)shade >= 3) throw new ArgumentOutOfRangeException(nameof(shade));
        FadeLevelColour originalLevels = original.FadeLevels;
        int green = Math.Min(31, originalLevels.Green + Math.Max(0, 5 - 5 * shade));
        int blue = Math.Min(31, originalLevels.Blue + 30 - 10 * shade);
        return new FadeLevelColour(originalLevels.Red, green, blue).ToColour();
    }

    /// <summary>The loading color <paramref name="slot"/> names.</summary>
    internal static PaletteFxColorKey Key(LoadingSuitColor slot) => slot switch
    {
        LoadingSuitColor.PowerDim1 => ColorFrames(PaletteFxHeatSuit.Power).Color(7, 1),
        LoadingSuitColor.PowerDim2 => ColorFrames(PaletteFxHeatSuit.Power).Color(7, 2),
        LoadingSuitColor.PowerBright9 => ColorFrames(PaletteFxHeatSuit.Power).Color(1, 9),
        LoadingSuitColor.PowerDim12 => ColorFrames(PaletteFxHeatSuit.Power).Color(7, 12),
        LoadingSuitColor.VariaBright10 => ColorFrames(PaletteFxHeatSuit.Varia).Color(1, 10),
        LoadingSuitColor.VariaBright11 => ColorFrames(PaletteFxHeatSuit.Varia).Color(1, 11),
        LoadingSuitColor.VariaDim12 => ColorFrames(PaletteFxHeatSuit.Varia).Color(7, 12),
        LoadingSuitColor.GravityDim2 => ColorFrames(PaletteFxHeatSuit.Gravity).Color(7, 2),
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Not a calculated loading tint endpoint."),
    };

    /// <summary>The normal-suit color each calculated loading tint endpoint derives from.</summary>
    internal static PaletteFxColorKey NormalSource(LoadingSuitColor slot) => slot switch
    {
        LoadingSuitColor.PowerDim1 => ColorFrames(PaletteFxHeatSuit.Power).Color(0, 1),
        LoadingSuitColor.PowerDim2 => ColorFrames(PaletteFxHeatSuit.Power).Color(0, 2),
        LoadingSuitColor.PowerBright9 => ColorFrames(PaletteFxHeatSuit.Power).Color(0, 9),
        LoadingSuitColor.PowerDim12 => ColorFrames(PaletteFxHeatSuit.Power).Color(0, 12),
        LoadingSuitColor.VariaBright10 => ColorFrames(PaletteFxHeatSuit.Varia).Color(0, 10),
        LoadingSuitColor.VariaBright11 => ColorFrames(PaletteFxHeatSuit.Varia).Color(0, 11),
        LoadingSuitColor.VariaDim12 => ColorFrames(PaletteFxHeatSuit.Varia).Color(0, 12),
        LoadingSuitColor.GravityDim2 => ColorFrames(PaletteFxHeatSuit.Gravity).Color(0, 2),
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Not a calculated loading tint endpoint."),
    };

    /// <summary>Applies the three saturating loading tint levels to one colour.</summary>
    /// <remarks>Shade0..2 runs bright to dim. Green=max(0,15-10*shade),
    /// blue=min(20,30-10*shade); channel sums saturate at31, red is unchanged.</remarks>
    internal static RgbColor TintColor(RgbColor original, int shade)
    {
        if ((uint)shade >= 3) throw new ArgumentOutOfRangeException(nameof(shade));
        FadeLevelColour originalLevels = original.FadeLevels;
        int green = Math.Min(31, originalLevels.Green + Math.Max(0, 15 - 10 * shade));
        int blue = Math.Min(31, originalLevels.Blue + Math.Min(20, 30 - 10 * shade));
        return new FadeLevelColour(originalLevels.Red, green, blue).ToColour();
    }

    /// <summary>Chooses the independent suit ink or its identical Power counterpart; Varia normal slot zero aliases Power bright black rather than Power normal's payload.</summary>
    /// <remarks>Original complete colours establish each equality. Varia's
    /// normal frame differs at0/2/10/11 and bright frames at1/2/10/11/12. Gravity's
    /// normal frame differs at2/10/11/12 and bright frames at2/10/11. All other slots
    /// share the Power input, including transparent entries where equal.
    /// Varia normal slot0 also shares Power bright slot0 (both black); the
    /// differing Power normal slot0 payload remains separate.
    /// Differing custom colors remain explicit overrides before this mapping.</remarks>
    private static PaletteFxColorKey ShareSuit(PaletteFxHeatSuit suit, int frame, int slot)
    {
        if (suit == PaletteFxHeatSuit.Varia && frame == 0 && slot == 0) return ColorFrames(PaletteFxHeatSuit.Power).Color(1, 0);
        bool distinct = suit == PaletteFxHeatSuit.Power || (suit == PaletteFxHeatSuit.Varia
            ? (frame == 0 ? slot is 0 or 2 or 10 or 11 : slot is 1 or 2 or 10 or 11 or 12)
            : (frame == 0 ? slot is 2 or 10 or 11 or 12 : slot is 2 or 10 or 11));
        return ColorFrames(distinct ? suit : PaletteFxHeatSuit.Power).Color(frame, slot);
    }
}
