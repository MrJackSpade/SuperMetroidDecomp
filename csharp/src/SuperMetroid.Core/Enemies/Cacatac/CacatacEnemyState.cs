using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The state of a Cacatac: its patrol direction and action, plus its two patrol bounds. It walks at its spawn settings'
/// speed and hangs or stands as they mount it, which it reads where it uses them (#2476).
/// </summary>
public sealed class CacatacEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal CacatacEnemyState(CacatacAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<CacatacSegment> Playback { get; set; }

    /// <summary>
    /// The way it patrols (#2277). A Cacatac placed to stop at first patrols right once its first idle step starts it, the
    /// game's restore-patrol cue sending every placement but left to the right.
    /// </summary>
    public HorizontalDirection PatrolDirection { get; internal set; }

    /// <summary>The current behaviour.</summary>
    public CacatacAction Action { get; internal set; }

    /// <summary>
    /// Left patrol limit's room X. Leftward patrol turns right once the whole X position lies strictly left of it. Every
    /// placement's limit lies inside the room.
    /// </summary>
    public int LeftPatrolX { get; internal set; }

    /// <summary>
    /// Right patrol limit's room X. Rightward patrol turns left once the whole X position reaches or passes it.
    /// </summary>
    public int RightPatrolX { get; internal set; }
}
