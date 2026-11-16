using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Skree;

namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>The state of one Metaree.</summary>
public sealed class MetareeEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal MetareeEnemyState(MetareeAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SkreeMetareeSegment> Playback { get; set; }

    /// <summary>Reloaded to 21 during flight, then counted down while underground.</summary>
    public int BurrowTimer { get; internal set; }

    /// <summary>The idle, preparing, diving or burrowing behaviour.</summary>
    public MetareeAction Action { get; internal set; }

    /// <summary>Set by the launch crouch's finishing cue.</summary>
    public bool AttackReady { get; internal set; }

    /// <summary>
    /// The dive's downward speed in whole pixels per update, taken once from Samus's height as the dive starts
    /// (<see cref="Compatibility.MetareeDiveSpeed"/>): 4 through 2734.
    /// </summary>
    public int YVelocity { get; internal set; }
}
