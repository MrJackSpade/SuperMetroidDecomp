using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>
/// The shades the Tourian corpses' pictures are pre-coloured in (#1512): the drained corpses' colours the room's
/// already-dead Sidehopper, Zoomers, Rippers and Skrees lie in, and each step of the Shitroid victim's drain into a
/// corpse. The import colours them from the installed Shitroid target colours and dead-Sidehopper drain colours.
/// </summary>
internal static class TourianCorpseShades
{
    /// <summary>The drained corpses' colours.</summary>
    internal static PictureShade Drained { get; } = new("tourian-corpse-drained", 0);

    /// <summary>A draw in the drained corpses' colours.</summary>
    internal static PictureShading InDrainedColours { get; } = PictureShading.Shaded(Drained);

    /// <summary>The effect name of the Shitroid victim's drain into a corpse.</summary>
    internal const string SidehopperDrainEffect = "dead-sidehopper-drain";

    /// <summary>The drain's steps: six draining stages, then the corpse's paint.</summary>
    internal const int SidehopperDrainSteps = 7;

    /// <summary>The shade of drain step <paramref name="step"/>.</summary>
    internal static PictureShade SidehopperDrain(int step) =>
        new(SidehopperDrainEffect, Ensure.BetweenInclusive(0, SidehopperDrainSteps - 1, step));
}
