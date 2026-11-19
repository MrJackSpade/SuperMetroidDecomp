using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's state: its aim and curl, its tongue's four links from root to mouth (#2162) and its hold on
/// Samus.
/// </summary>
public sealed class YappingMawEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<YappingMawSegment> Playback { get; set; }

    /// <summary>The behaviour main AI runs, initialized to neutral and advanced by targeting, curl completion, and cooldown expiration.</summary>
    public YappingMawAction Action { get; internal set; }

    /// <summary>
    /// Where the Maw was placed, in whole room pixels, which never moves: its links and its mouth are placed as offsets
    /// from it.
    /// </summary>
    public Position16<RoomPixels> Origin { get; internal set; }

    /// <summary>
    /// The four tongue links, root to mouth: each link's body projectile and its far end's solved offset from the root. The
    /// last link's far end is the mouth; the first three far ends also place the next links' body projectiles.
    /// </summary>
    public required IReadOnlyList<YappingMawLink> Links { get; init; }

    /// <summary>
    /// Approximate distance from the root to Samus in room pixels: a single axis's magnitude when Samus is 255 or more
    /// pixels away on it, otherwise the sum of two short trigonometric products. An attack
    /// caps it at <see cref="YappingMawAttackGeometry.MaximumTargetDistance"/>, so the extension sees 32 through 64.
    /// </summary>
    public int DistanceToSamus { get; internal set; }

    /// <summary>Half of the capped target distance in room pixels, 16 through 32, used as the tongue's X radius.</summary>
    public int SegmentRadius { get; internal set; }

    /// <summary>The bearing from the maw to Samus as it aims its bite.</summary>
    public Bearing AimAngle { get; internal set; }

    /// <summary><see cref="AimAngle"/> as a rotation; rotates the curled link corrections.</summary>
    public RotationAngle CurlAngle { get; internal set; }

    /// <summary>
    /// Whether the tongue bends the mirrored way, which it does when the curl angle lies in the turn's second half: a fact
    /// of <see cref="CurlAngle"/> (#2512).
    /// </summary>
    public bool MirroredBend => CurlAngle >= RotationAngle.HalfTurn;

    /// <summary>
    /// The uncurled tongue's offsets, subtracted from each link before the curl's correction: X a half turn along at the
    /// segment radius, Y a half turn along at half of it.
    /// </summary>
    public Offset16<RoomPixels> Baseline { get; internal set; }

    /// <summary>
    /// How far the tongue has curled, in angle units, fraction included. It accelerates out to the largest curl, where it
    /// stops on the whole unit, then decelerates back; the update that carries it below zero is the one on which the
    /// retracting tongue has completed its full curve.
    /// </summary>
    public SignedSixteenSixteen Extension { get; internal set; }

    /// <summary>
    /// The whole-mark place along the quadratically increasing common enemy speeds the next curl update adds: each
    /// update moves one mark on, so the curl accelerates out and, after the turn-around, back.
    /// </summary>
    public QuadraticSpeedCurvePlace SpeedCurvePlace { get; internal set; }

    /// <summary>
    /// Whether the tongue curls out or back (#2450): the curl adds the mark's speed while it extends and takes it away
    /// after the turn-around clamp.
    /// </summary>
    internal YappingMawCurlPhase CurlPhase { get; set; }

    /// <summary>The updates left in the hold after the tongue fully retracts, the one it releases Samus on included (#2281).</summary>
    public int RetractedDelayTimer { get; internal set; }

    /// <summary>Whether the mouth currently holds Samus, owning her input and position.</summary>
    public bool HasGrabbedSamus { get; internal set; }

    /// <summary>Signed whole room-pixel offset from the moving mouth to captive Samus, selected by aiming/animation cues.</summary>
    public Offset16<RoomPixels> HeldSamusOffset { get; internal set; }

    /// <summary>The sector the bite aims into, selecting its facing animations; the held placement applies only to the two horizontal sectors.</summary>
    public YappingMawAim Aim { get; internal set; }

    /// <summary>
    /// When the Maw may grab Samus (<see cref="YappingMawGrabCooldown"/>): counted on every update of the Maw, started at
    /// <see cref="YappingMawAttackDefinitions.GrabCooldown"/> as it retracts, lets Samus go or waits with her inside
    /// <see cref="YappingMawAttackDefinitions.GrabSafetyDistance"/>, and spent as it grabs.
    /// </summary>
    public YappingMawGrabCooldown GrabCooldown { get; internal set; }

    /// <summary>Whether the Maw lay outside the view as its update began, which silences its bite.</summary>
    public bool IsOffScreen { get; internal set; }

    /// <summary>The root/base object its attack direction selects, which every Maw spawns with (#2284).</summary>
    public required RoomEffectActor RootEffectActor { get; init; }
}
