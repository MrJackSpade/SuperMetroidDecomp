using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Lower Norfair Ridley's installed colours at work: the initial and arena-reveal installs and their identity.</summary>
internal static class NorfairRidleyColors
{
    /// <summary>Installs the 32 initial RGB5 colors in colours 160..191.</summary>
    internal static void ApplyInitial(NorfairRidleyInitialPaint initial, GameplayColours colours)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < NorfairRidleyPaletteDefinitions.InitialColorCount; color++)
            new RidleyColours(colours).NorfairInitial.Write(color, initial.ColorAt(color));
    }

    /// <summary>Copies one arena-reveal step to colours 113..126.</summary>
    internal static void ApplyReveal(NorfairRidleyRevealPaint reveal, GameplayColours colours, int step)
    {
        NorfairRidleyRevealPaint.ValidateStep(step);
        Ensure.NotNull(colours);
        for (int color = 0; color < NorfairRidleyPaletteDefinitions.RevealColorCount; color++)
            new RidleyColours(colours).NorfairReveal.Write(color, reveal.ColorAt(step, color));
    }
}
