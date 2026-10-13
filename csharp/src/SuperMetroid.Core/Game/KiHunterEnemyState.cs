using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 function pointers stored in Ki-Hunter variable A. Keeping the ROM
/// addresses as enum values makes a debugger watch directly comparable with enemy WRAM.
/// </summary>
public enum KiHunterEnemyFunction : ushort
{
    /// <summary><c>$A8:F268 Function_Kihunter_Winged_IdleFlying</c>: patrols within the spawn-height band, reflects movement on collision, and starts a swoop toward nearby Samus below.</summary>
    FlyingPatrol = 0xf268,
    /// <summary><c>$A8:F3B8 Function_Kihunter_Winged_Swoop</c>: follows an accelerating elliptic dive, changing art at the swipe angle and backing off on collision.</summary>
    Swooping = 0xf3b8,
    /// <summary><c>$A8:F4ED Function_Kihunter_Winged_BackOff</c>: moves upward away from a blocked dive until collision or passage above the spawn Y restores patrol.</summary>
    RecoveringFromSwoop = 0xf4ed,
    /// <summary><c>$A8:F55A Function_Kihunter_Wingless_InitialFalling</c>: applies gravity after clipping wings or a ground-bound spawn, then prepares a hop on landing.</summary>
    FallingAfterWingLoss = 0xf55a,
    /// <summary><c>$A8:F58B Function_Kihunter_Wingless_PrepareToHop</c>: aims a two-pixel-per-frame hop toward Samus with an RNG-selected initial upward speed of seven or eight pixels per frame.</summary>
    StartGroundJump = 0xf58b,
    /// <summary><c>$A8:F5E3 RTL_A8F5E3</c>: leaves movement to the jump, landing, or spit animation until its instruction stream selects the next function.</summary>
    NoOp = 0xf5e3,
    /// <summary><c>$A8:F5F0 Function_Kihunter_Wingless_Hop</c>: advances the collision-bearing ballistic hop and starts the landing animation after downward collision.</summary>
    GroundJump = 0xf5f0,
    /// <summary><c>$A8:F68B Function_Kihunter_Wingless_Thinking</c>: counts down the post-landing or post-spit wait, then spits within 96 horizontal pixels or prepares another hop.</summary>
    GroundWait = 0xf68b,
    /// <summary><c>$A8:F6B3 Function_Kihunter_Wingless_FireAcidSpit</c>: installs the acid-spit list facing Samus and suspends movement while bytecode owns the attack.</summary>
    SelectAcidSpit = 0xf6b3,
    /// <summary><c>$A8:F6F3 Function_KihunterWings_Attached</c>: copies X and Y from the physically preceding body slot, native enemy minus $40.</summary>
    FollowBody = 0xf6f3,
    /// <summary><c>$A8:F7CF Function_KihunterWings_Falling</c>: dispatches the detached wing's secondary arc function stored in its extended variable zero.</summary>
    DetachedWing = 0xf7cf,
}

/// <summary>The secondary function stored in detached-wing variable zero.</summary>
public enum KiHunterWingFunction : ushort
{
    /// <summary><c>$A8:F7DB Function_KihunterWings_Falling_DriftingLeft</c>: advances a radius-$30 orbit without collision, switching arcs when the angle crosses $C000.</summary>
    Orbit = 0xf7db,
    /// <summary><c>$A8:F8AD Function_KihunterWings_Falling_DriftingRight</c>: advances the opposite radius-$30 arc with vertical collision, deleting the wing on impact or switching back across $C000.</summary>
    FallingCollisionArc = 0xf8ad,
}

/// <summary>
/// Typed projection of Ki-Hunter's ordinary variables A-F plus the extended enemy words at
/// $7E:7800-$7828,x. Extended words cannot live in <see cref="RoomEnemySlot"/>'s common
/// 64-byte record, so they remain explicit here under the exact meanings used by the ROM.
/// Several words are intentionally aliased: the detached wing reuses the body's flight
/// variables for its orbit geometry, just as the original assembly does.
/// </summary>
public sealed class KiHunterEnemyState
{
    /// <summary>Live body or wing slot carrying Ki-Hunter's six common variable words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Captures the slot's body or wing role before native deletion can clear its definition identity.</summary>
    internal KiHunterEnemyState(RoomEnemySlot slot)
    {
        _slot = slot;
        // The definition word is mutable native storage and is cleared during deletion.
        // Role is established by the initializer and must survive that later write because
        // body/wing callbacks continue using physical +/-$40 slot aliases.
        IsWing = KiHunterBehavior.IsKiHunterWingDefinition(slot.EnemyDefinitionPointer);
    }

    /// <summary>Role fixed by the initializer, retained even if deletion clears the native definition word; body and wing callbacks still use adjacent physical slots.</summary>
    public bool IsWing { get; }

