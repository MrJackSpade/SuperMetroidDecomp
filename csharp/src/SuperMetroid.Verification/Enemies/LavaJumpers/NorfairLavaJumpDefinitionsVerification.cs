using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.LavaJumpers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Norfair lava-jumper takeoff velocities and RNG selection against the native four-word table.</summary>
internal static class NorfairLavaJumpDefinitionsVerification
{
    /// <summary>Compares all RNG values through production jump setup while forbidding runtime reads of the migrated velocity table.</summary>
    internal static void VerifyNorfairLavaJumpDefinitions(CartridgeImportAddressSpace rom)
    {

        for (ushort selector = 0; selector < 4; selector++)
        {
            ushort random = unchecked((ushort)(selector << 9));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2be86 + selector * 2)),
                NorfairLavaJumpDefinitions.Launch.Choose(random).RawValue,
                $"Norfair lava jumper velocity {selector}");
        }

        ushort nextRandom = 0;
        int advances = 0;
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () =>
            {
                advances++;
                return nextRandom;
            };
        Action<RoomEnemyPlace, SqueeptState> runMain = enemies.SqueeptBehavior.RunSqueeptMain;
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        var state = new SqueeptState(SqueeptAction.BeginJump);

        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            nextRandom = unchecked((ushort)random);
            advances = 0;
            state.Action = SqueeptAction.BeginJump;
            state.YVelocity = default;
            place.Contact = EnemyContact.None;
            place.Processing = EnemyProcessing.None;
            place.Presence = EnemyPresence.None;
            runMain(place, state);

            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2be86 + ((LittleEndianBytes.HighByte(unchecked((ushort)random))) & 6))), state.YVelocity.RawValue,
                $"Norfair lava jumper production velocity {random:X4}");
            AssertEqual(SqueeptAction.RiseBeforeAnimationSwitch,
                state.Action, $"Norfair lava jumper production handoff {random:X4}");
            AssertTrue(place.Processing.HasAny(EnemyProcessing.OffScreen),
                $"Norfair lava jumper off-screen processing {random:X4}");
            AssertEqual((SoundEffectId?)EnemySound.SplashIn, enemies.Audio.LatestSound,
                $"Norfair lava jumper sound {random:X4}");
            AssertEqual(1, advances, $"Norfair lava jumper RNG advance {random:X4}");
        }

        Console.WriteLine(
            "Norfair lava-jump definitions: four native velocities and all 65,536 real RNG selections pass with the source table forbidden.");
    }

}
