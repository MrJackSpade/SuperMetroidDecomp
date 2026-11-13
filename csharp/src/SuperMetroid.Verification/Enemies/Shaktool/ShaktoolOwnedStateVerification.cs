using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Shaktool segment state the chain owns: wrapping orbit angles, and the centre's orientation split from the saw's curl.</summary>
internal static class ShaktoolOwnedStateVerification
{
    /// <summary>
    /// On the real seven-segment group: the centre's orbit wraps through zero, a reversed chain flips the centre's
    /// midpoint a half turn, a first wall reversal empties the saw's curl progress, clears straightening, sets the
    /// reversed-once flag on every segment and toggles the centre's reversal, and a chain is refused anywhere but the
    /// first place.
    /// </summary>
    internal static void VerifyShaktoolOwnedState()
    {
        RoomEnemySystem enemies = CreateShaktoolGroup();
        ShaktoolSegmentState[] states = [.. Enumerable.Range(0, 7).Select(index =>
            enemies.ShaktoolSegments[index] ?? throw new InvalidOperationException($"Shaktool segment {index} has no state."))];
        RoomEnemySlot center = enemies.SlotTable.Slots[3];
        ShaktoolSegmentState centerState = states[3];

        foreach ((bool reversed, CompassDirection expectedOrientation) in new[] { new VerifyShaktoolOwnedStateCase(false, CompassDirection.Down), new VerifyShaktoolOwnedStateCase(true, CompassDirection.Up) })
        {
            centerState.MotionFlags = 0;
            centerState.OrbitAngle = RotationAngle.FromRaw(0xff80);
            centerState.AngularVelocity = RotationAngle.FromSteps(1);
            centerState.ResetOrientation();
            centerState.OrientationReversed = reversed;
            states[4].OrbitAngle = RotationAngle.FromRaw(0x8080);
            enemies.ShaktoolBehavior.AdvanceAndOrientShaktoolCenter(center, centerState);
            AssertEqual((ushort)0x0080, centerState.OrbitAngle.RawValue, "the centre's orbit wraps through zero");
            AssertEqual(expectedOrientation, centerState.Orientation, $"reversed={reversed} selects the expected head orientation");
            AssertEqual(reversed, centerState.OrientationReversed, "orienting keeps the chain's reversal");
        }

        foreach (ShaktoolSegmentState state in states)
            state.MotionFlags = ShaktoolMotionFlags.Straightening;
        centerState.ResetOrientation();
        RoomEnemySlot tail = enemies.SlotTable.Slots[6];
        states[6].CurlProgress = RotationAngle.FromSteps(0x12, 0x34);
        enemies.ShaktoolBehavior.ReverseShaktoolAfterCollision(tail, states[6], tail.Position.Whole);
        AssertEqual(RotationAngle.Zero, states[6].CurlProgress, "a wall reversal empties the saw's curl progress");
        AssertTrue(centerState.OrientationReversed, "a first wall reversal swaps the chain's ends");
        for (int index = 0; index < states.Length; index++)
        {
            AssertEqual(ShaktoolMotionFlags.ReversedOnce, states[index].MotionFlags, $"segment {index} has reversed once and stopped straightening");
            AssertEqual(ShaktoolFrameBehavior.Idle, states[index].FrameBehavior, $"segment {index} idles through the collision response");
        }

        // The chain's death deletes the first seven slots, so a chain starting anywhere else is refused (#1500).
        var misplacedEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot misplaced = misplacedEnemies.SlotTable.Slots[1];
        misplaced.Bind(EnemyDefinitionId.Shaktool, misplaced.Definition);
        misplaced.Bind(misplaced.DefinitionId, default(RoomEnemyDefinition));
        misplaced.SetSpawnParameter2(EnemyDefinitionId.Shaktool, 0);
        AssertThrows<InvalidDataException>(() => misplacedEnemies.ShaktoolBehavior.InitializeShaktool(misplaced),
            "a Shaktool chain starting after the first slot is refused");
        Console.WriteLine("Shaktool owned state: orbit wrap, reversed orientation, wall reversal and the first-slot chain match.");
    }

    /// <summary>Initializes the seven Shaktool segments in slots zero through six.</summary>
    private static RoomEnemySystem CreateShaktoolGroup()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 0; index < 7; index++)
        {
            RoomEnemySlot slot = enemies.SlotTable.Slots[index];
            slot.Bind(EnemyDefinitionId.Shaktool, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.SetSpawnParameter2(EnemyDefinitionId.Shaktool, unchecked((ushort)(index * 2)));
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
