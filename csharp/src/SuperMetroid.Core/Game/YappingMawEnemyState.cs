using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 function word stored in Yapping Maw's native variable A. Keeping the
/// cartridge addresses makes a debugger trace line up with WRAM and with $A8:A211's jump
/// dispatcher without a second, invented host state numbering scheme.
/// </summary>
public enum YappingMawAiFunction : ushort
{
    /// <summary>$A8:A235, Function_YappingMaw_Neutral: samples Samus distance/angle, refreshes the grab cooldown inside 32 pixels, and starts an attack only below the population activation threshold.</summary>
    WaitingForSamus = 0xa235,
    /// <summary>$A8:A28C, Function_YappingMaw_Attack: clears the curl accumulator, derives radii and mirrored bend from the target, and installs the eight-way attack art and held-Samus offsets.</summary>
    BeginExtension = 0xa28c,
    /// <summary>$A8:A445, Function_YappingMaw_Attacking: solves all four curled link positions, advances the quadratic angular motion, carries any captive with the mouth, and enters cooldown when the signed curl becomes negative.</summary>
    ExtendingOrRetracting = 0xa445,
    /// <summary>$A8:A68A, Function_YappingMaw_Cooldown: retains captive placement until the timer seeded at 64 underflows on call 65, then releases input and returns to neutral unless Samus death still owns the lock.</summary>
    RetractedDelay = 0xa68a,
}

/// <summary>
/// Typed projection of the unusually large Yapping Maw extension blocks at $7E:7800,
/// $7E:8000, and $7E:8800. Names describe the native words' actual consumers; arrays keep
/// the four identical tongue-link calculations inspectable without four copies of every
/// property.
/// </summary>
public sealed class YappingMawEnemyState
{
    internal YappingMawEnemyState(RoomEnemySlot owner) => Owner = owner;

    /// <summary>Physical enemy slot representing the moving mouth, distinct from the four body-projectile slots and the stationary root sprite-object slot.</summary>
    public RoomEnemySlot Owner { get; }
    /// <summary>Host projection of the native bank-$A8 variable-A dispatcher entry, initialized to neutral and advanced by targeting, curl completion, and cooldown expiration.</summary>
    public YappingMawAiFunction Function { get; internal set; }

    /// <summary>
    /// The immovable population position in whole room pixels, retained in native variables
    /// 0C/0D (Y at $7E:781E per enemy); link and mouth positions are offsets from this root
    /// rather than accumulated movement.
    /// </summary>
    public Position16<RoomPixels> Origin { get; internal set; }

    /// <summary>
    /// Final signed offsets from the root for the four successive tongue points (Y words at
    /// $7E:7802/$7806/$780A/$780E, positive downward). Link zero is nearest the root, link
    /// three is the mouth; the first three also position decorative eprojs.
    /// </summary>
    public Offset16<RoomPixels>[] SegmentOffsets { get; } = new Offset16<RoomPixels>[4];

    /// <summary>Approximate bank-$A0 distance used by the strict activation comparisons.</summary>
    public ushort DistanceToSamus { get; internal set; }

    /// <summary>Half of the clamped target distance, used as the tongue's X radius.</summary>
    public ushort SegmentRadius { get; internal set; }

    /// <summary>Zero-up, clockwise byte angle returned by CalculateAngleFromXY.</summary>
    public ushort AimAngle { get; internal set; }

    /// <summary>Byte-wrapped 64-minus-aim angle used to rotate the curled link corrections.</summary>
    public ushort CurlAngle { get; internal set; }

    /// <summary>Native variable 2F: zero bends one way, one bends the mirrored way.</summary>
    public ushort MirroredBend { get; internal set; }

    /// <summary>
    /// Signed room-pixel reference samples subtracted from each raw link before curl correction:
    /// X at angle $80 and SegmentRadius, Y (native $7E:781A) at angle $80 and half SegmentRadius.
    /// </summary>
    public Offset16<RoomPixels> Baseline { get; internal set; }

    /// <summary>
    /// Native B/C signed 16.16 curl amount. It accelerates past +128, wraps negative, and
    /// thereby signals that the retracting tongue has completed its full ROM curve.
    /// </summary>
    public ushort ExtensionWhole { get; internal set; }
    /// <summary>Native variable C's low half of the signed 16.16 curl-angle accumulator, in 1/65536-angle-unit increments; cleared at setup and at the +128 turn-around clamp.</summary>
    public ushort ExtensionFraction { get; internal set; }

    /// <summary>Native D byte offset into CommonEnemySpeeds_QuadraticallyIncreasing.</summary>
    public ushort QuadraticSpeedByteOffset { get; internal set; }

    /// <summary>Native E: 64-frame hold after the tongue fully retracts.</summary>
    public ushort RetractedDelayTimer { get; internal set; }

    /// <summary>Native F copied from population parameter one.</summary>
    public ushort ActivationDistance { get; internal set; }

    /// <summary>Native variable 30: the mouth currently owns Samus's input/position.</summary>
    public bool HasGrabbedSamus { get; internal set; }

    /// <summary>Signed whole room-pixel offset from the moving mouth to captive Samus, selected by aiming/animation opcodes (Y at native $7E:8026).</summary>
    public Offset16<RoomPixels> HeldSamusOffset { get; internal set; }

    /// <summary>Even byte offset zero through fourteen selecting one of eight facing lists.</summary>
    public ushort DirectionTableByteOffset { get; internal set; }

    /// <summary>
    /// Native variable 35. Main AI decrements it unconditionally; touch may grab only while
    /// its sign bit is set. Values 48 and zero therefore form real cooldown states, not bools.
    /// </summary>
    public ushort GrabCooldown { get; internal set; }

    /// <summary>Result of the point-based $A0:AD70 viewport test sampled before movement.</summary>
    public bool IsOffScreen { get; internal set; }

    /// <summary>Original OBJ palette bits restored after frozen flashing.</summary>
    public ushort OriginalPaletteBits { get; internal set; }

    /// <summary>The four physical bank-$86 body-link slots, ordered root to mouth.</summary>
    public RoomEnemyProjectileSlot?[] BodyProjectiles { get; } =
        new RoomEnemyProjectileSlot?[4];

    /// <summary>The bank-$B4 root/base object selected by population parameter two.</summary>
    public RoomSpriteObjectSlot? RootSpriteObject { get; internal set; }

    // Native variables 27..2E hold one correction offset per link; only bank-$A8
    // calculations mutate them.
    internal Offset16<RoomPixels>[] Corrections { get; } = new Offset16<RoomPixels>[4];
}
