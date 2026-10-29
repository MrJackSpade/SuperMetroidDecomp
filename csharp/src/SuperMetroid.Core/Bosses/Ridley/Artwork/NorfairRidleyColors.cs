using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Lower Norfair Ridley's installed colours at work: the initial and arena-reveal installs and their identity.</summary>
internal static class NorfairRidleyColors
{
    /// <summary>Hashes the initial colours, then the fifteen reveal steps.</summary>
    internal static string ContentIdentity(NorfairRidleyColorCatalog colors) =>
        SelectedPresentationHash.Create("NorfairRidleyColorCatalog-v1", content =>
        {
            Span<RgbColor> initialWords = stackalloc RgbColor[NorfairRidleyPaletteDefinitions.InitialColorCount];
            for (int color = 0; color < initialWords.Length; color++) initialWords[color] = colors.selection.Initial.ColorAt(color);
            content.AppendColors("initial", initialWords);
            RgbColor[][] revealWords = new RgbColor[NorfairRidleyPaletteDefinitions.RevealStepCount][];
            for (int step = 0; step < revealWords.Length; step++)
            {
                revealWords[step] = new RgbColor[NorfairRidleyPaletteDefinitions.RevealColorCount];
                for (int color = 0; color < revealWords[step].Length; color++) revealWords[step][color] = colors.selection.Reveal.ColorAt(step, color);
            }
            content.AppendColorFrames("reveal", revealWords);
        });

    /// <summary>Installs the 32 initial RGB5 colors in CGRAM entries 160..191.</summary>
    internal static void ApplyInitial(NorfairRidleyInitialPaint initial, GameplayColours colours)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < NorfairRidleyPaletteDefinitions.InitialColorCount; color++)
            new RidleyColours(colours).NorfairInitial.Write(color, initial.ColorAt(color));
    }

    /// <summary>Copies one arena-reveal step to CGRAM entries 113..126.</summary>
    internal static void ApplyReveal(NorfairRidleyRevealPaint reveal, GameplayColours colours, int step)
    {
        NorfairRidleyRevealPaint.ValidateStep(step);
        Ensure.NotNull(colours);
        for (int color = 0; color < NorfairRidleyPaletteDefinitions.RevealColorCount; color++)
            new RidleyColours(colours).NorfairReveal.Write(color, reveal.ColorAt(step, color));
    }
}
