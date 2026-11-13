using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Platforms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Tripper and Kamer state the platforms own: speeds, resting height and acceleration.</summary>
internal static class PlatformOwnedStateVerification
{
    /// <summary>
    /// Initializes a Tripper on the room's last pixel row and confirms its speeds come from the linear-speed record as
    /// one 16.16 value each, its resting height wraps to the top row, and its acceleration index climbs by one and
    /// holds at the cap.
    /// </summary>
    internal static void VerifyPlatformOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
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
        AssertEqual(EnemyQuadraticSpeedIndex.FromRows(accelerationCap), state.MaximumYSpeedTableIndex, "the acceleration cap is parameter two's high byte");

        int[] expectedIndexes = [1, 2, 3, 3, 3];
        foreach (int expected in expectedIndexes)
        {
            PlatformsBehavior.IncrementAndClampPlatformYSpeed(state);
            AssertEqual(EnemyQuadraticSpeedIndex.FromRows(expected), state.YSpeedTableIndex, "acceleration climbs by one row and holds at the cap");
        }
        Console.WriteLine("Platform owned state: speeds, wrapped resting height and clamped acceleration match.");
    }

    /// <summary>Runs the focused platform owned-state check.</summary>
    [VerificationCommand("--platform-owned-state")]
    internal static int PlatformOwnedStateCommand()
    {
        VerifyPlatformOwnedState();
        return 0;
    }
}
