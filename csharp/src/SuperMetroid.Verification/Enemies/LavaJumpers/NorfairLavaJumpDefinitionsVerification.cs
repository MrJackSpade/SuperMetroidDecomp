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
    internal static void VerifyNorfairLavaJumpDefinitions(SuperMetroidAddressSpace rom)
    {

        for (ushort selector = 0; selector < 4; selector++)
        {
            ushort random = unchecked((ushort)(selector << 9));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2be86 + selector * 2)),
                NorfairLavaJumps.InitialVerticalVelocity(random).RawValue,
                $"Norfair lava jumper velocity {selector}");
        }

        ushort nextRandom = 0;
        int advances = 0;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () =>
            {
                advances++;
                return nextRandom;
            };
        Action<RoomEnemySlot, NorfairLavaJumpingEnemyState> runMain = enemies.NorfairLavaJumpingEnemyBehavior.RunNorfairLavaJumpingEnemyMain;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new NorfairLavaJumpingEnemyState();

        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            nextRandom = unchecked((ushort)random);
            advances = 0;
            state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
            state.YVelocity = default;
            slot.Contact = EnemyContact.None;
            slot.Processing = EnemyProcessing.None;
            slot.Presence = EnemyPresence.None;
            runMain(slot, state);

            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2be86 + ((LittleEndianBytes.HighByte(unchecked((ushort)random))) & 6))), state.YVelocity.RawValue,
                $"Norfair lava jumper production velocity {random:X4}");
            AssertEqual(NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch,
                state.Function, $"Norfair lava jumper production handoff {random:X4}");
            AssertTrue(slot.Processing.HasAny(EnemyProcessing.OffScreen),
                $"Norfair lava jumper off-screen processing {random:X4}");
            AssertEqual((SoundEffectId?)EnemySound.SplashIn, enemies.Audio.LatestSound,
                $"Norfair lava jumper sound {random:X4}");
            AssertEqual(1, advances, $"Norfair lava jumper RNG advance {random:X4}");
        }

        Console.WriteLine(
            "Norfair lava-jump definitions: four native velocities and all 65,536 real RNG selections pass with the source table forbidden.");
    }

}
