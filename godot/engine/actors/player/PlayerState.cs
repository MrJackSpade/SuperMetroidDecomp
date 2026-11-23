using Engine.Actors.Components;

namespace Engine.Actors;

/// <summary>One of the things the player can be doing. The engine provides the standard ones; a game adds its own.</summary>
public abstract class PlayerState : State<Player>
{
    /// <summary>The name of the move values the player stands by.</summary>
    public const string StandingMoves = "standing";

    /// <summary>The name of the move values the player runs by.</summary>
    public const string RunningMoves = "running";

    /// <summary>The name of the move values the player turns around on the ground by.</summary>
    public const string TurningMoves = "turning";

    /// <summary>The name of the move values the player crouches by.</summary>
    public const string CrouchingMoves = "crouching";

    /// <summary>The name of the move values the player steers a jump by.</summary>
    public const string JumpingMoves = "jumping";

    /// <summary>The name of the move values the player steers a fall by.</summary>
    public const string FallingMoves = "falling";

    /// <summary>The name of the move values the player steers a wall jump by.</summary>
    public const string WallJumpingMoves = "wall_jumping";

    /// <summary>The name of the move values the low form moves along the ground by.</summary>
    public const string CrawlingMoves = "crawling";

    /// <summary>The name of the move values the low form steers through the air by.</summary>
    public const string CrawlingInAirMoves = "crawling_in_air";

    /// <summary>The name of the move values the player is thrown by when knocked back.</summary>
    public const string KnockedBackMoves = "knocked_back";

    /// <summary>Whether the player can use weapons in this state.</summary>
    public virtual bool CanAttack => true;

    /// <summary>Whether the player is in control of themselves in this state.</summary>
    public virtual bool InControl => true;
}
