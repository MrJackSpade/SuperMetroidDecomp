using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The state of a Baby Turtle, with its required extra values.</summary>
public sealed class BabyTurtleEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MamaTurtleSegment> Playback { get; set; }

    /// <summary>Gets the Baby Turtle's current action.</summary>
    public BabyTurtleAction? Action { get; internal set; }

    /// <summary>
    /// The child's spawn point: its spawn X, which the crawl turns around 48 pixels from, and the top of its spawn
    /// hitbox, which the crawl holds it to.
    /// </summary>
    public Position16<RoomPixels> SpawnTopCenter { get; internal set; }

    /// <summary>Signed whole-pixel velocity per update.</summary>
    public Offset16<RoomPixels> Velocity { get; internal set; }

    /// <summary>Gets the phase-specific Baby Turtle countdown.</summary>
    public int ActionTimer { get; internal set; }
    /// <summary>Gets the reaction countdown used while the child is not carrying Samus.</summary>
    public int NotCarryingSamusReactionTimer { get; internal set; }
}
