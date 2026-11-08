using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The state of a Baby Turtle, with its required extension words.</summary>
public sealed class BabyTurtleEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<MamaTurtleProgram> Program { get; set; }

    /// <summary>Gets the current Baby Turtle AI function pointer.</summary>
    public BabyTurtleAiFunction? Function { get; internal set; }

    /// <summary>
    /// The child's spawn point: its spawn X, which the crawl turns around 48 pixels from, and the top of its spawn
    /// hitbox, which the crawl holds it to.
    /// </summary>
    public Position16<RoomPixels> SpawnTopCenter { get; internal set; }

    /// <summary>Signed whole-pixel velocity per update.</summary>
    public Offset16<RoomPixels> Velocity { get; internal set; }

    /// <summary>Gets the phase-specific Baby Turtle countdown.</summary>
    public int FunctionTimer { get; internal set; }
    /// <summary>Gets the reaction countdown used while the child is not carrying Samus.</summary>
    public int NotCarryingSamusReactionTimer { get; internal set; }
}
