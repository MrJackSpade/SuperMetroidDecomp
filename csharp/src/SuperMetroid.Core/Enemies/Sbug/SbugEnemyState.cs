using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// The state of an Sbug. The names follow the behaviour the Sbug's movement establishes.
/// </summary>
public sealed class SbugEnemyState
{
    /// <summary>The Sbug's slot, whose population parameters the state reads.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the state for an Sbug's slot.</summary>
    internal SbugEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>The long lifetime used only by behavior four.</summary>
    public WrappingCountdown StraightMovementTimer { get; internal set; }

    /// <summary>The current AI behaviour.</summary>
    public SbugAction? Action { get; internal set; }

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

    /// <summary>The 32-frame segment/wait countdown.</summary>
    public int MovementTimer { get; internal set; }

    /// <summary>The speed the trigonometry tables are multiplied by, as the Sbug's placement names it.</summary>
    public int Speed => _slot.SpawnSettings<SbugSpawnSettings>().Speed;

    /// <summary>The heading the Sbug spawns on: zero points right, a quarter turn up, half a turn left.</summary>
    public RotationAngle InitialAngle => _slot.SpawnSettings<SbugSpawnSettings>().InitialAngle;

    /// <summary>The pixel distance Samus must be within on both axes for activation.</summary>
    public int ActivationRadius => _slot.SpawnSettings<SbugSpawnSettings>().ActivationRadius;

    /// <summary>How the Sbug moves once activated, as its placement names it.</summary>
    public SbugActivationBehavior ActivationBehavior => _slot.SpawnSettings<SbugSpawnSettings>().Behavior;

    /// <summary>Spawn-computed unsigned 1/65536-pixel horizontal magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public UnsignedSixteenSixteen ForwardXVelocity { get; internal set; }
    /// <summary>Spawn-computed unsigned 1/65536-pixel vertical magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public UnsignedSixteenSixteen ForwardYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 1/65536-pixel horizontal displacement for angle <c>InitialAngle - 32</c>, used by the left zigzag leg.</summary>
    public SignedSixteenSixteen LeftXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 1/65536-pixel vertical displacement for angle <c>InitialAngle - 32</c>, used by the left zigzag leg.</summary>
    public SignedSixteenSixteen LeftYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 1/65536-pixel horizontal displacement for angle <c>InitialAngle + 32</c>, used by the right zigzag leg.</summary>
    public SignedSixteenSixteen RightXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 1/65536-pixel vertical displacement for angle <c>InitialAngle + 32</c>, used by the right zigzag leg.</summary>
    public SignedSixteenSixteen RightYVelocity { get; internal set; }
    /// <summary>Current signed 1/65536-pixel horizontal displacement in pixels per AI update, computed from the custom angle and turned to its opposite when the Sbug reverses.</summary>
    public SignedSixteenSixteen CustomXVelocity { get; internal set; }
    /// <summary>Current signed 1/65536-pixel vertical displacement in pixels per AI update, computed from the custom angle and turned to its opposite when the Sbug reverses.</summary>
    public SignedSixteenSixteen CustomYVelocity { get; internal set; }

    /// <summary>Spawn-computed facing for the spawn heading; selects the forward loop.</summary>
    public CompassDirection ForwardFacing { get; internal set; }
    /// <summary>Spawn-computed facing for the spawn heading less an eighth of a turn; used by the left zigzag leg.</summary>
    public CompassDirection LeftFacing { get; internal set; }
    /// <summary>Spawn-computed facing for the spawn heading plus an eighth of a turn; used by the right zigzag leg.</summary>
    public CompassDirection RightFacing { get; internal set; }
    /// <summary>Facing for the custom heading; a reversal folds it into the four facings from right to up-left, as the game does.</summary>
    public CompassDirection CustomFacing { get; internal set; }

    /// <summary>The loop most recently requested by direction-selection code.</summary>
    internal SbugSegment? RequestedAnimation { get; set; }

    /// <summary>The loop already started, or none before the first direction is selected.</summary>
    internal SbugSegment? InstalledAnimation { get; set; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SbugSegment> Playback { get; set; }
}
