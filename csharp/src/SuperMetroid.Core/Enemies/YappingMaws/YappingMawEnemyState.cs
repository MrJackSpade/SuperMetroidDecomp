using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's unusually large state.
/// Names describe the values' actual consumers; the four identical tongue links are one list of link records (#2162).
/// </summary>
public sealed class YappingMawEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<YappingMawSegment> Playback { get; set; }

    /// <summary>The behaviour main AI runs, initialized to neutral and advanced by targeting, curl completion, and cooldown expiration.</summary>
    public YappingMawAction Action { get; internal set; }

    /// <summary>
    /// The immovable population position in whole room pixels, retained for the whole fight;
    /// link and mouth positions are offsets from this root
    /// rather than accumulated movement.
    /// </summary>
    public Position16<RoomPixels> Origin { get; internal set; }

    /// <summary>
    /// The four tongue links, root to mouth: each link's body projectile and its far end's solved offset from the root. The
    /// last link's far end is the mouth; the first three far ends also place the next links' body projectiles.
    /// </summary>
    public required IReadOnlyList<YappingMawLink> Links { get; init; }

    /// <summary>
    /// Approximate distance from the root to Samus in room pixels, 0 through 32768: a single axis's magnitude
    /// when Samus is 255 or more pixels away on it, otherwise the sum of two short trigonometric products. An attack
    /// caps it at <see cref="YappingMawAttackGeometry.MaximumTargetDistance"/>, so the extension sees 32 through 64.
    /// </summary>
    public int DistanceToSamus { get; internal set; }

    /// <summary>Half of the capped target distance in room pixels, 16 through 32, used as the tongue's X radius.</summary>
    public int SegmentRadius { get; internal set; }

    /// <summary>The bearing from the maw to Samus as it aims its bite.</summary>
    public Bearing AimAngle { get; internal set; }

    /// <summary><see cref="AimAngle"/> as a rotation; rotates the curled link corrections.</summary>
    public RotationAngle CurlAngle { get; internal set; }

    /// <summary>Whether the tongue bends the mirrored way, which it does when the curl angle lies in the turn's second half.</summary>
    public bool MirroredBend { get; internal set; }

    /// <summary>
    /// Signed room-pixel reference samples subtracted from each raw link before curl correction:
    /// X at angle 128 and SegmentRadius, Y at angle 128 and half SegmentRadius.
    /// </summary>
    public Offset16<RoomPixels> Baseline { get; internal set; }

    /// <summary>
    /// The signed 1/65536-step curl amount, in angle units. It accelerates out to the +128 turn-around clamp, where its
    /// fraction is cleared, then decelerates back; the update whose addition carries its whole part below zero is the
    /// one on which the retracting tongue has completed its full curve. Every addition carries the fraction into the
    /// whole part and wraps at 32 bits.
    /// </summary>
    public SignedSixteenSixteen Extension { get; internal set; }

    /// <summary>
    /// The whole-mark place along the quadratically increasing common enemy speeds the next curl update adds: each
    /// update moves one mark on, so the curl accelerates out and, after the turn-around, back.
    /// </summary>
    public QuadraticSpeedCurvePlace SpeedCurvePlace { get; internal set; }

    /// <summary>
    /// Which way the curl adds the mark's speed: as it is while the tongue extends, negated after the turn-around
    /// clamp.
    /// </summary>
    internal EnemySpeedSign QuadraticSpeedSign { get; set; }

    /// <summary>The updates left in the hold after the tongue fully retracts, the one it releases Samus on included (#2281).</summary>
    public int RetractedDelayTimer { get; internal set; }

    /// <summary>
    /// The distance from its spawn settings below which an attack starts, in pixels: every authored Maw's lies between
    /// 48 and 128.
    /// </summary>
    public int ActivationDistance { get; internal set; }

    /// <summary>
    /// Whether <paramref name="distance"/> is strictly inside <see cref="ActivationDistance"/>. The game compares them by
    /// the sign of their difference; with the target distance at most 32768 and an authored activation distance, that
    /// difference never reaches half the counting range, so the plain comparison is the game's.
    /// </summary>
    internal bool IsWithinActivationDistance(int distance) => distance < ActivationDistance;

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

    /// <summary>The root/base object its attack direction selects, which every Maw spawns with (#2284).</summary>
    public required RoomEffectActor RootEffectActor { get; init; }
}