    /// <summary>Native variable A: main-AI dispatcher.</summary>
    public KiHunterEnemyFunction Function
    {
        get => (KiHunterEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Native variable B: swoop target X or detached-wing speed-table index.</summary>
    public ushort TargetXOrSpeedIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native variable C: the Y origin of the current swoop.</summary>
    public ushort SwoopOriginY
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable D: detached-wing quadratic-speed accumulator.</summary>
    public ushort DetachedSpeedAccumulator
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native variable E: angle at which the swoop changes animation.</summary>
    public ushort AnimationChangeAngle
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native variable F: 8.8-ish swoop/detached-wing angle accumulator.</summary>
    public ushort Angle
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Body $7E:7800,x signed whole-angle velocity cap: $FFFE for a leftward swoop or two for a rightward swoop.</summary>
    public ushort MaximumAngularVelocity { get; internal set; }
    /// <summary>Wing $7E:7800,x bank-$A8 secondary function pointer, alternating between non-colliding and collision-bearing detached arcs.</summary>
    public KiHunterWingFunction WingFunction { get; internal set; }
    /// <summary>Body $7E:7804,x signed high word of 16.16 angular velocity; this whole-angle step advances the swoop angle each frame.</summary>
    public ushort AngularVelocityWhole { get; internal set; }
    /// <summary>Body $7E:7806,x fractional low word of angular velocity; acceleration carries into <see cref="AngularVelocityWhole"/>.</summary>
    public ushort AngularVelocityFraction { get; internal set; }
    /// <summary>Body $7E:7808,x signed high word of angular acceleration, paired with <see cref="AngularAccelerationFraction"/> for a plus or minus one-eighth angle step per frame squared.</summary>
    public ushort AngularAccelerationWhole { get; internal set; }
    /// <summary>Body $7E:780A,x low word of 16.16 angular acceleration: $2000 rightward or $E000 with high word $FFFF leftward.</summary>
    public ushort AngularAccelerationFraction { get; internal set; }
    /// <summary>Body $7E:7814,x upper room-pixel patrol boundary, initialized sixteen pixels above the spawn Y.</summary>
    public ushort UpperPatrolY { get; internal set; }
    /// <summary>Wing $7E:7814,x 8.8 speed-table reset index computed at detachment by accumulating angular deltas until $2000; reapplied at each arc transition.</summary>
    public ushort DetachedSpeedReset { get; internal set; }
    /// <summary>Body $7E:7816,x lower room-pixel patrol boundary, initialized sixteen pixels below spawn Y; wing detachment also reads its own same-address word.</summary>
    public ushort LowerPatrolY { get; internal set; }
    /// <summary>Body $7E:7818/$781A,x initial room position retained from body initialization rather than the changing patrol position; collision recovery returns to patrol after rising above its Y.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }
    /// <summary>Body $7E:781E,x gameplay-frame thinking countdown: twelve after landing or twenty-four after acid emission, consumed once the animation selects ground wait.</summary>
    public ushort WaitTimer { get; internal set; }
    /// <summary>Body $7E:7820,x swipe-animation latch, cleared on swoop setup and set once the angle passes its facing-specific trigger.</summary>
    public bool SwoopAnimationChanged { get; internal set; }
    /// <summary>Body $7E:7822/$7824,x whole-pixel swoop ellipse radii: the absolute horizontal separation from Samus and Samus Y minus body Y when a swoop begins.</summary>
    public Extent16<RoomPixels> SwoopRadius { get; internal set; }
    /// <summary>Body $7E:780C-$7812,x signed 16.16 velocity in pixels per gameplay frame (X fraction/whole at $780C/$780E, Y at $7810/$7812); positive Y is downward and gravity accumulates through the Y fraction.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Wing $7E:7806/$7808,x signed pixel correction for the non-colliding arc, calculated at angle $E0 and radius $30.</summary>
    public Offset16<RoomPixels> OrbitOffset { get; internal set; }
    /// <summary>Wing $7E:7802/$7804,x signed pixel correction for the collision-bearing arc, calculated at angle $A0 and radius $30.</summary>
    public Offset16<RoomPixels> FallingArcOffset { get; internal set; }
    /// <summary>Wing $7E:780A/$780C,x room-pixel anchor used with an arc-specific correction; recaptured whenever the detached arc changes.</summary>
    public Position16<RoomPixels> OrbitCenter { get; internal set; }
    /// <summary>Wing $7E:7810/$780E,x original room position saved at detachment and restored when the collision-bearing arc deletes the wing.</summary>
    public Position16<RoomPixels> SavedWingPosition { get; internal set; }
    /// <summary>Body $7E:7828,x wingless latch, set by spawn parameter-one bit 15 or by health reaching the following wing record's parameter-one threshold.</summary>
    public bool HasLostWings { get; internal set; }
}
