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
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static EnemyDropsVerification;

internal static class EnemyDropChanceDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for enemy drop chance definitions")]
    internal static void VerifyEnemyDropChanceDefinitions(SuperMetroidAddressSpace rom)
    {
        Span<byte> actual = stackalloc byte[EnemyDropChanceDefinitions.RecordSize];
        for (int record = 0; record < EnemyDropChanceDefinitions.RecordCount; record++)
        {
            ushort pointer = checked((ushort)(
                EnemyDropChanceDefinitions.FirstPointer +
                record * EnemyDropChanceDefinitions.RecordSize));
            EnemyDropChanceDefinitions.Copy(pointer, actual);
            for (int field = 0; field < actual.Length; field++)
            {
                int address = EnemyDropChanceDefinitions.NativeBank |
                    unchecked((ushort)(pointer + field));
                AssertEqual(rom.ReadCartridgeByte(address), actual[field],
                    $"enemy drop record ${pointer:X4} byte {field}");
            }
        }

        AssertThrows<InvalidDataException>(
            () => EnemyDropChanceDefinitions.Copy(
                unchecked((ushort)(EnemyDropChanceDefinitions.FirstPointer - 1)), new byte[6]),
            "enemy drop pointer below table is rejected");
        AssertThrows<InvalidDataException>(
            () => EnemyDropChanceDefinitions.Copy(
                unchecked((ushort)(EnemyDropChanceDefinitions.FirstPointer + 1)), new byte[6]),
            "unaligned enemy drop pointer is rejected");
        AssertThrows<InvalidDataException>(
            () => EnemyDropChanceDefinitions.Copy(
                unchecked((ushort)(EnemyDropChanceDefinitions.LastPointer +
                    EnemyDropChanceDefinitions.RecordSize)), new byte[6]),
            "enemy drop pointer above table is rejected");
        AssertThrows<ArgumentException>(
            () => EnemyDropChanceDefinitions.Copy(
                EnemyDropChanceDefinitions.FirstPointer, new byte[5]),
            "short enemy drop destination");

        var samus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
        };
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1]);
        typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(fixture.System, SuperMetroidAddressSpace.CreateWithoutCartridge());
        var projectile = new RoomEnemyProjectileSlot(1);

        for (int record = 0; record < EnemyDropChanceDefinitions.RecordCount; record++)
        {
            ushort pointer = checked((ushort)(
                EnemyDropChanceDefinitions.FirstPointer +
                record * EnemyDropChanceDefinitions.RecordSize));
            projectile.ItemDropChancesPointerOverride = pointer;
            EnemyPickupKind selected = fixture.System.SelectRandomEnemyDrop(projectile);
            AssertTrue(Enum.IsDefined(selected),
                $"production enemy drop record ${pointer:X4} returns a defined kind");
        }

        projectile.ItemDropChancesPointerOverride = unchecked((ushort)(
            EnemyDropChanceDefinitions.FirstPointer + 1));
        AssertThrows<InvalidDataException>(
            () => fixture.System.SelectRandomEnemyDrop(projectile),
            "production enemy drop rejects an unaligned restored probability pointer");
        projectile.ItemDropChancesPointerOverride = 0x8000;
        AssertThrows<InvalidDataException>(
            () => fixture.System.SelectRandomEnemyDrop(projectile),
            "production enemy drop rejects an external restored probability pointer");
        Console.WriteLine(
            "Enemy drop chance definitions: all 118 native records match ROM and execute " +
            "through production selection with RAM-only memory; invalid pointers reject without a ROM fallback.");
    }

    [VerificationCommand("--enemy-drop-chance-definitions")]
    internal static int EnemyDropChanceDefinitionsCommand()
    {
        Suite(nameof(VerifyEnemyDropChanceDefinitions), () => VerifyEnemyDropChanceDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
