using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream1(ISnesAddressSpace rom)
    {
        VerifyLookupStream1EnemyMovement(rom);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort medium = 0; medium < 3; medium++)
        {
            for (int variant = 0; variant < 4; variant++)
            {
                var actual = SamusVerticalMotionDefinitions.Launch(medium, (variant & 1) != 0, (variant & 2) != 0);
                int address = 0x909eb9 + 12 * variant + 2 * medium;
                AssertEqual(Word(address), actual.Whole, "stream1 native jump whole speed");
                AssertEqual(Word(address + 6), actual.Fraction, "stream1 native jump fractional speed");
            }
            var hurt = SamusVerticalMotionDefinitions.Knockback(medium);
            AssertEqual(Word(0x909ee9 + 2 * medium), hurt.Whole, "stream1 native knockback whole speed");
            AssertEqual(Word(0x909eef + 2 * medium), hurt.Fraction, "stream1 native knockback fractional speed");
            var bomb = SamusVerticalMotionDefinitions.BombJump(medium);
            AssertEqual(Word(0x909ef5 + 2 * medium), bomb.Whole, "stream1 native bomb whole speed");
            AssertEqual(Word(0x909efb + 2 * medium), bomb.Fraction, "stream1 native bomb fractional speed");
            var gravity = SamusVerticalMotionDefinitions.Gravity(medium);
            AssertEqual(Word(0x909ea7 + 2 * medium), gravity.Whole, "stream1 native whole gravity");
            AssertEqual(Word(0x909ea1 + 2 * medium), gravity.Fraction, "stream1 native fractional gravity");
        }
        foreach (ushort invalid in new ushort[] { 3, ushort.MaxValue })
        {
            for (int variant = 0; variant < 4; variant++)
                AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Launch(invalid, (variant & 1) != 0, (variant & 2) != 0), "stream1 invalid launch medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Knockback(invalid), "stream1 invalid hurt medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.BombJump(invalid), "stream1 invalid bomb medium");
            AssertThrows<IndexOutOfRangeException>(() => SamusVerticalMotionDefinitions.Gravity(invalid), "stream1 invalid gravity medium");
        }
        for (int address = 0x90c254; address <= 0x90c28e; address++)
            AssertEqual(rom.ReadByte(address), SamusProjectileCooldownDefinitions.ReadByte(address), "stream1 all native cooldown bytes including padding");
        AssertEqual(rom.ReadByte(0x90c291), SamusProjectileCooldownDefinitions.ReadByte(0x90c291), "stream1 bounded spacetime beam cooldown");
        foreach (int invalid in new[] { int.MinValue, 0x90c253, 0x90c28f, 0x90c290, 0x90c292, int.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusProjectileCooldownDefinitions.ReadByte(invalid), "stream1 invalid cooldown address");
    }
    private static void VerifyLookupStream1EnemyMovement(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort index = 0; index < 13; index++)
        {
            var intervals = BullMovementDefinitions.Intervals(index);
            AssertEqual(Word(0xa8d895 + 4 * index), intervals.Acceleration, "stream1 Bull acceleration interval");
            AssertEqual(Word(0xa8d897 + 4 * index), intervals.Deceleration, "stream1 Bull deceleration interval");
        }
        for (int direction = 0; direction < 10; direction++)
            AssertEqual(Word(0xa8d871 + 2 * direction), BullMovementDefinitions.ShotAngle(direction), "stream1 Bull compass angle");
        foreach (int invalid in new[] { -1, 10, int.MaxValue })
            AssertThrows<InvalidDataException>(() => BullMovementDefinitions.ShotAngle(invalid), "stream1 Bull invalid direction");
        AssertThrows<InvalidDataException>(() => BullMovementDefinitions.Intervals(13), "stream1 Bull invalid interval");
        for (byte index = 0; index < 6; index++)
            AssertEqual(Word(0xa29f36 + 2 * index), CacatacMovementDefinitions.TravelDistance(index), "stream1 Cacatac patrol blocks");
        foreach (byte invalid in new byte[] { 6, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacMovementDefinitions.TravelDistance(invalid), "stream1 Cacatac invalid patrol selector");
        for (ushort direction = 0; direction < 20; direction += 2)
            AssertEqual(Word(0x86d96a + direction), CacatacProjectileDefinitions.InstructionList((CacatacSpikeDirection)direction), "stream1 Cacatac named direction program");
        foreach (ushort invalid in new ushort[] { 1, 19, 20, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacProjectileDefinitions.InstructionList((CacatacSpikeDirection)invalid), "stream1 Cacatac invalid direction selector");
    }
}
