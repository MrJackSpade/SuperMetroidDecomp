namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The authored colour runs of the Ridley encounters (#2316), each shown one image at a time by its owner.</summary>
public static class RidleyFadeDefinitions
{
    /// <summary>Ceres Ridley's eyes fading in: its last image is the darkest, which the fade shows first.</summary>
    public static RidleyFade EyeFade { get; } = new("ridley-eye-fade", RidleyEyeFadeDefinitions.ShadeCount);

    /// <summary>Ridley's body fading in from black (its first image) to his own colours (its last).</summary>
    public static RidleyFade BodyFade { get; } = new("ridley-body-fade", RidleyShadeDefinitions.BodyFadeShadeCount);

    /// <summary>Lower Norfair Ridley's arena revealed as he arrives.</summary>
    public static RidleyFade ArenaReveal { get; } = new("ridley-norfair-reveal", NorfairRidleyRevealDefinitions.RevealStepCount);

    /// <summary>The self-destruct warning's EMERGENCY title cycling through its alarm colours, the first after the last.</summary>
    public static RidleyFade SelfDestructAlarm { get; } = new("ridley-alarm", CeresRidleyShadeDefinitions.AlarmStepCount);

    /// <summary>The getaway floor's shades, one for each whole floor pixel per screen pixel the floor is zoomed to.</summary>
    public static RidleyFade GetawayFloorZoom { get; } = new("ridley-floor-zoom", CeresRidleyShadeDefinitions.GetawayZoomStepCount);
}
