using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>$96:FF14: composes the four illustrated intro BG pages from their drawing steps
/// (<see cref="IntroBackgroundPageDefinitions"/>).</summary>
internal static class IntroBackgroundPages
{
    /// <summary>Composes the four 32×32 intro pages in native page order, filling border tiles before drawing each scene.</summary>
    internal static BackgroundTile[] Compile()
    {
        if (IntroBackgroundPageDefinitions.Pages.Length != IntroCinematicArtworkFormat.BackgroundPageCount)
            throw new InvalidOperationException(
                $"Intro background pages define {IntroBackgroundPageDefinitions.Pages.Length} tilemaps for {IntroCinematicArtworkFormat.BackgroundPageCount} pages.");
        var output = new BackgroundTile[IntroCinematicArtworkFormat.BackgroundPageCount * IntroBackgroundCanvas.Cells];
        for (int page = 0; page < IntroCinematicArtworkFormat.BackgroundPageCount; page++)
        {
            var canvas = new IntroBackgroundCanvas(output, page);
            canvas.Fill(new(0, 0), new(32, 32), IntroBackgroundPageDefinitions.BorderBlank);
            foreach (IntroBackgroundStroke stroke in IntroBackgroundPageDefinitions.Pages[page]) stroke.Draw(canvas);
        }
        return output;
    }
}
