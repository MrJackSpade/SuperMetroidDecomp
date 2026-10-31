using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// Typed projection of the unusually large Yapping Maw extension blocks at $7E:7800,
/// $7E:8000, and $7E:8800. Names describe the native words' actual consumers; arrays keep
/// the four identical tongue-link calculations inspectable without four copies of every
/// property.
/// </summary>
public sealed class YappingMawEnemyState
{
    /// <summary>Binds extended tongue and grab state to the live mouth enemy slot.</summary>
    internal YappingMawEnemyState(RoomEnemySlot owner) => Owner = owner;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<YappingMawProgram> Program { get; set; }

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

    /// <summary>
    /// Approximate bank-$A0 distance from the root to Samus in room pixels, 0 through 32768: a single axis's magnitude
    /// when Samus is 255 or more pixels away on it, otherwise the sum of two short trigonometric products. An attack
    /// caps it at <see cref="YappingMawAttackGeometry.MaximumTargetDistance"/>, so the extension sees 32 through 64.
    /// </summary>
    public int DistanceToSamus { get; internal set; }

    /// <summary>Half of the capped target distance in room pixels, 16 through 32, used as the tongue's X radius.</summary>
    public int SegmentRadius { get; internal set; }

    /// <summary>The bearing to Samus returned by CalculateAngleFromXY.</summary>
    public Bearing AimAngle { get; internal set; }

    /// <summary><see cref="AimAngle"/> in the table convention; rotates the curled link corrections.</summary>
    public RotationAngle CurlAngle { get; internal set; }

    /// <summary>Native variable 2F: whether the tongue bends the mirrored way, which it does when the curl angle lies in the turn's second half.</summary>
    public bool MirroredBend { get; internal set; }

    /// <summary>
    /// Signed room-pixel reference samples subtracted from each raw link before curl correction:
    /// X at angle $80 and SegmentRadius, Y (native $7E:781A) at angle $80 and half SegmentRadius.
    /// </summary>
    public Offset16<RoomPixels> Baseline { get; internal set; }

    /// <summary>
    /// The signed 16.16 curl amount, in angle units. It accelerates out to the +128 turn-around clamp, where its
    /// fraction is cleared, then decelerates back; the update whose addition carries its whole part below zero is the
    /// one on which the retracting tongue has completed its full curve. Every addition carries the fraction into the
    /// whole part and wraps at 32 bits.
    /// </summary>
    public SignedSixteenSixteen Extension { get; internal set; }

    /// <summary>Record index into CommonEnemySpeeds_QuadraticallyIncreasing (native D holds the byte offset).</summary>
    public int QuadraticSpeedRecord { get; internal set; }

    /// <summary>Whether the negative half of the record is read (native D's extra +4 at the turn-around clamp).</summary>
    public bool QuadraticSpeedNegative { get; internal set; }

    /// <summary>Native E: 64-frame hold after the tongue fully retracts.</summary>
    public WrappingCountdown RetractedDelayTimer { get; internal set; }

    /// <summary>
    /// Native F: the population parameter-one distance, 0 through 65535, below which an attack starts. The cartridge
    /// compares it by the sign of a sixteen-bit difference, so a parameter of 32768 or more behaves as the cartridge's
    /// wrapped comparison does; <see cref="IsWithinActivationDistance"/> owns that comparison.
    /// </summary>
    public int ActivationDistance { get; internal set; }

    /// <summary>
    /// Whether <paramref name="distance"/> is strictly inside <see cref="ActivationDistance"/>, judged as the cartridge
    /// does by the sign of their difference wrapped to sixteen bits.
    /// </summary>
    internal bool IsWithinActivationDistance(int distance) =>
        CoordinateAxis.Difference(distance, ActivationDistance) < 0;

    /// <summary>Native variable 30: the mouth currently owns Samus's input/position.</summary>
    public bool HasGrabbedSamus { get; internal set; }

    /// <summary>Signed whole room-pixel offset from the moving mouth to captive Samus, selected by aiming/animation opcodes (Y at native $7E:8026).</summary>
    public Offset16<RoomPixels> HeldSamusOffset { get; internal set; }

    /// <summary>The sector the bite aims into, selecting its facing animations; the held placement applies only to the two horizontal sectors.</summary>
    public YappingMawAim Aim { get; internal set; }

    /// <summary>
    /// Native variable 35. Main AI decrements it unconditionally; touch may grab only while
    /// its sign bit is set. Values 48 and zero therefore form real cooldown states, not bools.
    /// </summary>
    public WrappingCountdown GrabCooldown { get; internal set; }

    /// <summary>Result of the point-based $A0:AD70 viewport test sampled before movement.</summary>
    public bool IsOffScreen { get; internal set; }

    /// <summary>The actor's own colours, captured at room load and restored to every link after frozen flashing.</summary>
    public SpriteColouring OriginalColouring { get; internal set; } = SharedSpriteColouring.Common;

    /// <summary>The four physical bank-$86 body-link slots, ordered root to mouth.</summary>
    public RoomEnemyProjectileSlot?[] BodyProjectiles { get; } =
        new RoomEnemyProjectileSlot?[4];

    /// <summary>The bank-$B4 root/base object selected by population parameter two.</summary>
    public RoomSpriteObjectSlot? RootSpriteObject { get; internal set; }

    /// <summary>One native variable-27..2E curl correction per tongue link, changed only by bank-$A8 calculations.</summary>
    internal Offset16<RoomPixels>[] Corrections { get; } = new Offset16<RoomPixels>[4];
}
