using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The state of the Nuclear Waffle, with its projectile and sprite-object links. The seven articulated body links
/// alternate between the two shared native pools, exactly as the original actor does. Its behaviour and wait are
/// values the cartridge keeps in the enemy's common record, so a kill empties them with that record
/// (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class NuclearWaffleEnemyState
{
    /// <summary>Where the body's typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<NuclearWaffleProgram> Program { get; set; }

    /// <summary>The current waiting or sweeping behaviour; none once a kill has emptied the record.</summary>
    public NuclearWaffleEnemyFunction? Function { get; internal set; }

    /// <summary>Wrapping wait countdown in enemy AI calls, expiring only after decrement produces a signed-negative word and reloaded from the population reset byte.</summary>
    public WrappingCountdown WaitingTimer { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: no behaviour and no wait.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        WaitingTimer = WrappingCountdown.Start(0);
    }

    /// <summary>
    /// Orbit radius in room pixels, 0 through 255, from parameter one's high byte. Every link and the head sit on
    /// the circle of this radius around <see cref="Origin"/>.
    /// </summary>
    public int OrbitRadius { get; internal set; }

    /// <summary>Whether the sweep runs forward (parameter two's low byte one) rather than backward (zero).</summary>
    public bool SweepsForward { get; internal set; }

    /// <summary>
    /// The wait, in AI updates, reloaded into <see cref="WaitingTimer"/> at room load and whenever a wait expires:
    /// parameter two's high byte, 0 through 255.
    /// </summary>
    public int WaitingTimerReset { get; internal set; }

    /// <summary>
    /// Native $7E:8000/$8002 sweep angle accumulator: the whole part is the head's angle in units of 1/256 turn and
    /// the fraction carries between updates. It restarts at <see cref="SweepStartAngle"/> with a zero fraction and
    /// moves monotonically toward <see cref="SweepEndAngle"/> until the last damaging link reaches it, so its whole
    /// part stays between 140 and 500 and never wraps; the orbit lookup uses only its position within the turn.
    /// </summary>
    public SignedSixteenSixteen SweepAngle { get; internal set; }

    /// <summary>The head's current angle in units of 1/256 turn: the whole part of <see cref="SweepAngle"/>.</summary>
    public int CurrentAngle => SweepAngle.WholePixels;

    /// <summary>Native $7E:8006 startAngle: 400 for the backward sweep or 240 for the forward one.</summary>
    public int SweepStartAngle { get; internal set; }
    /// <summary>Native $7E:801C finishAngle: 240 for the backward sweep or 400 for the forward one; the last damaging link's clamp result determines sweep completion.</summary>
    public int SweepEndAngle { get; internal set; }
    /// <summary>
    /// Native $7E:8008/$800A signed 16.16 angular speed in 1/256-turn units per AI update, from the common linear speed
    /// table: the positive record for a forward sweep and the separately stored negative record for a backward one.
    /// </summary>
    public SignedSixteenSixteen AngularSpeed { get; internal set; }
    /// <summary>Native $7E:800C/$800E arcOriginX/YPosition: initial population position in whole room pixels, retained as the fixed orbit center.</summary>
    public Position16<RoomPixels> Origin { get; internal set; }
    /// <summary>Native $7E:8010/$8012 bodyPartSpawnX/YPosition: head position after applying the initial angle and radius, used as the whole-pixel spawn position for the four projectile links.</summary>
    public Position16<RoomPixels> InitialHead { get; internal set; }
    /// <summary>Native $7E:8016 angleBetweenBodyPartsTimes2: signed same-pool link pitch, -24 backward or +24 forward, subtracted successively from each link angle.</summary>
    public int SegmentSpacing { get; internal set; }
    /// <summary>Native $7E:8018 angleBetweenBodyParts: signed half-pitch, -12 or +12 angle units, added before the projectile-link loop to stagger it against the sprite-object links.</summary>
    public int InterleavedSegmentOffset { get; internal set; }
    /// <summary>Native $7E:801E fallingExplosionAngle: 256 backward or 384 forward; crossing it is the silent second joint turn.</summary>
    public int FirstTurnThreshold { get; internal set; }
    /// <summary>Native $7E:8020 risingExplosionAngle: 384 backward or 256 forward; crossing it is the joint turn that queues the turn sound.</summary>
    public int SecondTurnThreshold { get; internal set; }
    /// <summary>Which turn threshold the head last crossed; a change spawns two turn sprite objects at the head's previous position.</summary>
    public NuclearWaffleTurnState HeadOrientationState { get; internal set; }
    /// <summary>The owning enemy's sheet characters and palette row, captured at room load for the persistent body links and transient joint-turn sprite objects.</summary>
    public SpriteGraphics GraphicsAttributes { get; internal set; }

    /// <summary>Four persistent, damaging bank-$86 links.</summary>
    public RoomEnemyProjectileSlot?[] ProjectileSegments { get; } =
        new RoomEnemyProjectileSlot?[4];

    /// <summary>Three persistent, cosmetic bank-$B4 links interleaved with the projectiles.</summary>
    public RoomSpriteObjectSlot?[] SpriteSegments { get; } =
        new RoomSpriteObjectSlot?[3];

    /// <summary>Last threshold state for each damaging link; a state change spawns its joint-turn effect.</summary>
    internal NuclearWaffleTurnState[] ProjectileOrientationStates { get; } = new NuclearWaffleTurnState[4];
    /// <summary>Last threshold state for each cosmetic link, tracked separately from projectile links.</summary>
    internal NuclearWaffleTurnState[] SpriteOrientationStates { get; } = new NuclearWaffleTurnState[3];
}
