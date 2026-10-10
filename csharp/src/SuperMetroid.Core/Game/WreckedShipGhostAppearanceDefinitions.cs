using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Calculated appearance geometry, palette channels and visibility timing for Wrecked Ship ghost appearances.</summary>
public static class WreckedShipGhostAppearanceDefinitions
{
    /// <summary>$A8:99AE-99BC: categorical olive paint levels retained as chosen artwork content; these pixel labels do not encode a brightness scale.</summary>
    private static readonly byte[] OliveLevels = [31, 23, 9, 5, 26, 21, 16, 14];
    /// <summary>$A8:99BE-99CA: warm paint selected by shared Kago cavity/bug artwork; retained as categorical color content, not inferred from ghost-unused slots.</summary>
    private static readonly byte[] WarmRed = [31, 31, 24, 10, 25, 10, 5];
    /// <summary>$A8:99BE/99C6: chosen orange/ochre paint components retained as categorical artwork content.</summary>
    private static readonly byte[] WarmGreen = [14, 21];
    /// <summary>$A8:99AC: transparent slot0 blue14 has no visible color-generating rule, but $9B9B/$9E88 copy and fade this exact target payload.</summary>
    private const int BackdropBlue = 14;
    /// <summary>$A8:99AE-99C6: chosen olive tint and highlight/ochre blue components retained as categorical artwork content.</summary>
    private const int OliveBlueReduction = 7, BrightOliveBlue = 21,
        MiddleOliveBlue = 13, OchreBlue = 1;
    /// <summary>$A8:99C8/99CA: both brown bug shades share G=R-3 and B=0; the chosen hue separation is retained as categorical artwork content.</summary>
    private const int BrownGreenReduction = 3;

    /// <summary>
    /// $A8:99AC-99CB, Palette_Coven: olive colors share red/green and a clamped
    /// blue reduction except slots1/6. Warm slots have zero blue except ochre;
    /// slots10..12 are pure red. The 22 remaining paint/tint inputs and their family
    /// assignments are chosen artwork content: generating them from numeric pixel
    /// labels would invent different colors or restate the same choices. Slot0 is
    /// separately retained as copied/faded target data with no visual generating rule.
    /// Unused sprite slots are still copied and faded as native target data.
    /// </summary>
    public static Bgr555 PaletteColor(int index)
    {
        if ((uint)index >= 16) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == 0) return new(0, 0, BackdropBlue);
        if (index <= OliveLevels.Length)
        {
            int level = OliveLevels[index - 1];
            int blue = index == 1 ? BrightOliveBlue : index == 6 ? MiddleOliveBlue : Math.Max(0, level - OliveBlueReduction);
            return new(level, level, blue);
        }
        int warm = index - 9;
        int green = index == 9 ? WarmGreen[0] : index == 13 ? WarmGreen[1]
            : index >= 14 ? WarmRed[warm] - BrownGreenReduction : 0;
        return new(WarmRed[warm], green, index == 13 ? OchreBlue : 0);
    }
    /// <summary>$A8:9AA8 contains the nine row-major positions of a three-by-three spawn grid.</summary>
    public const int SpawnCount = 9;

    /// <summary>$A8:9ACC contains sixteen alternating intervals followed by the $FFFF terminator.</summary>
    public const int FlickerCount = 17;

    /// <summary>
    /// Calculates the signed pair at $A8:9AA8 + 4*i, i=0..8. Horizontal and vertical
    /// movement classes select columns and rows, each spaced 64 pixels around Samus.
    /// The upper-right approach spawns level with Samus: its native Y word at $A8:9AB2
    /// is zero, consumed directly by the position addition at $A8:9DD1.
    /// </summary>
    public static (short X, short Y) SpawnOffset(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, SpawnCount);
        int horizontalDirection = index % 3 - 1;
        int verticalDirection = index / 3 - 1;
        bool approachingUpperRight = horizontalDirection > 0 && verticalDirection < 0;
        return ((short)(64 * horizontalDirection),
            (short)(approachingUpperRight ? 0 : 64 * verticalDirection));
    }

    /// <summary>
    /// Calculates $A8:9ACC + 2*i, i=0..16. Each four-interval stage repeats twice:
    /// the even interval grows from max(1, stage), the odd interval falls from 8-stage.
    /// Index16 terminates flickering. Native appearance starts at index1 but index0
    /// remains valid when the caller resets its offset after the terminator.
    /// </summary>
    public static short FlickerDuration(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, FlickerCount);
        if (index == FlickerCount - 1)
            return -1;
        int stage = index / 4;
        return (short)((index & 1) == 0 ? Math.Max(1, stage) : 8 - stage);
    }
}
