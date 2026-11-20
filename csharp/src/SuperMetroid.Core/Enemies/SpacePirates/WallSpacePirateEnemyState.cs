using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of a wall Pirate: the jump its placement chose.
/// </summary>
public sealed class WallSpacePirateEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal WallSpacePirateSpawnSettings Settings { get; }

    /// <summary>Starts the state in <paramref name="action"/> on <paramref name="wall"/>, jumping as <paramref name="jump"/> does.</summary>
    internal WallSpacePirateEnemyState(WallSpacePirateAction action, HorizontalDirection wall, WallSpacePirateJump jump, WallSpacePirateSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Action = action;
        Wall = wall;
        Jump = Ensure.NotNull(jump);
    }

    /// <summary>The wall the Pirate clings to, or, jumping, the wall it jumps to (#2165).</summary>
    public HorizontalDirection Wall { get; internal set; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WallSpacePirateSegment> Playback { get; set; }

    /// <summary>
    /// What the Pirate does each update: the starting wall's action when it is placed, then the one each animation cue
    /// selects as its attack, jump and landing animations play.
    /// </summary>
    public WallSpacePirateAction Action { get; internal set; }

    /// <summary>
    /// The way the Pirate climbs its wall: down at first, reversed by a blocked vertical move, and chosen again by the
    /// climb animation's direction cues (<see cref="SpacePirateDecisions.ClimbsUp"/>).
    /// </summary>
    public WallSpacePirateClimbDirection ClimbDirection { get; internal set; }

    /// <summary>
    /// Whole room-pixel jump-ellipse center, set when a jump starts: X is the departure X plus or minus half the gap
    /// between the walls, and Y the departure Y.
    /// </summary>
    public Position16<RoomPixels> WallJumpArcCenter { get; internal set; }

    /// <summary>
    /// The jump ellipse's angle, which wraps over a whole turn: a quarter turn for a rightward jump or three quarters
    /// for a leftward jump, stepped after each position calculation. A rightward jump steps down through zero to its
    /// endpoint, so the wrap is load-bearing.
    /// </summary>
    public RotationAngle WallJumpArcAngle { get; internal set; }

    /// <summary>
    /// The jump between the walls its placement chose (#2450), slow or fast: the endpoints rightward and leftward, whose
    /// reaching starts that wall's landing, and the steps of the turn its angle turns each update, subtracted jumping
    /// right and added jumping left.
    /// </summary>
    internal WallSpacePirateJump Jump { get; }
}
