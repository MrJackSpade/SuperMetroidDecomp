using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's unusually large state.
/// Names describe the values' actual consumers; arrays keep
/// the four identical tongue-link calculations inspectable without four copies of every
/// property.
/// </summary>
public sealed class YappingMawEnemyState
{
    /// <summary>Binds extended tongue and grab state to the live mouth enemy slot.</summary>
    internal YappingMawEnemyState(RoomEnemySlot owner) => Owner = owner;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<YappingMawProgram> Playback { get; set; }

    /// <summary>Physical enemy slot representing the moving mouth, distinct from the four body-projectile slots and the stationary root sprite object.</summary>
    public RoomEnemySlot Owner { get; }
    /// <summary>The behaviour main AI runs, initialized to neutral and advanced by targeting, curl completion, and cooldown expiration.</summary>
    public YappingMawAiFunction Function { get; internal set; }

    /// <summary>
    /// The immovable population position in whole room pixels, retained for the whole fight;
    /// link and mouth positions are offsets from this root
    /// rather than accumulated movement.
    /// </summary>
    public Position16<RoomPixels> Origin { get; internal set; }

    /// <summary>
    /// Final signed offsets from the root for the four successive tongue points (Y
    /// positive downward). Link zero is nearest the root, link
    /// three is the mouth; the first three also position decorative body-link projectiles.
    /// </summary>
    public Offset16<RoomPixels>[] SegmentOffsets { get; } = new Offset16<RoomPixels>[4];

    /// <summary>
    /// Approximate distance from the root to Samus in room pixels, 0 through 32768: a single axis's magnitude
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

    /// <summary>Whether the tongue bends the mirrored way, which it does when the curl angle lies in the turn's second half.</summary>
    public bool MirroredBend { get; internal set; }

    /// <summary>
    /// Signed room-pixel reference samples subtracted from each raw link before curl correction:
    /// X at angle 128 and SegmentRadius, Y at angle 128 and half SegmentRadius.
    /// </summary>
    public Offset16<RoomPixels> Baseline { get; internal set; }

    /// <summary>
    /// The signed 16.16 curl amount, in angle units. It accelerates out to the +128 turn-around clamp, where its
    /// fraction is cleared, then decelerates back; the update whose addition carries its whole part below zero is the
    /// one on which the retracting tongue has completed its full curve. Every addition carries the fraction into the
    /// whole part and wraps at 32 bits.
    /// </summary>
    public SignedSixteenSixteen Extension { get; internal set; }

    /// <summary>
    /// The row of the quadratically increasing common enemy speeds the next curl update adds: each update moves one
    /// row on, so the curl accelerates out and, after the turn-around, back.
    /// </summary>
    public int QuadraticSpeedRow { get; internal set; }

    /// <summary>
    /// Which of the row's two speeds the curl adds: the positive one while the tongue extends, the negative one after
    /// the turn-around clamp.
    /// </summary>
    internal EnemySpeedSign QuadraticSpeedSign { get; set; }

    /// <summary>64-frame hold after the tongue fully retracts.</summary>
    public int RetractedDelayTimer { get; internal set; }

    /// <summary>
    /// The distance from its spawn settings below which an attack starts. The game compares it by the sign of a
    /// wrapped difference, so a distance of 32768 or more behaves as the game's
    /// wrapped comparison does; <see cref="IsWithinActivationDistance"/> owns that comparison.
    /// </summary>
    public int ActivationDistance { get; internal set; }

    /// <summary>
    /// Whether <paramref name="distance"/> is strictly inside <see cref="ActivationDistance"/>, judged as the game
    /// does by the sign of their difference wrapped to sixteen bits.
    /// </summary>
    internal bool IsWithinActivationDistance(int distance) =>
        CoordinateAxis.Difference(distance, ActivationDistance) < 0;

    /// <summary>Whether the mouth currently holds Samus, owning her input and position.</summary>
    public bool HasGrabbedSamus { get; internal set; }

    /// <summary>Signed whole room-pixel offset from the moving mouth to captive Samus, selected by aiming/animation cues.</summary>
    public Offset16<RoomPixels> HeldSamusOffset { get; internal set; }

    /// <summary>The sector the bite aims into, selecting its facing animations; the held placement applies only to the two horizontal sectors.</summary>
    public YappingMawAim Aim { get; internal set; }

    /// <summary>
    /// The grab cooldown. Main AI decrements it unconditionally; touch may grab only while
    /// it is negative. Values 48 and zero therefore form real cooldown states, not bools.
    /// </summary>
    public WrappingCountdown GrabCooldown { get; internal set; }

    /// <summary>Result of the point-based viewport test sampled before movement.</summary>
    public bool IsOffScreen { get; internal set; }

    /// <summary>The four physical body-link slots, ordered root to mouth.</summary>
    public EnemyProjectile?[] BodyProjectiles { get; } =
        new EnemyProjectile?[4];

    /// <summary>The root/base object selected by population parameter two.</summary>
    public RoomSpriteObject? RootSpriteObject { get; internal set; }

    /// <summary>One curl correction per tongue link, changed only by calculations.</summary>
    internal Offset16<RoomPixels>[] Corrections { get; } = new Offset16<RoomPixels>[4];
}
