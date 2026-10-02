using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombedRevealControlMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] controls = [0xc8ec,0xc8f0,0xc8f2,0xc8f6,0xc8f8,0xc8fc,0xc8fe,0xc902,0xc91c,0xc920,0xc922,0xc926];
        ushort[] operands = [0xc8ee,0xc8f4,0xc8fa,0xc900,0xc91e,0xc924];
        var known = controls.Concat(operands).ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmBombedRevealProgramDefinitions.TryReadWord((ushort)address, out ushort value);
            AssertEqual(known.Contains((ushort)address), found, "bombed reveal complete word domain including unused-list gaps and odd bytes");
            if (!found) AssertEqual((ushort)0, value, "bombed reveal missing value cleared");
        }
        foreach (ushort address in controls)
        {
            AssertTrue(RoomPlmBombedRevealProgramDefinitions.TryReadWord(address, out ushort value), "bombed reveal native control owned");
            AssertEqual(ReadBotwoonInstructionWord(rom,0x840000 | address), value, "bombed reveal native one-frame duration/delete");
            AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "bombed reveal shared control reader");
            AssertEqual(value,shared,"bombed reveal shared control value");
        }
    }

    private static void VerifyBombedRevealDrawMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xc8ee,0xc8f4,0xc8fa,0xc900,0xc91e,0xc924];
        foreach (ushort address in operands)
        {
            AssertTrue(RoomPlmBombedRevealProgramDefinitions.TryReadWord(address, out ushort value), "bombed reveal native draw operand owned");
            AssertEqual(ReadBotwoonInstructionWord(rom,0x840000 | address), value, "bombed reveal named draw selection matches native operand");
            AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "bombed reveal shared draw reader");
            AssertEqual(value,shared,"bombed reveal shared draw value");
        }
    }

    private static void VerifyContactCrumbleHeaderSelection(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 8; index++)
            AssertEqual(ReadBotwoonInstructionWord(rom, 0x949139 + 2 * index),
                RoomPlmHeaders.ContactCrumbleByReactionIndex(index), "contact crumble native header selection");
        for (int index = 8; index <= byte.MaxValue; index++)
            AssertThrows<IndexOutOfRangeException>(() => RoomPlmHeaders.ContactCrumbleByReactionIndex(index),
                "contact crumble header rejects unsupported BTS without masking");
        foreach (int index in new[] {int.MinValue, -1, 256, int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => RoomPlmHeaders.ContactCrumbleByReactionIndex(index),
                "contact crumble header int bounds");
    }

    private static void VerifyCollisionBombInstructionSelection(SuperMetroidAddressSpace rom) =>
        VerifyNativeBlockInstructionSelection(rom, 0x94936b, 8,
            RoomPlmInstructionLists.CollisionBombByReactionIndex, "collision bomb");

    private static void VerifyReactionBombInstructionSelection(SuperMetroidAddressSpace rom) =>
        VerifyNativeBlockInstructionSelection(rom, 0x94a012, 8,
            RoomPlmInstructionLists.ReactionBombByReactionIndex, "bomb reaction");

    private static void VerifyCrumbleRevealInstructionSelection(SuperMetroidAddressSpace rom) =>
        VerifyNativeBlockInstructionSelection(rom, 0x949da4, 4,
            RoomPlmInstructionLists.CrumbleRevealBySize, "crumble reveal");

    private static void VerifyContactCrumbleInstructionSelection(SuperMetroidAddressSpace rom) =>
        VerifyNativeBlockInstructionSelection(rom, 0x949139, 8,
            RoomPlmInstructionLists.ContactCrumbleByReactionIndex, "contact crumble");

    private static void VerifyBombSpecialInstructionSelection(SuperMetroidAddressSpace rom)
    {
        AssertEqual(80, BombSpecialBlockReactions.Count, "bounded native normal-BTS compatibility extent");
        VerifyNativeBlockInstructionSelection(rom, 0x949da4, 80,
            BombSpecialBlockReactions.InstructionListAt, "bomb special including adjacent area aliases");
    }

    private static void VerifyNativeBlockInstructionSelection(SuperMetroidAddressSpace rom,
        int nativeTable, int count, Func<int, ushort> select, string label)
    {
        for (int index = 0; index < count; index++)
        {
            // Independent cartridge header indirection, not the replacement's size/role cases.
            ushort header = ReadBotwoonInstructionWord(rom, nativeTable + index * 2);
            ushort instruction = ReadBotwoonInstructionWord(rom, 0x840000 | (header + 2));
            AssertEqual(instruction, select(index), $"{label}: native instruction at BTS {index:X2}");
        }
        for (int index = count; index <= byte.MaxValue; index++)
            AssertThrows<IndexOutOfRangeException>(() => select(index), $"{label}: rejects unsupported BTS without masking");
        foreach (int index in new[] {int.MinValue, -1, 256, int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => select(index), $"{label}: int bounds");
    }
}
