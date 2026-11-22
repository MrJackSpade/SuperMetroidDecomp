using S = SuperMetroid.Core.Enemies.Magdollite.MagdolliteRiseStage;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>The phase definitions of Magdollite's rising body.</summary>
internal static partial class MagdollitePhaseDefinitions
{
    /// <summary>
    /// Maximum whole-pixel rise before Magdollite reaches its apex; whether the arm grows a final phase there is
    /// <see cref="MagdolliteApexGrowth"/>'s.
    /// </summary>
    internal const int MaximumRise = 108;

    /// <summary>How far the arm's body moves down as it grows a phase, and back up as it loses one, in pixels.</summary>
    internal const int BodyShiftPerPhase = 8;

    /// <summary>The stages in the order the arm rises through them; each stage's neighbours are those beside it here.</summary>
    internal static MagdolliteRiseStage[] RiseOrder { get; } =
        [S.Setup, S.OneSegment, S.TwoSegments, S.ThreeSegments, S.FourSegments, S.FiveSegments, S.SixSegments, S.SevenSegments, S.EightSegments];

    /// <summary>
    /// Each stage's distance threshold, body animation and body-to-overlay Y offset (#2343):
    /// the setup shows the one-segment pillar as the first visible stage does; each following sixteen-pixel rise selects
    /// the next pillar and moves the overlay down eight pixels relative to the body.
    /// </summary>
    internal static IReadOnlyDictionary<MagdolliteRiseStage, MagdollitePhaseDefinition> Phases { get; } =
        new Dictionary<MagdolliteRiseStage, MagdollitePhaseDefinition>
        {
            [S.Setup] = new(0, MagdolliteBodySegment.OneSegmentPillar, 12),
            [S.OneSegment] = new(16, MagdolliteBodySegment.OneSegmentPillar, 12),
            [S.TwoSegments] = new(32, MagdolliteBodySegment.TwoSegmentPillar, 20),
            [S.ThreeSegments] = new(48, MagdolliteBodySegment.ThreeSegmentPillar, 28),
            [S.FourSegments] = new(64, MagdolliteBodySegment.FourSegmentPillar, 36),
            [S.FiveSegments] = new(80, MagdolliteBodySegment.FiveSegmentPillar, 44),
            [S.SixSegments] = new(96, MagdolliteBodySegment.SixSegmentPillar, 52),
            [S.SevenSegments] = new(112, MagdolliteBodySegment.SevenSegmentPillar, 60),
            [S.EightSegments] = new(128, MagdolliteBodySegment.EightSegmentPillar, 68),
        };
}
