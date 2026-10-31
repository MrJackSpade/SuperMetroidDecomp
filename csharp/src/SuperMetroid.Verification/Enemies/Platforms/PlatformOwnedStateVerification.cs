using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Platforms;
namespace SuperMetroid.Verification.Enemies.Platforms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Tripper and Kamer state the platforms own: speeds, resting height, acceleration and the cleared record.</summary>
internal static class PlatformOwnedStateVerification
{
    /// <summary>
    /// Initializes a Tripper on the room's last pixel row and confirms its speeds come from the linear-speed record as
    /// one 16.16 value each, its resting height wraps to the top row, its acceleration index climbs by one and holds
    /// at the cap, and a kill empties the values kept in the common record.
    /// </summary>
    internal static void VerifyPlatformOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Tripper, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.Tripper, (ushort)PlatformHorizontalMovement.Left);
        const int speedRecord = 5;
        const int accelerationCap = 3;
        slot.SetSpawnParameter2(EnemyDefinitionId.Tripper, LittleEndianBytes.Word(speedRecord, accelerationCap));
        slot.Position = slot.Position.WithWholeY(ushort.MaxValue);
        PlatformsBehavior.InitializePlatform(slot);
        PlatformEnemyState state = enemies.PlatformStates[0]
            ?? throw new InvalidOperationException("The initialized Tripper has no platform state.");

        EnemyLinearSpeedRow speeds = EnemyLinearSpeeds.Row(speedRecord);
        AssertEqual(speeds.Positive, state.RightSpeed, "rightward speed is the row's positive speed");
        AssertEqual(speeds.Negative, state.LeftSpeed, "leftward speed is the row's negative speed");
        AssertEqual((ushort)0, state.RestingHeight.Y, "one pixel below the last row wraps to the top row");
        AssertEqual(accelerationCap, state.MaximumYSpeedTableIndex, "the acceleration cap is parameter two's high byte");

        int[] expectedIndexes = [1, 2, 3, 3, 3];
        foreach (int expected in expectedIndexes)
        {
            PlatformsBehavior.IncrementAndClampPlatformYSpeed(state);
            AssertEqual(expected, state.YSpeedTableIndex, "acceleration climbs by one and holds at the cap");
        }

        slot.Clear();
        AssertEqual(SignedSixteenSixteen.Zero, state.RightSpeed, "a kill empties the rightward speed");
        AssertEqual(SignedSixteenSixteen.Zero, state.LeftSpeed, "a kill empties the leftward speed");
        AssertEqual(Position16<RoomPixels>.Zero, state.RestingHeight, "a kill empties the resting height");
        AssertEqual(0, state.YSpeedTableIndex, "a kill empties the acceleration index");
        Console.WriteLine("Platform owned state: speeds, wrapped resting height, clamped acceleration and the cleared record match.");
    }

    /// <summary>Runs the focused platform owned-state check.</summary>
    [VerificationCommand("--platform-owned-state")]
    internal static int PlatformOwnedStateCommand()
    {
        VerifyPlatformOwnedState();
        return 0;
    }
}
