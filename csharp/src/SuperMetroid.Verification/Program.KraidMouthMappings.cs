using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidMouthShapeCases(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        for (int entry = 0; entry < 8; entry++)
        {
            ushort pointer = (ushort)(0x9788 + 8 * entry);
            int address = 0xa70000 | pointer;
            var expected = (Word(address), Word(address + 2), Word(address + 4), Word(address + 6));
            AssertTrue(KraidMouthHitboxes.IsDefined(pointer), "Native mouth shape is defined");
            AssertEqual(expected, KraidMouthHitboxes.Resolve(pointer), "Native mouth shape case");
            AssertEqual((expected.Item1, expected.Item2, expected.Item4),
                KraidMouthHitboxes.ResolveCollision(rom, pointer), "Collision uses left/top/bottom only");
        }
        for (int raw = 0x9787; raw <= 0x97c8; raw++)
        {
            ushort pointer = (ushort)raw;
            bool expected = raw >= 0x9788 && raw <= 0x97c0 && (raw - 0x9788) % 8 == 0;
            AssertEqual(expected, KraidMouthHitboxes.IsDefined(pointer), "Native shape alignment");
            if (!expected)
                AssertThrows<InvalidDataException>(() => KraidMouthHitboxes.Resolve(pointer), "Non-shape address rejects");
        }
    }

    private static void VerifyKraidLowHalfBoundaryMapping(SuperMetroidAddressSpace rom)
    {
        AssertEqual(7, KraidMouthHitboxes.LowHalfBoundaryLength, "Native compatibility window length");
        for (int index = 0; index < 7; index++)
            AssertEqual(rom.ReadByte(0xa78000 + index), KraidMouthHitboxes.LowHalfBoundaryByte(index),
                "Native instruction byte in low-half compatibility window");
        AssertThrows<IndexOutOfRangeException>(() => KraidMouthHitboxes.LowHalfBoundaryByte(-1), "Negative boundary byte");
        AssertThrows<IndexOutOfRangeException>(() => KraidMouthHitboxes.LowHalfBoundaryByte(7), "Past boundary byte");
        var bus = new KraidMouthBoundaryReadBus();
        byte ExpectedByte(ushort pointer) => pointer < 0x8000
            ? KraidMouthBoundaryReadBus.Value(pointer) : rom.ReadByte(0xa70000 | pointer);
        short ExpectedWord(ushort pointer) => unchecked((short)(ExpectedByte(pointer) | ExpectedByte((ushort)(pointer + 1)) << 8));
        for (ushort pointer = 0x7ff9; pointer <= 0x7fff; pointer++)
            AssertEqual((ExpectedWord(pointer), ExpectedWord((ushort)(pointer + 2)), ExpectedWord((ushort)(pointer + 6))),
                KraidMouthHitboxes.ResolveCollision(bus, pointer), "Mouth boundary crossing uses original bytes");
        for (ushort pointer = 0x7ffd; pointer <= 0x7fff; pointer++)
            AssertEqual(unchecked((ushort)ExpectedWord((ushort)(pointer + 2))),
                KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(bus, pointer), "Head boundary crossing uses original bytes");
    }
}
