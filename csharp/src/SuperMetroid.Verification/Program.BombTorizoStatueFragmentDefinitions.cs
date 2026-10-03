using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombTorizoStatueFragmentDefinitions(
        SuperMetroidAddressSpace rom)
    {
        VerifyStatueFragmentProgramField(rom);
        VerifyStatueFragmentXField(rom);
        VerifyStatueFragmentYField(rom);
        VerifyStatueFragmentVelocityField(rom);
        VerifyStatueFragmentAccelerationField(rom);
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (ushort parameter = 1; parameter < 32; parameter += 2)
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueFragmentDefinitions.ForParameter(parameter), "statue rejects every odd parameter in the native range");
        foreach (ushort parameter in new ushort[] {32,33,0x100,0x101,0x7ffe,0x8000,0xfffe,0xffff})
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueFragmentDefinitions.ForParameter(parameter), "statue rejects parameters beyond the sixteen fragments");
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

    private static ushort ReadStatueFragmentOracleWord(ISnesAddressSpace rom, int address) =>
        (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

    private static void VerifyStatueFragmentProgramField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(ReadStatueFragmentOracleWord(rom, 0x86a7ab + parameter),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).InstructionList, "statue native program field");
    }
    private static void VerifyStatueFragmentXField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(unchecked((short)ReadStatueFragmentOracleWord(rom, 0x86a7cb + parameter)),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).XOffset, "statue native signed X field including mirrored set");
    }
    private static void VerifyStatueFragmentYField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(unchecked((short)ReadStatueFragmentOracleWord(rom, 0x86a7eb + (parameter & 15))),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).YOffset, "statue native signed Y field with eight-row wrap");
    }
    private static void VerifyStatueFragmentVelocityField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(ReadStatueFragmentOracleWord(rom, 0x86a7fb + (parameter & 15)),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).YVelocity, "statue native constant launch velocity field");
    }
    private static void VerifyStatueFragmentAccelerationField(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 32; parameter += 2)
            AssertEqual(ReadStatueFragmentOracleWord(rom, 0x86a80b + (parameter & 15)),
                BombTorizoStatueFragmentDefinitions.ForParameter(parameter).Acceleration, "statue native constant acceleration field");
    }
    private sealed class BombTorizoStatueFragmentReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address is >= 0x86a7ab and < 0x86a81b
                ? throw new InvalidOperationException(
                    $"Bomb Torizo statue fragment attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
