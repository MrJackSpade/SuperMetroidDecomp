using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The Zebetites' installed barrier pulse colours, every frame kept as installed.</summary>
public sealed class ZebetiteColorCatalog
{
    /// <summary>The installed pulse frames.</summary>
    private readonly InstalledColourRows frames;

    /// <summary>Keeps every complete installed pulse frame.</summary>
    /// <param name="frames">Each pulse frame's colours.</param>
    /// <exception cref="InvalidDataException">A frame or a colour is missing.</exception>
    internal ZebetiteColorCatalog(RgbColor[][] frames) => this.frames = new InstalledColourRows(frames,
        ZebetiteColorFormat.FrameCount, ZebetiteColorFormat.ColorsPerFrame, "Zebetite pulse");

    /// <summary>The installed colour <paramref name="color"/> of pulse frame <paramref name="frame"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame or colour is outside the pulse.</exception>
    public RgbColor Resolve(int frame, int color) => frames.Colour(frame, color);
}
