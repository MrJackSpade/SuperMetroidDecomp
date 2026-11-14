using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>Lower Norfair Ridley's installed initial and arena-reveal colours, kept as installed.</summary>
public sealed class NorfairRidleyColorCatalog
{
    /// <summary>The installed initial colours.</summary>
    private readonly InstalledColourRows initial;
    /// <summary>The installed reveal steps.</summary>
    private readonly InstalledColourRows reveal;

    /// <summary>Keeps the complete installed initial palette and reveal steps.</summary>
    /// <param name="initial">The initial palette's colours.</param>
    /// <param name="reveal">Each reveal step's colours.</param>
    /// <exception cref="InvalidDataException">A step or a colour is missing.</exception>
    internal NorfairRidleyColorCatalog(RgbColor[] initial, RgbColor[][] reveal)
    {
        this.initial = new InstalledColourRows(initial, NorfairRidleyColorFormat.InitialColorCount, "Norfair Ridley initial");
        this.reveal = new InstalledColourRows(reveal, NorfairRidleyRevealDefinitions.RevealStepCount,
            NorfairRidleyColorFormat.RevealColorCount, "Norfair Ridley reveal");
    }

    /// <summary>One of Ridley's installed initial colours.</summary>
    internal RgbColor ResolveInitial(int color) => initial.Colour(color);

    /// <summary>One installed colour of a reveal step.</summary>
    internal RgbColor ResolveReveal(int step, int color) => reveal.Colour(step, color);
}
