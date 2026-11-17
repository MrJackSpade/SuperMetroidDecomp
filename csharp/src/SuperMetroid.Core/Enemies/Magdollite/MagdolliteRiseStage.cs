namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// How far a Magdollite arm has grown (#2044, #2343): the zero-distance setup a dormant arm shows, then its pillar of one
/// through eight segments. Each stage names the stage above and below it (<see cref="MagdolliteRiseStages"/>); the arm
/// grows a stage as it rises past the next stage's threshold and loses one as it sinks back below its own; whether it
/// grows a final stage at full height is <see cref="MagdolliteApexGrowth"/>'s. Its values are arbitrary.
/// </summary>
public enum MagdolliteRiseStage
{
    /// <summary>The zero-distance setup a dormant arm shows, the default stage; it shows the one-segment pillar.</summary>
    Setup,
    /// <summary>The one-segment pillar a waking arm starts its rise at.</summary>
    OneSegment,
    /// <summary>The two-segment pillar.</summary>
    TwoSegments,
    /// <summary>The three-segment pillar.</summary>
    ThreeSegments,
    /// <summary>The four-segment pillar.</summary>
    FourSegments,
    /// <summary>The five-segment pillar.</summary>
    FiveSegments,
    /// <summary>The six-segment pillar.</summary>
    SixSegments,
    /// <summary>The seven-segment pillar.</summary>
    SevenSegments,
    /// <summary>The eight-segment pillar, the arm's last stage.</summary>
    EightSegments,
}
