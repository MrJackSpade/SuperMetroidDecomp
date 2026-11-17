using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>
/// The shades the Tourian corpses' pictures are pre-coloured in (#1512): the drained corpses' colours the room's
/// already-dead Sidehopper, Zoomers, Rippers and Skrees lie in, and each step of the Shitroid victim's drain into a
/// corpse. The import colours them from the installed Shitroid target colours and dead-Sidehopper drain colours.
/// </summary>
internal static class TourianCorpseShades
{
    /// <summary>
    /// The colours the Shitroid's room entry loads for its victim Sidehopper, which the Shitroid also draws in: the victim
    /// owns them, and the Shitroid's encounter names the same shade.
    /// </summary>
    internal static PictureShade ShitroidTargets { get; } = new("shitroid-targets", 0);

    /// <summary>A draw in the colours the Shitroid's room entry loads.</summary>
    internal static PictureShading InShitroidTargetColours { get; } = PictureShading.Shaded(ShitroidTargets);

    /// <summary>The drained corpses' colours.</summary>
    internal static PictureShade Drained { get; } = new("tourian-corpse-drained", 0);

    /// <summary>A draw in the drained corpses' colours.</summary>
    internal static PictureShading InDrainedColours { get; } = PictureShading.Shaded(Drained);

    /// <summary>The effect name of the Shitroid victim's drain into a corpse.</summary>
    internal const string SidehopperDrainEffect = "dead-sidehopper-drain";

    /// <summary>
    /// The shade the victim shows at drain stage <paramref name="stage"/> (#2369): one of six draining steps, then the
    /// corpse's paint.
    /// </summary>
    internal static PictureShade SidehopperDrain(SidehopperDrainStage stage) => new(SidehopperDrainEffect, stage.ShadeStep);
}
