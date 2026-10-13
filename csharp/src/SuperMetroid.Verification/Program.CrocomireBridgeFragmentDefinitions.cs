using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Crocomire bridge fragment positions and the eleven-fragment spawn limit.</summary>
internal static class CrocomireBridgeFragmentDefinitionsVerification
{
    /// <summary>Compares native X positions with real allocations, movement seeds, cursor advance, and cutoff.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire bridge fragment definitions")]
    internal static void VerifyCrocomireBridgeFragmentDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa49156;
        for (ushort byteOffset = 0; byteOffset < 22; byteOffset += 2)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(sourceAddress + byteOffset);
            AssertEqual(expected, CrocomireBridgeFragmentDefinitions.XPosition(byteOffset),
                $"Crocomire bridge fragment offset ${byteOffset:X2}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireBridgeFragmentDefinitions.XPosition(1),
            "Crocomire odd bridge-fragment offset");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireBridgeFragmentDefinitions.XPosition(22),
            "Crocomire bridge-fragment offset past table");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var death = new CrocomireDeathState();
        var state = new CrocomireEnemyState(enemies.SlotTable.Slots[0]);
        enemies.BindFixtureBus(NewCrocomireBridgeFragmentReadGuard(rom));
        // $86:92A5 reads the seed without advancing it; generating one would fail here.
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0x1234);
        enemies.CrocomireBehavior.CrocomireDeath = death;
        Action spawn = enemies.CrocomireBehavior.SpawnNextCrocomireBridgeFragment;

        for (ushort fragment = 0; fragment < 11; fragment++)
        {
            spawn();
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[17 - fragment];
            AssertEqual(RoomEnemyProjectileKind.CrocomireBridgeCrumbling, projectile.Kind,
                $"Crocomire bridge fragment {fragment} kind");
            AssertEqual(CrocomireBridgeFragmentDefinitions.XPosition((ushort)(fragment * 2)),
                projectile.Position.X.Whole,
                $"Crocomire bridge fragment {fragment} production X");
            AssertEqual((ushort)187, projectile.Position.Y.Whole,
                $"Crocomire bridge fragment {fragment} production Y");
            AssertEqual((ushort)0x0074, projectile.Velocity.Y.RawValue,
                $"Crocomire bridge fragment {fragment} production Y velocity");
            AssertEqual(checked((ushort)((fragment + 1) * 2)), death.BridgeFragmentCursor,
                $"Crocomire bridge fragment {fragment} cursor");
        }

        int activeBeforeCutoff = enemies.ActiveEnemyProjectileCount;
        spawn();
        AssertEqual(activeBeforeCutoff, enemies.ActiveEnemyProjectileCount,
            "Crocomire bridge fragment cursor 22 stops spawning");
        AssertEqual((ushort)22, death.BridgeFragmentCursor,
            "Crocomire bridge fragment cutoff preserves cursor");
        Console.WriteLine(
            "Crocomire bridge fragments: all eleven native positions, eleven real allocations and the cursor cutoff pass with the source table forbidden.");
    }

    /// <summary>Rejects live reads of the compiled bridge fragment position table.</summary>
    private static CartridgeReadGuard NewCrocomireBridgeFragmentReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa49156 and < 0xa4916c,
                address => $"Crocomire bridge fragment attempted migrated position read ${address:X6}.");

    /// <summary>Runs the Crocomire bridge fragment position and spawn checks.</summary>
    [VerificationCommand("--crocomire-bridge-fragment-definitions")]
    internal static int CrocomireBridgeFragmentDefinitionsCommand()
    {
        Suite(nameof(VerifyCrocomireBridgeFragmentDefinitions), () => VerifyCrocomireBridgeFragmentDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
