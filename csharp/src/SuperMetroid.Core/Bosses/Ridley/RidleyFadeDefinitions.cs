namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The authored colour runs of the Ridley encounters (#2316), each shown one image at a time by its owner, its images in
/// the order the run shows them (#2409).
/// </summary>
public static class RidleyFadeDefinitions
{
    /// <summary>Both Ridleys' eyes fading in from the darkest shade (its first image) to his own eye colours (its last).</summary>
    public static RidleyFade EyeFade { get; } = new("ridley-eye-fade", RidleyEyeFadeDefinitions.ShadeCount);

    /// <summary>Ridley's body fading in from black (its first image) to his own colours (its last).</summary>
    public static RidleyFade BodyFade { get; } = new("ridley-body-fade", RidleyShadeDefinitions.BodyFadeShadeCount);

    /// <summary>Lower Norfair Ridley's arena revealed as he arrives.</summary>
    public static RidleyFade ArenaReveal { get; } = new("ridley-norfair-reveal", NorfairRidleyRevealDefinitions.RevealStepCount);

    /// <summary>
    /// The self-destruct warning's EMERGENCY title cycling through its alarm colours, the first after the last; the
    /// warning's first step shows the second.
    /// </summary>
    public static RidleyFade SelfDestructAlarm { get; } = new("ridley-alarm", CeresRidleyShadeDefinitions.AlarmStepCount);

    /// <summary>
    /// The getaway floor's shades, one for each whole floor pixel per screen pixel the floor is zoomed to, which the zoom
    /// selects rather than a sequence steps.
    /// </summary>
    public static RidleyFade GetawayFloorZoom { get; } = new("ridley-floor-zoom", CeresRidleyShadeDefinitions.GetawayZoomStepCount);
}
