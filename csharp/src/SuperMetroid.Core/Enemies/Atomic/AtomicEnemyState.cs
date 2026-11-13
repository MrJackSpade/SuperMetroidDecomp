using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// The state of one Atomic: its two movement selections and its two speeds. Both speeds come whole from the shared
/// linear-speed record; in particular the negative one is the table's own value, not a negation of the positive one.
/// </summary>
public sealed class AtomicEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<AtomicSegment> Playback { get; set; }

    /// <summary>
    /// The vertical movement chosen this update; the selected behaviour changes the actor's Y coordinate.
    /// </summary>
    public AtomicVerticalMovement? VerticalMovement { get; internal set; }

    /// <summary>
    /// The horizontal movement chosen this update; the selected behaviour changes the actor's X coordinate.
    /// </summary>
    public AtomicHorizontalMovement? HorizontalMovement { get; internal set; }

    /// <summary>Signed 16.16 room-pixel speed per AI update for down and right movement, the speed its spawn settings give.</summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>Signed 16.16 room-pixel speed per AI update for up and left movement, the negation of <see cref="Speed"/>.</summary>
    public SignedSixteenSixteen NegativeSpeed { get; internal set; }
}
