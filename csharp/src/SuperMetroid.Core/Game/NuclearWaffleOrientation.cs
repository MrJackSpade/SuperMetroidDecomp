using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 dispatcher word stored in Nuclear Waffle variable A. “Nuclear Waffle”
/// is the disassembly's internal name; the cartridge name table calls definition $E0BF
/// Puromi.
/// </summary>
public enum NuclearWaffleEnemyFunction : ushort
{
    /// <summary>$A6:9615, Function_Puromi_Inactive: decrements the signed-expiry wait timer, then resets the sweep and joint-orientation latches and enables offscreen processing.</summary>
    Waiting = 0x9615,
    /// <summary>$A6:9682, Function_Puromi_Active: positions the head and seven interleaved links, advances angular speed, and returns to waiting when the final projectile link reaches the endpoint.</summary>
    Sweeping = 0x9682,
}

/// <summary>One orientation result returned by native helper <c>$A6:98E7</c>.</summary>
/// <param name="TurnSpriteVariant">Turn-effect selector 0 for the clockwise sprite-object program or 1 for its counterclockwise counterpart; direction and crossed threshold choose it.</param>
/// <param name="State">Native explosion reason 0 before a joint turn, 1 at the rising threshold, or 2 at the falling threshold; transitions spawn overlays, with state 2 suppressing the turn sound.</param>
public readonly record struct NuclearWaffleOrientation(
    ushort TurnSpriteVariant,
    ushort State);

/// <summary>
/// Debugger-facing projection of Nuclear Waffle's base, extra-RAM, projectile, and sprite-
/// object state. The seven articulated body links alternate between the two shared native
/// pools, exactly as the original actor does.
/// </summary>
public sealed class NuclearWaffleEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new nuclear waffle enemy state instance.</summary>
    internal NuclearWaffleEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native variable A ($0FA8 plus slot byte index): current bank-$A6 inactive/active dispatcher pointer.</summary>
    public NuclearWaffleEnemyFunction Function
    {
        get => (NuclearWaffleEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Native variable B: wrapping wait countdown in enemy AI calls, expiring only after decrement produces a signed-negative word and reloaded from the population reset byte.</summary>
    public ushort WaitingTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
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

    /// <summary>Gets projectile orientation states.</summary>
    internal ushort[] ProjectileOrientationStates { get; } = new ushort[4];
    /// <summary>Gets sprite orientation states.</summary>
    internal ushort[] SpriteOrientationStates { get; } = new ushort[3];
}
