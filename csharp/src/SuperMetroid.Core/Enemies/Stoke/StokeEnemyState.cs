using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>The state of one Stoke: its walking speeds, facing and behaviour.</summary>
public sealed class StokeEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<StokeSegment> Playback { get; set; }

    /// <summary>Signed 16.16 room-pixel displacement per walking update to the right, from the positive half of the population's linear-speed record.</summary>
    public SignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>Signed 16.16 room-pixel displacement per walking update to the left, from the negative half of the population's linear-speed record.</summary>
    public SignedSixteenSixteen LeftVelocity { get; internal set; }

    /// <summary>Facing latch changed by animation cues, one update after a turn request; initialized from its spawn direction.</summary>
    public StokeDirection Direction { get; internal set; }

    /// <summary>The current walking or attacking behaviour.</summary>
    public StokeAiFunction? Function { get; internal set; }
}
