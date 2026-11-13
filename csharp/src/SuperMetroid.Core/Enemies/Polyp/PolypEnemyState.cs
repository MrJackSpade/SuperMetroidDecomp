using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>The state of the Polyp.</summary>
public sealed class PolypEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PolypSegment> Playback { get; set; }

    /// <summary>The current Polyp behaviour.</summary>
    public PolypEnemyFunction? Function { get; internal set; }

    /// <summary>The launch cooldown, decremented through -1 before the Polyp rearms.</summary>
    public int CooldownTimer { get; internal set; }
}
