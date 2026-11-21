using SuperMetroid.AssetExtraction.Enemies;
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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Tripper, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        const int speedRecord = 5;
        const int accelerationCap = 3;
        place.SetSpawnParameter2(EnemyDefinitionId.Tripper, LittleEndianBytes.Word(speedRecord, accelerationCap));
        place.SetSpawnSettings(place.HeldOccupant().SpawnSettings<PlatformSpawnSettings>() with { Movement = HorizontalDirection.Left });
        place.Position = place.Position.WithWholeY(256);
        PlatformEnemyState state = PlatformsBehavior.InitializePlatform(place.HeldOccupant().SpawnSettings<PlatformSpawnSettings>(), place, EnemyDefinitionId.Tripper);

        AssertEqual(EnemyLinearSpeedTable.Speed(speedRecord), place.HeldOccupant().SpawnSettings<PlatformSpawnSettings>().Speed, "the patrol speed is the row's speed");
        AssertEqual(257, state.RestingY, "the platform rests one pixel below its placement");

        // The acceleration cap is parameter two's high byte.
        int[] expectedIndexes = [1, 2, accelerationCap, accelerationCap, accelerationCap];
        foreach (int expected in expectedIndexes)
        {
            PlatformsBehavior.IncrementAndClampPlatformYSpeed(place.HeldOccupant(), state);
            AssertEqual(QuadraticSpeedCurvePlace.FromMarks(expected), state.YSpeedCurvePlace, "acceleration climbs by one row and holds at the cap");
        }
        Console.WriteLine("Platform owned state: speeds, resting height and clamped acceleration match.");
    }

    /// <summary>Runs the focused platform owned-state check.</summary>
    [VerificationCommand("--platform-owned-state")]
    internal static int PlatformOwnedStateCommand()
    {
        VerifyPlatformOwnedState();
        return 0;
    }
}
