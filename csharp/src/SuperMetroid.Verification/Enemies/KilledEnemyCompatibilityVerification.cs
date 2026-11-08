using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks the compatibility rules for what the game reads of an enemy after a hit killed it (#1543).</summary>
internal static class KilledEnemyCompatibilityVerification
{
    /// <summary>
    /// Confirms each rule: a killed fingernail gives its sibling's launch no speed, the destroyed shutter is never found
    /// moving, a killed Gamet holds its formation back, a killed Evir body reads as facing left, and the rest of a
    /// Shaktool chain reads the killed segment's emptied remains for that update and keeps what it writes there.
    /// </summary>
    internal static void VerifyKilledEnemyCompatibility()
    {
        AssertEqual(5, KilledFingernailStops.SiblingSpeed(5, stopped: 0, siblingKilled: false), "a living sibling gives its last speed");
        AssertEqual(0, KilledFingernailStops.SiblingSpeed(5, stopped: 0, siblingKilled: true), "a killed sibling gives a stopped nail's");

        AssertTrue(DestroyedShutterReaction.FindsMoving(moving: true, destroyed: false), "a moving shutter the hit left is found moving");
        AssertTrue(!DestroyedShutterReaction.FindsMoving(moving: true, destroyed: true), "the shutter the hit destroyed is found at rest");
        AssertTrue(!DestroyedShutterReaction.FindsMoving(moving: false, destroyed: false), "a shutter at rest is found at rest");

        AssertTrue(KilledGametHoldsFormation.Waits("waiting", static _ => true), "a living member waiting counts as waiting");
        AssertTrue(!KilledGametHoldsFormation.Waits("rising", static _ => false), "a living member that is not waiting holds the formation");
        AssertTrue(!KilledGametHoldsFormation.Waits<string>(null, static _ => true), "a killed member holds the formation");

        AssertEqual(EvirFacing.Right, KilledEvirBodyFacing.Facing(EvirFacing.Right, EvirFacing.Left, bodyKilled: false), "a living body reads as it faces");
        AssertEqual(EvirFacing.Left, KilledEvirBodyFacing.Facing(EvirFacing.Right, EvirFacing.Left, bodyKilled: true), "a killed body reads as facing left");

        var enemies = new RoomEnemySystem(new EnemySession(new UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot killedPlace = enemies.SlotTable.Slots[3];
        RoomEnemySlot otherPlace = enemies.SlotTable.Slots[4];
        var segments = new KilledShaktoolSegment<RoomEnemySlot, ShaktoolSegmentState>();
        AssertEqual<ShaktoolSegmentState?>(null, segments.RemainsAt(killedPlace), "no remains before a kill");
        segments.RecordKill(killedPlace);
        ShaktoolSegmentState remains = segments.RemainsAt(killedPlace)
            ?? throw new InvalidOperationException("The killed segment left no remains.");
        AssertEqual(RotationAngle.Zero, remains.OrbitAngle, "the remains have no orbit");
        AssertEqual(0, remains.AngularVelocity, "the remains have no angular speed");
        AssertEqual<ShaktoolFrameBehavior?>(null, remains.FrameBehavior, "the remains have no motion");
        AssertEqual<ShaktoolSegmentState?>(null, segments.RemainsAt(otherPlace), "a place no kill emptied has no remains");
        remains.TargetAngle = RotationAngle.FromRaw(0x4000);
        AssertTrue(ReferenceEquals(remains, segments.RemainsAt(killedPlace)), "the chain reads back what it wrote to the remains");
        segments.RecordKill(otherPlace);
        AssertEqual<ShaktoolSegmentState?>(null, segments.RemainsAt(killedPlace), "a later kill replaces the remains");
        Console.WriteLine("Killed-enemy compatibility: fingernails, shutters, Gamets, Evir bodies and Shaktool segments match.");
    }

    /// <summary>Runs the focused killed-enemy compatibility check.</summary>
    [VerificationCommand("--killed-enemy-compatibility")]
    internal static int KilledEnemyCompatibilityCommand()
    {
        VerifyKilledEnemyCompatibility();
        return 0;
    }
}
