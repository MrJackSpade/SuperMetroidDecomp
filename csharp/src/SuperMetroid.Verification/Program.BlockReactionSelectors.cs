using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
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
