using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Crocomire bridge fragment positions and the eleven-fragment spawn limit.</summary>
internal static class CrocomireBridgeFragmentDefinitionsVerification
{
    /// <summary>Compares native X positions with real allocations, movement seeds, cursor advance, and cutoff.</summary>
    internal static void VerifyCrocomireBridgeFragmentDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa49156;
        // The cartridge table holds one X word per fragment, in launch order.
        for (int fragment = 0; fragment < CrocomireBridgeFragmentDefinitions.FragmentCount; fragment++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(sourceAddress + fragment * sizeof(ushort));
            AssertEqual(expected, CrocomireBridgeFragmentDefinitions.XPosition(fragment),
                $"Crocomire bridge fragment {fragment}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireBridgeFragmentDefinitions.XPosition(-1),
            "Crocomire bridge fragment before the first");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireBridgeFragmentDefinitions.XPosition(CrocomireBridgeFragmentDefinitions.FragmentCount),
            "Crocomire bridge fragment past the last");

        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var death = new CrocomireDeathState();
        var state = new CrocomireEnemyState(enemies.SlotTable.Slots[0]);
        // $86:92A5 reads the seed without advancing it; generating one would fail here.
        enemies.VerificationServices().ReadRandom = () => 0x1234;
        enemies.CrocomireBehavior.Access.CrocomireDeath = death;
        Action spawn = enemies.CrocomireBehavior.BridgeCollapse.SpawnNextCrocomireBridgeFragment;

        for (ushort fragment = 0; fragment < 11; fragment++)
        {
            spawn();
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[17 - fragment];
            AssertEqual(RoomEnemyProjectileKind.CrocomireBridgeCrumbling, projectile.Kind,
                $"Crocomire bridge fragment {fragment} kind");
            AssertEqual(CrocomireBridgeFragmentDefinitions.XPosition(fragment),
                projectile.Position.X.Whole,
                $"Crocomire bridge fragment {fragment} production X");
            AssertEqual((ushort)187, projectile.Position.Y.Whole,
                $"Crocomire bridge fragment {fragment} production Y");
            AssertEqual((ushort)0x0074, projectile.Velocity.Y.RawValue,
                $"Crocomire bridge fragment {fragment} production Y velocity");
            AssertEqual(fragment + 1, death.BridgeFragmentsSpawned,
                $"Crocomire bridge fragment {fragment} launch count");
        }

        int activeBeforeCutoff = enemies.ActiveEnemyProjectileCount;
        spawn();
        AssertEqual(activeBeforeCutoff, enemies.ActiveEnemyProjectileCount,
            "Crocomire bridge fragment cursor 22 stops spawning");
        AssertEqual(CrocomireBridgeFragmentDefinitions.FragmentCount, death.BridgeFragmentsSpawned,
            "Crocomire bridge fragment cutoff preserves the launch count");
        Console.WriteLine(
            "Crocomire bridge fragments: all eleven native positions, eleven real allocations and the cursor cutoff pass with the source table forbidden.");
    }


    /// <summary>Runs the Crocomire bridge fragment position and spawn checks.</summary>
    [VerificationCommand("--crocomire-bridge-fragment-definitions")]
    internal static int CrocomireBridgeFragmentDefinitionsCommand()
    {
        Suite(nameof(VerifyCrocomireBridgeFragmentDefinitions), () => VerifyCrocomireBridgeFragmentDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
