using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The state of a Cacatac: its patrol speed, direction and action, plus its two patrol bounds.
/// </summary>
public sealed class CacatacEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<CacatacSegment> Playback { get; set; }

    /// <summary>
    /// The distance it patrols each update, in pixels: added to X while it walks right and subtracted while it walks
    /// left.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>The way it patrols: left, right, or, placed to stop at first, still until its first idle step.</summary>
    public CacatacDirection Direction { get; internal set; }

    /// <summary>The current behaviour.</summary>
    public CacatacAction? Action { get; internal set; }

    /// <summary>
    /// Left patrol limit; only X is meaningful. Leftward patrol turns right once the whole X position lies strictly
    /// left of it, measured as the wrapped signed difference, so a limit that wrapped past the room's left edge still
    /// turns the enemy as the game does.
    /// </summary>
    public Position16<RoomPixels> LeftPatrolLimit { get; internal set; }

    /// <summary>
    /// Right patrol limit; only X is meaningful. Rightward patrol turns left once the whole X position reaches or
    /// passes it, measured as the wrapped signed difference.
    /// </summary>
    public Position16<RoomPixels> RightPatrolLimit { get; internal set; }

    /// <summary>
    /// Whether it hangs from a ceiling or stands on a floor, as its spawn mounting names. The game tests only zero versus nonzero, so this is a
    /// Boolean discriminator rather than a guessed flags enum.
    /// </summary>
    public bool UpsideUp { get; internal set; }
}
