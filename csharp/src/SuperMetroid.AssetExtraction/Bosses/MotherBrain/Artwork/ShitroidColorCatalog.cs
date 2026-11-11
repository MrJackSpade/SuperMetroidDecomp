using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The installed live-Shitroid RGB5 images, kept as installed; timer and fade destinations stay compiled.</summary>
public sealed class ShitroidColorCatalog
{
    /// <summary>The installed normal pulse phases.</summary>
    private readonly InstalledColourRows normal;
    /// <summary>Each initialization target's installed colours.</summary>
    private readonly Dictionary<ShitroidColorTarget, InstalledColourRows> targets;

    /// <summary>Keeps the complete installed pulse and initialization targets.</summary>
    /// <param name="normal">Each normal pulse phase's colours.</param>
    /// <param name="sidehopper">The Sidehopper target's colours.</param>
    /// <param name="shitroid">The Shitroid target's colours, including its transparent slot.</param>
    /// <param name="deadSidehopper">The dead Sidehopper target's colours.</param>
    /// <exception cref="InvalidDataException">A phase, a target or a colour is missing.</exception>
    internal ShitroidColorCatalog(RgbColor[][] normal, RgbColor[] sidehopper, RgbColor[] shitroid, RgbColor[] deadSidehopper)
    {
        this.normal = new InstalledColourRows(normal, ShitroidColorDefinitions.NormalFrameCount,
            ShitroidColorDefinitions.NormalColorsPerFrame, "Shitroid normal pulse");
        targets = new()
        {
            [ShitroidColorTarget.Sidehopper] = new(sidehopper, ShitroidColorDefinitions.TargetColorCount, "Shitroid sidehopper target"),
            [ShitroidColorTarget.Shitroid] = new(shitroid, ShitroidColorDefinitions.TargetColorCount, "Shitroid target"),
            [ShitroidColorTarget.DeadSidehopper] = new(deadSidehopper, ShitroidColorDefinitions.TargetColorCount, "Shitroid dead sidehopper target"),
        };
    }

    /// <summary>Gets an installed packed RGB5 ink from normal pulse phase 0–7 and ink 0–3, written to colours 165–168; phase cadence and cries remain actor behavior.</summary>
    public RgbColor NormalColor(int frame, int color) => normal.Colour(frame, color);

    /// <summary>Gets installed packed RGB5 color 0–15 from a selected initialization target, including its transparent-slot value; the actor copies targets to its palette image and current colours during setup.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The target is undefined or the colour is outside it.</exception>
    public RgbColor TargetColor(ShitroidColorTarget target, int color) => targets.TryGetValue(target, out InstalledColourRows? colours)
        ? colours.Colour(color)
        : throw new ArgumentOutOfRangeException(nameof(target));
}
