using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The state of Oum.</summary>
public sealed class MaridiaLargeSnailEnemyState
{
    /// <summary>Starts the snail in <paramref name="action"/>, its bounce in <paramref name="bouncePhase"/>.</summary>
    internal MaridiaLargeSnailEnemyState(MaridiaLargeSnailAction action, MaridiaLargeSnailBouncePhase bouncePhase)
    {
        Action = action;
        BouncePhase = bouncePhase;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MaridiaLargeSnailSegment> Playback { get; set; }

    /// <summary>
    /// The way Oum faces: toward Samus while idle, the way it moves when it attacks, and as it faced then while it rolls
    /// (#2145, #2338). It spawns facing right.
    /// </summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>
    /// Whether Oum's roll runs backwards: a roll from idle or after an attack runs forwards, and each wall it meets
    /// reverses it (#2145).
    /// </summary>
    public bool RollsBackwards { get; internal set; }

    /// <summary>
    /// Whether Oum still shows the left-facing idle it spawns with while it counts as facing right (#2145): until it first
    /// starts an animation, asking for the right-facing idle keeps that left-facing one, and asking for the left-facing
    /// idle starts it afresh.
    /// </summary>
    internal bool ShowsSpawnIdle { get; set; }

    /// <summary>
    /// Set only during the rolling frames where Oum may turn into its next
    /// attack; it says nothing of Oum's hitboxes.
    /// </summary>
    public bool AttackAllowsRotation { get; internal set; }

    /// <summary>
    /// Whether the snail stops moving on its next update: set after its update and read by the next, as the game
    /// stops it one update later.
    /// </summary>
    public bool StopMovementNextFrame { get; internal set; }

    /// <summary>The outer action.</summary>
    public MaridiaLargeSnailAction Action { get; internal set; }

    /// <summary>
    /// The bounce arc's place along the quadratic speed curve, stepped a mark and a half each update. Falling holds it at
    /// mark 64; an impact takes 16 marks off and may leave it below the start, which ends the bounces, and rising holds
    /// it at the start once it would go below.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }

    /// <summary>
    /// The rolling updates left before Oum may attack, the one it may first attack on included (#2340): it starts at
    /// <see cref="MaridiaLargeSnailMotionDefinitions.InitialAttackCooldown"/> and each attack restarts it; once run out it
    /// stays at zero until Oum attacks.
    /// </summary>
    public UpdateCountdown AttackCooldown { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The way Oum rolls (#2338): toward Samus when it attacks, turned by each wall it meets, and leftward once Samus is
    /// to its left while it idles.
    /// </summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>Remaining landing impacts before Oum is settled.</summary>
    public int RemainingBounces { get; internal set; }

    /// <summary>Falling/rising half of the quadratic bounce.</summary>
    public MaridiaLargeSnailBouncePhase BouncePhase { get; internal set; }
}
