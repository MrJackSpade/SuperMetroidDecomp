namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The steps Ceres Ridley's colours pass through and the rules that select them (#1413). The colours themselves, and
/// their shapes, are the import's.
/// </summary>
public static class CeresRidleyShadeDefinitions
{
    /// <summary>The black-to-visible body shades, one shown every two Ceres Ridley updates.</summary>
    public const int BodyFadeShadeCount = 16;

    /// <summary>The health band Ceres Ridley shows from his fiftieth to his sixty-ninth accepted hit while fighting.</summary>
    public const int HealthMidBand = 0;
    /// <summary>
    /// The health band Ceres Ridley shows from his seventieth accepted hit on: the game omits the branch after its
    /// ninety-hit comparison, so the middle band is never shown in Ceres.
    /// </summary>
    public const int HealthLateBand = 2;

    /// <summary>The getaway's zoom steps, selected by the whole part of the getaway zoom (zero to eight).</summary>
    public const int GetawayZoomStepCount = 9;
}
