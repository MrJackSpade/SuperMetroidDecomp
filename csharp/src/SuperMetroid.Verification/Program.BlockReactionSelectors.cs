using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static BabyMetroidRouteDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Represents block reaction selectors verification.</summary>
internal static class BlockReactionSelectorsVerification
{
    /// <summary>Verifies shared break animation selection.</summary>
    internal static void VerifySharedBreakAnimationSelection(SuperMetroidAddressSpace rom)
    {
        // Independent operands in the four original respawning bomb programs,
        // including their reverse reveal. These are not draw-layout base pointers.
        ushort[] firstOperands = [0xcc41,0xcc6b,0xcc97,0xccc3];
        for (int shape = 0; shape < 4; shape++)
        {
            for (int frame = 0; frame < 7; frame++)
                AssertEqual(ReadRomWord(rom,0x840000 | (firstOperands[shape] + 4 * frame)),
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

    /// <summary>Verifies bombed reveal physical draw mapping.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for bombed reveal physical draw mapping")]
    internal static void VerifyBombedRevealPhysicalDrawMapping(SuperMetroidAddressSpace rom)
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
            ushort count = ReadRomWord(rom,0x840000 | pointer);
            AssertEqual(count,run.DirectionAndCount,"bombed reveal native direction/count");
            AssertEqual(count & 0x7fff,run.LevelWords.Length,"bombed reveal native word count");
            AssertEqual(ReadRomWord(rom,0x840000 | (pointer + 2)),run.LevelWords.Span[0],
                "bombed reveal native physical parent and visual tile");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (pointer + 4))),run.Next.X,"bombed reveal native terminal X");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (pointer + 5))),run.Next.Y,"bombed reveal native terminal Y");
        }
    }

    /// <summary>Verifies bombed reveal control mapping.</summary>
    internal static void VerifyBombedRevealControlMapping(SuperMetroidAddressSpace rom)
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
            AssertEqual(ReadRomWord(rom,0x840000 | address), value, "bombed reveal native one-frame duration/delete");
            AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "bombed reveal shared control reader");
            AssertEqual(value,shared,"bombed reveal shared control value");
        }
    }

    /// <summary>Verifies bombed reveal draw mapping.</summary>
    internal static void VerifyBombedRevealDrawMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xc8ee,0xc8f4,0xc8fa,0xc900,0xc91e,0xc924];
        foreach (ushort address in operands)
        {
            AssertTrue(RoomPlmBombedRevealProgramDefinitions.TryReadWord(address, out ushort value), "bombed reveal native draw operand owned");
            AssertEqual(ReadRomWord(rom,0x840000 | address), value, "bombed reveal named draw selection matches native operand");
            AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "bombed reveal shared draw reader");
            AssertEqual(value,shared,"bombed reveal shared draw value");
        }
    }

    /// <summary>Verifies contact crumble header selection.</summary>
    internal static void VerifyContactCrumbleHeaderSelection(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 8; index++)
            AssertEqual(ReadRomWord(rom, 0x949139 + 2 * index),
                (ushort)RoomPlmHeaders.ContactCrumbleByReactionIndex(index), "contact crumble native header selection");
        for (int index = 8; index <= byte.MaxValue; index++)
            AssertThrows<IndexOutOfRangeException>(() => RoomPlmHeaders.ContactCrumbleByReactionIndex(index),
                "contact crumble header rejects unsupported BTS without masking");
        foreach (int index in new[] {int.MinValue, -1, 256, int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => RoomPlmHeaders.ContactCrumbleByReactionIndex(index),
                "contact crumble header int bounds");
    }

    /// <summary>Verifies collision bomb instruction selection.</summary>
    internal static void VerifyCollisionBombInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x94936b, 8,
            RoomPlmInstructionLists.CollisionBombByReactionIndex, "collision bomb"));

    /// <summary>Verifies reaction bomb instruction selection.</summary>
    internal static void VerifyReactionBombInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x94a012, 8,
            RoomPlmInstructionLists.ReactionBombByReactionIndex, "bomb reaction"));

    /// <summary>Verifies crumble reveal instruction selection.</summary>
    internal static void VerifyCrumbleRevealInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949da4, 4,
            RoomPlmInstructionLists.CrumbleRevealBySize, "crumble reveal"));

    /// <summary>Verifies contact crumble instruction selection.</summary>
    internal static void VerifyContactCrumbleInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949139, 8,
            RoomPlmInstructionLists.ContactCrumbleByReactionIndex, "contact crumble"));

    /// <summary>Verifies bomb special instruction selection.</summary>
    internal static void VerifyBombSpecialInstructionSelection(SuperMetroidAddressSpace rom)
    {
        AssertEqual(80, BombSpecialBlockReactions.Count, "bounded native normal-BTS compatibility extent");
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949da4, 80,
            BombSpecialBlockReactions.InstructionListAt, "bomb special including adjacent area aliases"));
    }

    /// <summary>Verifies native block instruction selection.</summary>
    private static void VerifyNativeBlockInstructionSelection(SuperMetroidAddressSpace rom,
        int nativeTable, int count, Func<int, ushort> select, string label)
    {
        for (int index = 0; index < count; index++)
        {
            // Independent cartridge header indirection, not the replacement's size/role cases.
            ushort header = ReadRomWord(rom, nativeTable + index * 2);
            ushort instruction = ReadRomWord(rom, 0x840000 | (header + 2));
            AssertEqual(instruction, select(index), $"{label}: native instruction at BTS {index:X2}");
        }
        for (int index = count; index <= byte.MaxValue; index++)
            AssertThrows<IndexOutOfRangeException>(() => select(index), $"{label}: rejects unsupported BTS without masking");
        foreach (int index in new[] {int.MinValue, -1, 256, int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => select(index), $"{label}: int bounds");
    }
}
