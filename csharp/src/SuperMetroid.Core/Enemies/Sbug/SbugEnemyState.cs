using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// The state of an Sbug. The names follow the behaviour the Sbug's movement establishes.
/// </summary>
public sealed class SbugEnemyState
{
    /// <summary>Creates the state of an Sbug placed with <paramref name="settings"/>, starting in <paramref name="action"/>.</summary>
    internal SbugEnemyState(SbugSpawnSettings settings, SbugAction action)
    {
        Ensure.NotNull(settings);
        Speed = settings.Speed;
        InitialAngle = settings.InitialAngle;
        ActivationRadius = settings.ActivationRadius;
        ActivationBehavior = settings.Behavior;
        Action = action;
    }

    /// <summary>The current AI behaviour.</summary>
    public SbugAction Action { get; internal set; }

    /// <summary>The updates left in a forward-then-wait leg, the one it ends on included (<see cref="SbugMovementDefinitions.LegDuration"/>, #2281).</summary>
    public int MovementTimer { get; internal set; }

    /// <summary>The speed the heading's sine and cosine scale, as the Sbug's placement names it.</summary>
    public int Speed { get; }

    /// <summary>The heading the Sbug spawns on: zero points right, a quarter turn up, half a turn left.</summary>
    public RotationAngle InitialAngle { get; }

    /// <summary>The pixel distance Samus must be within on both axes for activation.</summary>
    public int ActivationRadius { get; }

    /// <summary>How the Sbug moves once activated, as its placement names it.</summary>
    public SbugActivationBehavior ActivationBehavior { get; }

    /// <summary>Spawn-computed horizontal displacement in pixels per AI update along the spawn heading, used by forward movement.</summary>
    public SignedSixteenSixteen ForwardXVelocity { get; internal set; }
    /// <summary>Spawn-computed vertical displacement in pixels per AI update along the spawn heading, used by forward movement.</summary>
    public SignedSixteenSixteen ForwardYVelocity { get; internal set; }
    /// <summary>Spawn-computed horizontal displacement for angle <c>InitialAngle - 32</c>, used by the left zigzag leg.</summary>
    public SignedSixteenSixteen LeftXVelocity { get; internal set; }
    /// <summary>Spawn-computed vertical displacement for angle <c>InitialAngle - 32</c>, used by the left zigzag leg.</summary>
    public SignedSixteenSixteen LeftYVelocity { get; internal set; }
    /// <summary>Spawn-computed horizontal displacement for angle <c>InitialAngle + 32</c>, used by the right zigzag leg.</summary>
    public SignedSixteenSixteen RightXVelocity { get; internal set; }
    /// <summary>Spawn-computed vertical displacement for angle <c>InitialAngle + 32</c>, used by the right zigzag leg.</summary>
    public SignedSixteenSixteen RightYVelocity { get; internal set; }

    /// <summary>Spawn-computed facing for the spawn heading; selects the forward loop.</summary>
    public CompassDirection ForwardFacing { get; internal set; }
    /// <summary>Spawn-computed facing for the spawn heading less an eighth of a turn; used by the left zigzag leg.</summary>
    public CompassDirection LeftFacing { get; internal set; }
    /// <summary>Spawn-computed facing for the spawn heading plus an eighth of a turn; used by the right zigzag leg.</summary>
    public CompassDirection RightFacing { get; internal set; }

    /// <summary>
    /// The animation the enemy plays and where it stands in it (#1600): none before it selects its first direction, then
    /// the loop of the facing it last selected, which selecting that facing again leaves playing (#2145).
    /// </summary>
    internal AnimationPlayback<SbugSegment> Playback { get; set; }
}
