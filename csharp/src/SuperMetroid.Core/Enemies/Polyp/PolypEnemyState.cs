using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>The state of the Polyp.</summary>
public sealed class PolypEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal PolypEnemyState(PolypAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PolypSegment> Playback { get; set; }

    /// <summary>The current Polyp behaviour.</summary>
    public PolypAction Action { get; internal set; }

    /// <summary>The updates left in the launch cooldown, the one the Polyp rearms on included (#2281).</summary>
    public int CooldownTimer { get; internal set; }
}
