using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
namespace SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Shaktool segment state the chain owns: wrapping orbit angles, the centre's orientation split from the saw's curl, and the cleared record.</summary>
internal static class ShaktoolOwnedStateVerification
{
    /// <summary>
    /// On the real seven-segment group: the centre's orbit wraps through zero, a reversed chain flips the centre's
    /// midpoint a half turn, a first wall reversal empties the saw's curl progress, clears straightening, sets the
    /// reversed-once flag on every segment and toggles the centre's reversal, and a kill empties a segment's record.
    /// </summary>
    internal static void VerifyShaktoolOwnedState()
    {
        RoomEnemySystem enemies = CreateShaktoolGroup();
        ShaktoolSegmentState[] states = [.. Enumerable.Range(0, 7).Select(index =>
            enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException($"Shaktool segment {index} has no state."))];
        RoomEnemySlot center = enemies.SlotTable.Slots[3];
        ShaktoolSegmentState centerState = states[3];

        foreach ((bool reversed, int expectedBucket) in new[] { (false, 0x80), (true, 0x00) })
        {
            centerState.MotionFlags = 0;
            centerState.OrbitAngle = SnesAngle.FromRaw(0xff80);
            centerState.AngularVelocity = 0x0100;
            centerState.ResetOrientation();
            centerState.OrientationReversed = reversed;
            states[4].OrbitAngle = SnesAngle.FromRaw(0x8080);
            enemies.ShaktoolBehavior.AdvanceAndOrientShaktoolCenter(center, centerState);
            AssertEqual((ushort)0x0080, centerState.OrbitAngle.RawValue, "the centre's orbit wraps through zero");
            AssertEqual(expectedBucket, centerState.OrientationBucket, $"reversed={reversed} selects the expected heading bucket");
            AssertEqual(reversed, centerState.OrientationReversed, "orienting keeps the chain's reversal");
        }

        foreach (ShaktoolSegmentState state in states)
            state.MotionFlags = ShaktoolMotionFlags.Straightening;
        centerState.ResetOrientation();
        RoomEnemySlot tail = enemies.SlotTable.Slots[6];
        states[6].CurlProgress = 0x1234;
        enemies.ShaktoolBehavior.ReverseShaktoolAfterCollision(tail, states[6], tail.Position.Whole);
        AssertEqual(0, states[6].CurlProgress, "a wall reversal empties the saw's curl progress");
        AssertTrue(centerState.OrientationReversed, "a first wall reversal swaps the chain's ends");
        for (int index = 0; index < states.Length; index++)
        {
            AssertEqual(ShaktoolMotionFlags.ReversedOnce, states[index].MotionFlags, $"segment {index} has reversed once and stopped straightening");
            AssertEqual(ShaktoolPreInstruction.IdleAfterAttack, states[index].PreInstruction, $"segment {index} idles through the collision response");
        }

        tail.Clear();
        AssertEqual((SnesAngle.Zero, SnesAngle.Zero, 0, 0), (states[6].TargetAngle, states[6].OrbitAngle, states[6].AngularVelocity, states[6].OwnerSlotIndex),
            "a kill empties the segment's angles, rate and owner");
        AssertEqual(null, states[6].PreInstruction, "a kill empties the segment's motion");
        Console.WriteLine("Shaktool owned state: orbit wrap, reversed orientation, wall reversal and the cleared record match.");
    }

    /// <summary>Initializes the seven Shaktool segments in slots zero through six.</summary>
    private static RoomEnemySystem CreateShaktoolGroup()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 0; index < 7; index++)
        {
            RoomEnemySlot slot = enemies.SlotTable.Slots[index];
            slot.Bind(EnemyDefinitionId.Shaktool, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xaa });
            slot.SetSpawnParameter2(unchecked((ushort)(index * 2)));
            slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(0x0100 + index * 16)), 0x0200));
            enemies.ShaktoolBehavior.InitializeShaktool(slot);
        }
        return enemies;
    }

    /// <summary>Runs the focused Shaktool owned-state check.</summary>
    [VerificationCommand("--shaktool-owned-state")]
    internal static int ShaktoolOwnedStateCommand()
    {
        VerifyShaktoolOwnedState();
        return 0;
    }
}
