using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The state of a Cacatac: its patrol speed, direction and action, plus its two patrol bounds.
/// </summary>
public sealed class CacatacEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal CacatacEnemyState(CacatacAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<CacatacSegment> Playback { get; set; }

    /// <summary>
    /// The distance it patrols each update, in pixels: added to X while it walks right and subtracted while it walks
    /// left.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>
    /// The way it patrols (#2277). A Cacatac placed to stop at first patrols right once its first idle step starts it, the
    /// game's restore-patrol cue sending every placement but left to the right.
    /// </summary>
    public HorizontalDirection PatrolDirection { get; internal set; }

    /// <summary>The current behaviour.</summary>
    public CacatacAction Action { get; internal set; }

    /// <summary>
    /// Left patrol limit's room X. Leftward patrol turns right once the whole X position lies strictly left of it,
    /// measured as the wrapped signed difference, so a limit that wrapped past the room's left edge still turns the
    /// enemy as the game does.
    /// </summary>
    public int LeftPatrolX { get; internal set; }

    /// <summary>
    /// Right patrol limit's room X. Rightward patrol turns left once the whole X position reaches or passes it,
    /// measured as the wrapped signed difference.
    /// </summary>
    public int RightPatrolX { get; internal set; }

    /// <summary>Whether it hangs from a ceiling or stands on a floor, as its spawn settings name it (#2369).</summary>
    internal CacatacMounting Mounting { get; set; }
}
