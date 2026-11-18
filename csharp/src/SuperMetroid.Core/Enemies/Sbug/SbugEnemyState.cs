using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// The state of an Sbug. The names follow the behaviour the Sbug's movement establishes. It reads its speed, spawn heading,
/// activation distance and activated behaviour from its spawn settings where it uses them, and the straight legs they give
/// it (<see cref="SbugLeg"/>) with them (#2450).
/// </summary>
public sealed class SbugEnemyState
{
    /// <summary>Creates the state of an Sbug starting in <paramref name="action"/>.</summary>
    internal SbugEnemyState(SbugAction action) => Action = action;

    /// <summary>
    /// The long countdown of the random movement that reverses when far from Samus: while it has not passed zero, each
    /// leg may reverse away from her.
    /// </summary>
    public WrappingCountdown StraightMovementTimer { get; internal set; }

    /// <summary>The current AI behaviour.</summary>
    public SbugAction Action { get; internal set; }

    /// <summary>
    /// The custom movement heading, in standard-mathematics orientation (zero right, a quarter turn up). The game
    /// adds random turns to a running total of turns; only its part within one revolution steers, and that part is the
    /// heading. It changes
    /// only through <see cref="FaceCustomHeading"/> and <see cref="TurnCustomHeading"/>.
    /// </summary>
    public RotationAngle CustomHeading { get; private set; }

    /// <summary>Sets the custom heading outright, as the toward- and away-from-Samus activations do.</summary>
    internal void FaceCustomHeading(RotationAngle heading) => CustomHeading = heading;

    /// <summary>
    /// Turns the custom heading on by <paramref name="turn"/>, continuing around the circle.
    /// </summary>
    internal void TurnCustomHeading(RotationAngle turn) =>
        CustomHeading += turn;

    /// <summary>The updates left in the current leg, the one it ends on included (<see cref="SbugMovementDefinitions.LegDuration"/>, #2281).</summary>
    public int MovementTimer { get; internal set; }

    /// <summary>Current horizontal displacement in pixels per AI update, computed from the custom angle and turned to its opposite when the Sbug reverses.</summary>
    public SignedSixteenSixteen CustomXVelocity { get; internal set; }
    /// <summary>Current vertical displacement in pixels per AI update, computed from the custom angle and turned to its opposite when the Sbug reverses.</summary>
    public SignedSixteenSixteen CustomYVelocity { get; internal set; }

    /// <summary>Facing for the custom heading; a reversal folds it into the four facings from right to up-left, as the game does.</summary>
    public CompassDirection CustomFacing { get; internal set; }

    /// <summary>
    /// The animation the enemy plays and where it stands in it (#1600): none before it selects its first direction, then
    /// the loop of the facing it last selected, which selecting that facing again leaves playing (#2145).
    /// </summary>
    internal AnimationPlayback<SbugSegment> Playback { get; set; }
}
