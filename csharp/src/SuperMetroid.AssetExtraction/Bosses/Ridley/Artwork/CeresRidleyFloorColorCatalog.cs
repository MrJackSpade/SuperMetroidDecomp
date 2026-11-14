using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>The installed Ceres Ridley Mode-7 zoom shades, kept as installed, separate from movement and rotation.</summary>
public sealed class CeresRidleyFloorColorCatalog
{
    /// <summary>The installed shades, by getaway zoom step.</summary>
    private readonly InstalledColourRows steps;

    /// <summary>Keeps every zoom step's complete installed shades.</summary>
    /// <param name="steps">Each zoom step's colours, as installed.</param>
    /// <exception cref="InvalidDataException">A step or a colour is missing.</exception>
    internal CeresRidleyFloorColorCatalog(RgbColor[][] steps) => this.steps = new InstalledColourRows(steps,
        CeresRidleyShadeDefinitions.GetawayZoomStepCount, CeresRidleyColorFormat.GetawayZoomColorCount, "Ceres Ridley zoom");

    /// <summary>The installed colour <paramref name="color"/> of getaway zoom step <paramref name="zoomStep"/>.</summary>
    internal RgbColor Resolve(int zoomStep, int color) => steps.Colour(zoomStep, color);
}
