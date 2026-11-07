using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Which single beam's paint each palette slot copies its colour from, and which slots are black.</summary>
public static class BeamPaletteSources
{
    /// <summary>The beam selection picks Ice before Plasma before Wave before Spazer before Power; the first two colors alias the common Power inputs in all five beams' paints.</summary>
    internal static SamusBeamCombination ColorSourceSelection(SamusBeamCombination selection, int color)
    {
        if (color < 2) return SamusBeamCombination.Power;
        if (selection.HasIce) return SamusBeamCombination.Ice;
        if (selection.HasPlasma) return SamusBeamCombination.Plasma;
        if (selection.HasWave) return SamusBeamCombination.Wave;
        if ((selection.Components & SamusBeamFlags.Spazer) != SamusBeamFlags.None) return SamusBeamCombination.Spazer;
        return SamusBeamCombination.Power;
    }

    /// <summary>Power/Wave/Plasma/Spazer slots9..14 are black; Ice uses independent colored entries.</summary>
    internal static bool IsBlackSlot(SamusBeamCombination selection, int color) =>
        !selection.HasIce && color is >= 9 and <= 14;
}
