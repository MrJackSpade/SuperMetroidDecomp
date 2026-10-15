using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Debugger-facing projection of Nuclear Waffle's base, extra-RAM, projectile, and sprite-
/// object state. The seven articulated body links alternate between the two shared native
/// pools, exactly as the original actor does.
/// </summary>
public sealed class NuclearWaffleEnemyState
{
    /// <summary>Underlying population slot that owns the native dispatcher and wait words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches the articulated actor state to its room enemy slot.</summary>
    internal NuclearWaffleEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the body's typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<NuclearWaffleProgram> Program { get; set; }

    /// <summary>Native variable A ($0FA8 plus slot byte index): current bank-$A6 inactive/active dispatcher pointer.</summary>
    public NuclearWaffleEnemyFunction Function
    {
        get => (NuclearWaffleEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Native variable B: wrapping wait countdown in enemy AI calls, expiring only after decrement produces a signed-negative word and reloaded from the population reset byte.</summary>
    public NativeCountdown WaitingTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableB);
        internal set => _slot.VariableB = value.RawValue;
    }

    /// <summary>Low byte of population parameter one.</summary>
    public byte AngularSpeedIndex { get; internal set; }

    /// <summary>High byte of population parameter one, in pixels.</summary>
    public byte OrbitRadius { get; internal set; }

    /// <summary>Low byte of population parameter two; zero sweeps backward.</summary>
    public byte Direction { get; internal set; }

    /// <summary>High byte of population parameter two.</summary>
    public byte WaitingTimerReset { get; internal set; }

    /// <summary>Native $7E:8000 subAngle word: low fractional part of the sweep's signed 16.16 angle-unit accumulator, reset when the wait expires.</summary>
    public ushort AngleSubposition { get; internal set; }
    /// <summary>Native $7E:8002 angle word: whole sweep units, with 256 units per turn; full-word comparisons control clamping and completion while position lookup wraps its low byte.</summary>
    public ushort CurrentAngle { get; internal set; }
    /// <summary>Native $7E:8006 startAngle: $0190 for reverse direction 0 or $00F0 for forward direction 1, restored at the start of each sweep.</summary>
    public ushort SweepStartAngle { get; internal set; }
    /// <summary>Native $7E:801C finishAngle: $00F0 for direction 0 or $0190 for direction 1; the last damaging link's clamp result determines sweep completion.</summary>
    public ushort SweepEndAngle { get; internal set; }
    /// <summary>Native $7E:8008 subAngleDelta: fractional low word of angular speed in 16.16 table-angle units per AI update, selected from the independently stored positive or negative speed record.</summary>
    public ushort AngularSpeedFraction { get; internal set; }
    /// <summary>Native $7E:800A angleDelta: signed whole-angle-unit high word added to the current angle with fractional carry; one unit is 1/256 turn.</summary>
    public short AngularSpeedWhole { get; internal set; }
    /// <summary>Native $7E:800C/$800E arcOriginX/YPosition: initial population position in whole room pixels, retained as the fixed orbit center.</summary>
    public Position16<RoomPixels> Origin { get; internal set; }
    /// <summary>Native $7E:8010/$8012 bodyPartSpawnX/YPosition: head position after applying the initial angle and radius, used as the whole-pixel spawn position for the four projectile links.</summary>
    public Position16<RoomPixels> InitialHead { get; internal set; }
    /// <summary>Native $7E:8016 angleBetweenBodyPartsTimes2: signed same-pool link pitch, -24 for direction 0 or +24 for direction 1, subtracted successively from each link angle.</summary>
    public short SegmentSpacing { get; internal set; }
    /// <summary>Native $7E:8018 angleBetweenBodyParts: signed half-pitch, -12 or +12 angle units, added before the projectile-link loop to stagger it against the sprite-object links.</summary>
    public short InterleavedSegmentOffset { get; internal set; }
    /// <summary>Native $7E:801E fallingExplosionAngle: threshold $0100 for direction 0 or $0180 for direction 1, producing orientation state 2 and the silent second joint-turn effect.</summary>
    public ushort FirstTurnThreshold { get; internal set; }
    /// <summary>Native $7E:8020 risingExplosionAngle: threshold $0180 for direction 0 or $0100 for direction 1, producing orientation state 1 and the library-two $5E joint-turn sound.</summary>
    public ushort SecondTurnThreshold { get; internal set; }
    /// <summary>Native headExplosionReason latch, 0/1/2 from the angle-threshold helper; a change spawns two turn sprite objects at the head's previous position.</summary>
    public ushort HeadOrientationState { get; internal set; }
    /// <summary>Native graphicsIndices word combining the owning enemy's VRAM tile-base and OBJ palette bits for persistent body links and transient joint-turn sprite objects.</summary>
    public ushort GraphicsIndex { get; internal set; }

    /// <summary>Four persistent, damaging bank-$86 links.</summary>
    public RoomEnemyProjectileSlot?[] ProjectileSegments { get; } =
        new RoomEnemyProjectileSlot?[4];

    /// <summary>Three persistent, cosmetic bank-$B4 links interleaved with the projectiles.</summary>
    public RoomSpriteObjectSlot?[] SpriteSegments { get; } =
        new RoomSpriteObjectSlot?[3];

    /// <summary>Last threshold state for each damaging link; a state change spawns its joint-turn effect.</summary>
    internal ushort[] ProjectileOrientationStates { get; } = new ushort[4];
    /// <summary>Last threshold state for each cosmetic link, tracked separately from projectile links.</summary>
    internal ushort[] SpriteOrientationStates { get; } = new ushort[3];
}
