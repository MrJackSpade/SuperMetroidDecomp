using S = SuperMetroid.Core.Enemies.Magdollite.MagdolliteRiseStage;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>The phase definitions of Magdollite's rising body.</summary>
internal static class MagdollitePhaseDefinitions
{
    /// <summary>
    /// Maximum whole-pixel rise before Magdollite reaches its apex; whether the arm grows a final phase there is
    /// <see cref="MagdolliteApexGrowth"/>'s.
    /// </summary>
    internal const int MaximumRise = 108;

    /// <summary>How far the arm's body moves down as it grows a phase, and back up as it loses one, in pixels.</summary>
    internal const int BodyShiftPerPhase = 8;

    /// <summary>
    /// Each stage's distance threshold, body animation, body-to-overlay Y offset and the stages above and below it (#2343):
    /// the setup shows the one-segment pillar as the first visible stage does; each following sixteen-pixel rise selects
    /// the next pillar and moves the overlay down eight pixels relative to the body.
    /// </summary>
    internal static IReadOnlyDictionary<MagdolliteRiseStage, MagdollitePhaseDefinition> Phases { get; } =
        new Dictionary<MagdolliteRiseStage, MagdollitePhaseDefinition>
        {
            [S.Setup] = new(0, MagdolliteSegment.OneSegmentPillar, 12, Higher: S.OneSegment, Lower: null),
            [S.OneSegment] = new(16, MagdolliteSegment.OneSegmentPillar, 12, Higher: S.TwoSegments, Lower: S.Setup),
            [S.TwoSegments] = new(32, MagdolliteSegment.TwoSegmentPillar, 20, Higher: S.ThreeSegments, Lower: S.OneSegment),
            [S.ThreeSegments] = new(48, MagdolliteSegment.ThreeSegmentPillar, 28, Higher: S.FourSegments, Lower: S.TwoSegments),
            [S.FourSegments] = new(64, MagdolliteSegment.FourSegmentPillar, 36, Higher: S.FiveSegments, Lower: S.ThreeSegments),
            [S.FiveSegments] = new(80, MagdolliteSegment.FiveSegmentPillar, 44, Higher: S.SixSegments, Lower: S.FourSegments),
            [S.SixSegments] = new(96, MagdolliteSegment.SixSegmentPillar, 52, Higher: S.SevenSegments, Lower: S.FiveSegments),
            [S.SevenSegments] = new(112, MagdolliteSegment.SevenSegmentPillar, 60, Higher: S.EightSegments, Lower: S.SixSegments),
            [S.EightSegments] = new(128, MagdolliteSegment.EightSegmentPillar, 68, Higher: null, Lower: S.SevenSegments),
        };
}
