using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEnemyDropChanceDefinitions(SuperMetroidAddressSpace rom)
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
                AssertEqual(rom.ReadByte(address), actual[field],
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
        var guard = new EnemyDropChanceReadGuard(fixture.Bus);
        typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(fixture.System, guard);
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

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production enemy drop selection never reads the compiled native table");

        projectile.ItemDropChancesPointerOverride = unchecked((ushort)(
            EnemyDropChanceDefinitions.FirstPointer + 1));
        AssertThrows<InvalidDataException>(
            () => fixture.System.SelectRandomEnemyDrop(projectile),
            "production enemy drop rejects an unaligned restored probability pointer");
        projectile.ItemDropChancesPointerOverride = 0x8000;
        AssertThrows<InvalidDataException>(
            () => fixture.System.SelectRandomEnemyDrop(projectile),
            "production enemy drop rejects an external restored probability pointer");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "invalid production enemy drop pointers fail without a bank-$B4 fallback read");
        Console.WriteLine(
            "Enemy drop chance definitions: all 118 native records match ROM and execute " +
            "through production selection with the source table forbidden; invalid pointers reject without a ROM fallback.");
    }

    private sealed class EnemyDropChanceReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address is >= 0xb4f1f4 and < 0xb4f4b8)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Enemy drop selection attempted migrated chance read ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
