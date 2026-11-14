using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The state of the Nuclear Waffle, with its projectile and sprite-object links. The seven articulated body links
/// alternate between enemy projectiles and sprite objects.
/// </summary>
public sealed class NuclearWaffleEnemyState
{
    /// <summary>Where the body's typed animation (#641) resumes.</summary>
    internal AnimationPlayback<NuclearWaffleSegment> Playback { get; set; }

    /// <summary>The current waiting or sweeping behaviour.</summary>
    public NuclearWaffleAction? Action { get; internal set; }

    /// <summary>Wrapping wait countdown in enemy AI calls, expiring only after decrement produces a negative value and reloaded from the population reset.</summary>
    public int WaitingTimer { get; internal set; }

    /// <summary>
    /// Orbit radius in room pixels, from the spawn settings. Every link and the head sit on
    /// the circle of this radius around <see cref="Origin"/>.
    /// </summary>
    public int OrbitRadius { get; internal set; }

    /// <summary>Whether the sweep runs forward rather than backward, as the spawn settings' direction says.</summary>
    public bool SweepsForward { get; internal set; }

    /// <summary>
    /// The wait, in AI updates, reloaded into <see cref="WaitingTimer"/> at room load and whenever a wait expires:
    /// the spawn settings' wait time.
    /// </summary>
    public int WaitingTimerReset { get; internal set; }

    /// <summary>
    /// The sweep angle: the whole part is the head's angle in units of 1/256 turn and
    /// the fraction carries between updates. It restarts at <see cref="SweepStartAngle"/> with a zero fraction and
    /// moves monotonically toward <see cref="SweepEndAngle"/> until the last damaging link reaches it, so its whole
    /// part stays between 140 and 500 and never wraps; the orbit lookup uses only its position within the turn.
    /// </summary>
    public SignedSixteenSixteen SweepAngle { get; internal set; }

    /// <summary>The head's current angle in units of 1/256 turn: the whole part of <see cref="SweepAngle"/>.</summary>
    public int CurrentAngle => SweepAngle.WholePixels;

    /// <summary>The start angle: 400 for the backward sweep or 240 for the forward one.</summary>
    public int SweepStartAngle { get; internal set; }
    /// <summary>The finish angle: 240 for the backward sweep or 400 for the forward one; the last damaging link's clamp result determines sweep completion.</summary>
    public int SweepEndAngle { get; internal set; }
    /// <summary>
    /// Signed 1/65536-step angular speed in 1/256-turn units per AI update: the spawn settings' angular speed for a forward
    /// sweep and its negation for a backward one.
    /// </summary>
    public SignedSixteenSixteen AngularSpeed { get; internal set; }
    /// <summary>The arc origin: initial population position in whole room pixels, retained as the fixed orbit center.</summary>
    public Position16<RoomPixels> Origin { get; internal set; }
    /// <summary>The link spawn position: head position after applying the initial angle and radius, used as the whole-pixel spawn position for the four projectile links.</summary>
    public Position16<RoomPixels> InitialHead { get; internal set; }
    /// <summary>Signed same-pool link pitch, -24 backward or +24 forward, subtracted successively from each link angle.</summary>
    public int SegmentSpacing { get; internal set; }
    /// <summary>Signed half-pitch, -12 or +12 angle units, added before the projectile-link loop to stagger it against the sprite-object links.</summary>
    public int InterleavedSegmentOffset { get; internal set; }
    /// <summary>The falling explosion angle: 256 backward or 384 forward; crossing it is the silent second joint turn.</summary>
    public int FallingTurnThreshold { get; internal set; }
    /// <summary>The rising explosion angle: 384 backward or 256 forward; crossing it is the joint turn that queues the turn sound.</summary>
    public int RisingTurnThreshold { get; internal set; }
    /// <summary>Which turn threshold the head last crossed; a change spawns two turn sprite objects at the head's previous position.</summary>
    public NuclearWaffleTurnState HeadOrientationState { get; internal set; }
    /// <summary>The owning enemy's artwork and colours, captured at room load for the persistent body links and transient joint-turn sprite objects.</summary>
    public SpriteGraphics GraphicsAttributes { get; internal set; }

    /// <summary>Four persistent, damaging links.</summary>
    public EnemyProjectile?[] ProjectileSegments { get; } =
        new EnemyProjectile?[4];

    /// <summary>Three persistent, cosmetic links interleaved with the projectiles.</summary>
    public RoomSpriteObject?[] SpriteSegments { get; } =
        new RoomSpriteObject?[3];

    /// <summary>Last threshold state for each damaging link; a state change spawns its joint-turn effect.</summary>
    internal NuclearWaffleTurnState[] ProjectileOrientationStates { get; } = new NuclearWaffleTurnState[4];
    /// <summary>Last threshold state for each cosmetic link, tracked separately from projectile links.</summary>
    internal NuclearWaffleTurnState[] SpriteOrientationStates { get; } = new NuclearWaffleTurnState[3];
}
