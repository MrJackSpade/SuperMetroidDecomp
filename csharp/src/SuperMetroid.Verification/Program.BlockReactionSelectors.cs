using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Compares all four shared breakup draw sequences with their native bank-$84 operands and checks selector bounds.</summary>
    /// <param name="rom">Retail address space containing the original breakup instruction lists.</param>
    private static void VerifySharedBreakAnimationSelection(SuperMetroidAddressSpace rom)
    {
        // Independent operands in the four original respawning bomb programs,
        // including their reverse reveal. These are not draw-layout base pointers.
        ushort[] firstOperands = [0xcc41,0xcc6b,0xcc97,0xccc3];
        for (int shape = 0; shape < 4; shape++)
        {
            for (int frame = 0; frame < 7; frame++)
                AssertEqual(ReadBotwoonInstructionWord(rom,0x840000 | (firstOperands[shape] + 4 * frame)),
                    RoomPlmBreakAnimationDefinitions.DrawForShape(shape,frame),"shared breakup native shape/frame selection");
            foreach (int frame in new[] {int.MinValue,-1,7,8,255,256,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => RoomPlmBreakAnimationDefinitions.DrawForShape(shape,frame),
                    "shared breakup rejects frames outside forward/reverse sequence");
        }
        foreach (int shape in new[] {int.MinValue,-1,4,5,255,256,int.MaxValue})
        for (int frame = 0; frame < 7; frame++)
            AssertThrows<ArgumentOutOfRangeException>(() => RoomPlmBreakAnimationDefinitions.DrawForShape(shape,frame),
                "shared breakup rejects unknown shapes");
    }

    /// <summary>Exhaustively checks the bombed-reveal draw-pointer domain and each native physical draw record.</summary>
    /// <param name="rom">Retail address space containing the draw records and their terminal offsets.</param>
    private static void VerifyBombedRevealPhysicalDrawMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] nativePointers = [0xa49b,0xa4e7,0xa4ed];
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool found = RoomPlmBombedRevealDrawDefinitions.TryGet((ushort)pointer, out var draw);
            AssertEqual(nativePointers.Contains((ushort)pointer), found, "bombed reveal complete native draw pointer domain");
            if (!found)
            {
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), draw, "bombed reveal missing draw cleared");
                continue;
            }
            AssertEqual((ushort)pointer, draw.Pointer, "bombed reveal draw identity");
            AssertEqual(1, draw.Runs.Length, "bombed reveal single native run");
            var run = draw.Runs.Span[0];
            ushort count = ReadBotwoonInstructionWord(rom,0x840000 | pointer);
            AssertEqual(count,run.DirectionAndCount,"bombed reveal native direction/count");
            AssertEqual(count & 0x7fff,run.LevelWords.Length,"bombed reveal native word count");
            AssertEqual(ReadBotwoonInstructionWord(rom,0x840000 | (pointer + 2)),run.LevelWords.Span[0],
                "bombed reveal native physical parent and visual tile");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0x840000 | (pointer + 4))),run.NextX,"bombed reveal native terminal X");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0x840000 | (pointer + 5))),run.NextY,"bombed reveal native terminal Y");
        }
    }

    /// <summary>Checks the complete bombed-reveal control-word domain against retail operands and the shared reader.</summary>
    /// <param name="rom">Retail address space used to compare control words and durations.</param>
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

    /// <summary>Confirms each bombed-reveal draw operand matches its native instruction and shared word lookup.</summary>
    /// <param name="rom">Retail address space containing the bank-$84 instruction operands.</param>
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

    /// <summary>Verifies all eight contact-crumble BTS header selections against retail data and checks invalid indices.</summary>
    /// <param name="rom">Retail address space containing the contact-crumble header table.</param>
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

    /// <summary>Checks the eight collision-bomb instruction lists selected by their native BTS table.</summary>
    /// <param name="rom">Retail address space used to follow native header indirection.</param>
    private static void VerifyCollisionBombInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x94936b, 8,
            RoomPlmInstructionLists.CollisionBombByReactionIndex, "collision bomb"));

    /// <summary>Checks the eight bomb-reaction instruction lists selected by their native BTS table.</summary>
    /// <param name="rom">Retail address space used to follow native header indirection.</param>
    private static void VerifyReactionBombInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x94a012, 8,
            RoomPlmInstructionLists.ReactionBombByReactionIndex, "bomb reaction"));

    /// <summary>Checks the four crumble-reveal instruction lists selected by the size index.</summary>
    /// <param name="rom">Retail address space used to follow native header indirection.</param>
    private static void VerifyCrumbleRevealInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949da4, 4,
            RoomPlmInstructionLists.CrumbleRevealBySize, "crumble reveal"));

    /// <summary>Checks the eight contact-crumble instruction lists selected by their reaction-table index.</summary>
    /// <param name="rom">Retail address space used to follow native header indirection.</param>
    private static void VerifyContactCrumbleInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949139, 8,
            RoomPlmInstructionLists.ContactCrumbleByReactionIndex, "contact crumble"));

    /// <summary>Checks the bounded 80-entry bomb-special selection, including adjacent area aliases.</summary>
    /// <param name="rom">Retail address space used to compare each selected instruction list.</param>
    private static void VerifyBombSpecialInstructionSelection(SuperMetroidAddressSpace rom)
    {
        AssertEqual(80, BombSpecialBlockReactions.Count, "bounded native normal-BTS compatibility extent");
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949da4, 80,
            BombSpecialBlockReactions.InstructionListAt, "bomb special including adjacent area aliases"));
    }

    /// <summary>Follows each retail header to its instruction pointer and compares the supplied selector over its supported domain.</summary>
    /// <param name="rom">Retail address space containing the selector's native header table and instruction lists.</param>
    /// <param name="nativeTable">Bus address of the table of header pointers.</param>
    /// <param name="count">Number of supported reaction indices in the table.</param>
    /// <param name="select">Production selector that returns the instruction-list pointer for an index.</param>
    /// <param name="label">Name included in assertion messages for the selected block behavior.</param>
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
