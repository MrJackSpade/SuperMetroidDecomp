using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The state of a Baby Turtle.</summary>
public sealed partial class BabyTurtleEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal BabyTurtleEnemyState(BabyTurtleAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BabyTurtleSegment> Playback { get; set; }

    /// <summary>Gets the Baby Turtle's current action.</summary>
    public BabyTurtleAction Action { get; internal set; }

    /// <summary>
    /// The child's spawn point: its spawn X, which the crawl turns around its travel distance from, and the top of its spawn
    /// hitbox, which the crawl holds it to.
    /// </summary>
    public Position16<RoomPixels> SpawnTopCenter { get; internal set; }

    /// <summary>
    /// The way the baby crawls or spins (#2338): its placement's at first, turned at the end of its crawl, by Samus's
    /// touch and by a wall it spins into, and Samus's facing when she spins it off.
    /// </summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>
    /// How fast the baby moves along <see cref="Heading"/>, in whole pixels an update (#2338): its crawl speed as it crawls, and
    /// a spin's speed from a spin until its crawl next turns it.
    /// </summary>
    public int HorizontalSpeed { get; internal set; }

    /// <summary>The downward speed of a spin in whole pixels an update.</summary>
    public int SpinFallSpeed { get; internal set; }

    /// <summary>
    /// Signed whole-pixel velocity per update: <see cref="HorizontalSpeed"/> toward <see cref="Heading"/>, and
    /// <see cref="SpinFallSpeed"/> downward.
    /// </summary>
    public Offset16<RoomPixels> Velocity => new(HorizontalSpeed.Toward(Heading), SpinFallSpeed);

    /// <summary>Gets the phase-specific Baby Turtle countdown.</summary>
    public UpdateCountdown ActionTimer { get; internal set; } = UpdateCountdown.Ended();
    /// <summary>Gets the reaction countdown used while the child is not carrying Samus.</summary>
    public UpdateCountdown NotCarryingSamusReactionTimer { get; internal set; } = UpdateCountdown.Ended();
}
