using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which beam row each palette slot copies its colour from, and which slots are black.</summary>
public static class BeamPaletteSources
{
    /// <summary>$90:C3C9..C3E0 selects Ice before Plasma before Wave before Spazer before Power; the first two colors alias the common Power inputs in all five rows.</summary>
    internal static SamusBeamCombination ColorSourceSelection(SamusBeamCombination selection, int color)
    {
        if (color < 2) return SamusBeamCombination.Power;
        if (selection.HasIce) return SamusBeamCombination.Ice;
        if (selection.HasPlasma) return SamusBeamCombination.Plasma;
        if (selection.HasWave) return SamusBeamCombination.Wave;
        if (selection.HasSpazer) return SamusBeamCombination.Spazer;
        return SamusBeamCombination.Power;
    }

    /// <summary>$90:C3F3..C3FE and corresponding Power/Wave/Plasma/Spazer slots9..14 are black; Ice uses independent colored entries.</summary>
    internal static bool IsBlackSlot(SamusBeamCombination selection, int color) =>
        !selection.HasIce && color is >= 9 and <= 14;

    /// <summary>Returns the canonical JSON key for an ordinary beam combination using its uppercase two-digit hexadecimal selection index.</summary>
    /// <param name="selection">Retail beam combination; simultaneous Spazer/Plasma selections are outside this catalog.</param>
    /// <returns>A key from <c>beam-00</c> through <c>beam-0B</c>.</returns>
    public static string Key(SamusBeamCombination selection)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        return $"beam-{selection.TableIndex:X2}";
    }
}
