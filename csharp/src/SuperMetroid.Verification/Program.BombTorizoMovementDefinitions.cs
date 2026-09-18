using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombTorizoMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int standingXAddress = 0xaac3ee;
        const int standingYAddress = 0xaac40e;
        const int sittingXAddress = 0xaac440;
        const int sittingYAddress = 0xaac460;
        const int normalWalkAddress = 0xaac4bd;
        const int facelessWalkAddress = 0xaac532;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        for (ushort tableOffset = 0; tableOffset < 32; tableOffset += 2)
        {
            BombTorizoPostureDisplacement displacement =
                BombTorizoMovementDefinitions.Posture(tableOffset);
            int wrappedYOffset = tableOffset & 0x000f;

            AssertEqual(unchecked((short)ReadWord(rom, standingXAddress + tableOffset)),
                displacement.X,
                $"Bomb Torizo standing X offset ${tableOffset:X2}");
            AssertEqual(unchecked((short)ReadWord(rom, standingYAddress + wrappedYOffset)),
                displacement.Y,
                $"Bomb Torizo standing Y offset ${tableOffset:X2}");
            AssertEqual(unchecked((short)ReadWord(rom, sittingXAddress + tableOffset)),
                displacement.X,
                $"Bomb Torizo sitting X offset ${tableOffset:X2}");
            AssertEqual(unchecked((short)ReadWord(rom, sittingYAddress + wrappedYOffset)),
                displacement.Y,
                $"Bomb Torizo sitting Y offset ${tableOffset:X2}");
        }

        for (ushort tableOffset = 0; tableOffset < 40; tableOffset += 2)
        {
            ushort velocity = BombTorizoMovementDefinitions.WalkVelocity(tableOffset);
            AssertEqual(ReadWord(rom, normalWalkAddress + tableOffset), velocity,
                $"Bomb Torizo normal walk velocity ${tableOffset:X2}");
            AssertEqual(ReadWord(rom, facelessWalkAddress + tableOffset), velocity,
                $"Bomb Torizo faceless walk velocity ${tableOffset:X2}");
        }

        AssertThrows<InvalidDataException>(
            () => BombTorizoMovementDefinitions.Posture(1),
            "Bomb Torizo odd posture offset");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovementDefinitions.Posture(32),
            "Bomb Torizo posture offset outside table");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovementDefinitions.WalkVelocity(1),
            "Bomb Torizo odd walk offset");
        AssertThrows<InvalidDataException>(
            () => BombTorizoMovementDefinitions.WalkVelocity(40),
            "Bomb Torizo walk offset outside table");

        BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        MethodInfo applyPosture = typeof(RoomEnemySystem).GetMethod(
            "ApplyBombTorizoMapOffset",
            staticFlags)!;

        for (ushort tableOffset = 0; tableOffset < 32; tableOffset += 2)
        foreach (bool subtract in new[] { false, true })
        {
            BombTorizoPostureDisplacement expected =
                BombTorizoMovementDefinitions.Posture(tableOffset);
            var slot = new RoomEnemySlot(0)
            {
                XPosition = 1000,
                YPosition = 500,
            };

            applyPosture.Invoke(null, [slot, tableOffset, subtract]);
            int sign = subtract ? -1 : 1;
            AssertEqual(unchecked((ushort)(1000 + sign * expected.X)), slot.XPosition,
                $"Bomb Torizo posture X ${tableOffset:X2} subtract {subtract}");
            AssertEqual(unchecked((ushort)(500 + sign * expected.Y)), slot.YPosition,
                $"Bomb Torizo posture Y ${tableOffset:X2} subtract {subtract}");
        }

        var guarded = new BombTorizoMovementReadGuard(rom);
        var enemies = new RoomEnemySystem();
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guarded);
        MethodInfo processWalk = typeof(RoomEnemySystem).GetMethod(
            "ProcessBombTorizoWalkInstruction",
            instanceFlags)!;
        var level = new RoomLevelData(
            64,
            64,
            new ushort[4096],
            new byte[4096],
            new ushort[4096],
            new byte[8]);

        for (ushort tableOffset = 0; tableOffset < 40; tableOffset += 2)
        {
            var slot = new RoomEnemySlot(0)
            {
                XPosition = 512,
                YPosition = 512,
                XRadius = 8,
                YRadius = 8,
            };
            var state = new TorizoEnemyState(slot, isGolden: false);
            const ushort cursor = 0x9000;
            ushort next = (ushort)processWalk.Invoke(
                enemies,
                [slot, state, null, level, cursor, tableOffset, (ushort)0xb962, (ushort)0xbdd8])!;

            AssertEqual(BombTorizoMovementDefinitions.WalkVelocity(tableOffset),
                state.HorizontalVelocity,
                $"Bomb Torizo walk consumer velocity ${tableOffset:X2}");
            AssertEqual(unchecked((ushort)(cursor + 4)), next,
                $"Bomb Torizo walk consumer continuation ${tableOffset:X2}");
        }

        Console.WriteLine(
            "Bomb Torizo movement definitions: all 88 native words, 32 real posture applications and 20 real walk consumers pass with all six source tables forbidden.");
    }

    private sealed class BombTorizoMovementReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xaac3ee and < 0xaac41e or
                >= 0xaac440 and < 0xaac470 or
                >= 0xaac4bd and < 0xaac4e5 or
                >= 0xaac532 and < 0xaac55a
                ? throw new InvalidOperationException(
                    $"Bomb Torizo movement attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
