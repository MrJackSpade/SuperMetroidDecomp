using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireBridgeFragmentDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa49156;
        for (ushort byteOffset = 0; byteOffset < 22; byteOffset += 2)
        {
            ushort expected = (ushort)(
                rom.ReadByte(sourceAddress + byteOffset) |
                rom.ReadByte(sourceAddress + byteOffset + 1) << 8);
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
        var state = new CrocomireEnemyState(enemies.Slots[0]);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new CrocomireBridgeFragmentReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0x1234));
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, death);
        var spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnNextCrocomireBridgeFragment", flags)!
            .CreateDelegate<Action<CrocomireEnemyState>>(enemies);

        for (ushort fragment = 0; fragment < 11; fragment++)
        {
            spawn(state);
            RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[17 - fragment];
            AssertEqual(RoomEnemyProjectileKind.CrocomireBridgeCrumbling, projectile.Kind,
                $"Crocomire bridge fragment {fragment} kind");
            AssertEqual(CrocomireBridgeFragmentDefinitions.XPosition((ushort)(fragment * 2)),
                projectile.XPosition,
                $"Crocomire bridge fragment {fragment} production X");
            AssertEqual((ushort)187, projectile.YPosition,
                $"Crocomire bridge fragment {fragment} production Y");
            AssertEqual((ushort)0x0074, projectile.YVelocity,
                $"Crocomire bridge fragment {fragment} production Y velocity");
            AssertEqual(checked((ushort)((fragment + 1) * 2)), death.BridgeFragmentCursor,
                $"Crocomire bridge fragment {fragment} cursor");
        }

        int activeBeforeCutoff = enemies.ActiveEnemyProjectileCount;
        spawn(state);
        AssertEqual(activeBeforeCutoff, enemies.ActiveEnemyProjectileCount,
            "Crocomire bridge fragment cursor 22 stops spawning");
        AssertEqual((ushort)22, death.BridgeFragmentCursor,
            "Crocomire bridge fragment cutoff preserves cursor");
        Console.WriteLine(
            "Crocomire bridge fragments: all eleven native positions, eleven real allocations and the cursor cutoff pass with the source table forbidden.");
    }

    private sealed class CrocomireBridgeFragmentReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa49156 and < 0xa4916c
            ? throw new InvalidOperationException(
                $"Crocomire bridge fragment attempted migrated position read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
