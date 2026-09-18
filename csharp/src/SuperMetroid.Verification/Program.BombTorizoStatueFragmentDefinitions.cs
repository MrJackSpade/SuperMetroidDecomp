using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombTorizoStatueFragmentDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const int instructionAddress = 0x86a7ab;
        const int xOffsetAddress = 0x86a7cb;
        const int yOffsetAddress = 0x86a7eb;
        const int yVelocityAddress = 0x86a7fb;
        const int accelerationAddress = 0x86a80b;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        for (ushort index = 0; index < 16; index++)
        {
            ushort parameter = unchecked((ushort)(index * 2));
            int row = index & 7;
            BombTorizoStatueFragmentDefinition definition =
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter);
            AssertEqual(ReadWord(rom, instructionAddress + index * 2),
                definition.InstructionList,
                $"Bomb Torizo statue fragment {index} instruction");
            AssertEqual(unchecked((short)ReadWord(rom, xOffsetAddress + index * 2)),
                definition.XOffset,
                $"Bomb Torizo statue fragment {index} X offset");
            AssertEqual(unchecked((short)ReadWord(rom, yOffsetAddress + row * 2)),
                definition.YOffset,
                $"Bomb Torizo statue fragment {index} Y offset");
            AssertEqual(ReadWord(rom, yVelocityAddress + row * 2),
                definition.YVelocity,
                $"Bomb Torizo statue fragment {index} Y velocity");
            AssertEqual(ReadWord(rom, accelerationAddress + row * 2),
                definition.Acceleration,
                $"Bomb Torizo statue fragment {index} acceleration");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => BombTorizoStatueFragmentDefinitions.ForParameter(1),
            "Bomb Torizo statue odd fragment parameter");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BombTorizoStatueFragmentDefinitions.ForParameter(0x20),
            "Bomb Torizo statue high fragment parameter");

        var guarded = new BombTorizoStatueFragmentReadGuard(rom);
        for (ushort index = 0; index < 16; index++)
        {
            ushort parameter = unchecked((ushort)(index * 2));
            BombTorizoStatueFragmentDefinition expected =
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter);
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
                .SetValue(enemies, guarded);
            RoomEnemyProjectileSlot projectile =
                enemies.SpawnBombTorizoStatueBreakingProjectile(
                    new BombTorizoStatueProjectileRequest(
                        BombTorizoStatueFragmentDefinitions.ProjectileDefinition,
                        parameter,
                        PlmBlockX: 10,
                        PlmBlockY: 20)) ??
                throw new InvalidDataException(
                    $"Bomb Torizo statue fragment {index} failed to allocate.");

            AssertEqual(RoomEnemyProjectileKind.BombTorizoStatueBreaking,
                projectile.Kind,
                $"Bomb Torizo statue fragment {index} projectile kind");
            AssertEqual(expected.InstructionList, projectile.InstructionPointer,
                $"Bomb Torizo statue fragment {index} production instruction");
            AssertEqual(unchecked((ushort)(10 * 16 + expected.XOffset)),
                projectile.XPosition,
                $"Bomb Torizo statue fragment {index} production X");
            AssertEqual(unchecked((ushort)(20 * 16 + expected.YOffset)),
                projectile.YPosition,
                $"Bomb Torizo statue fragment {index} production Y");
            AssertEqual(expected.YVelocity, projectile.YVelocity,
                $"Bomb Torizo statue fragment {index} production velocity");
            AssertEqual(expected.Acceleration, projectile.Variable1,
                $"Bomb Torizo statue fragment {index} production acceleration");
        }

        Console.WriteLine(
            "Bomb Torizo statue fragments: 56 native source words and all sixteen real room-graphics projectile allocations pass with the five physical tables forbidden.");
    }

    private sealed class BombTorizoStatueFragmentReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x86a7ab and < 0x86a81b
                ? throw new InvalidOperationException(
                    $"Bomb Torizo statue fragment attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
