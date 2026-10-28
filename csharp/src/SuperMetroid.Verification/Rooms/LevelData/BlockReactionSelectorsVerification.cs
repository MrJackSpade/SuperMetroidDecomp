using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>Checks native block-reaction selectors for bomb shapes, reveals, and their live dispatch paths.</summary>
internal static class BlockReactionSelectorsVerification
{
    /// <summary>Matches bomb-block break and reverse-reveal draw operands to the four native shape programs.</summary>
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

    /// <summary>Checks the complete bombed-reveal physical draw domain and native run geometry.</summary>
    internal static void VerifyBombedRevealPhysicalDrawMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] nativePointers = [0xa49b,0xa4e7,0xa4ed];
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool found = RoomPlmBombedRevealDrawDefinitions.TryGet(NativePlmDraws.At<RoomPlmBombedRevealDraw>(pointer), out var draw);
            AssertEqual(nativePointers.Contains((ushort)pointer), found, "bombed reveal complete native draw pointer domain");
            if (!found)
            {
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), draw, "bombed reveal missing draw cleared");
                continue;
            }
            AssertEqual((ushort)pointer, PlmDrawAddressCodec.Of(draw.Draw), "bombed reveal draw identity");
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

    /// <summary>Checks every possible address against the bombed-reveal control and operand domain.</summary>
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
        }
    }

    /// <summary>Matches all bombed-reveal draw operands to cartridge words and the shared PLM reader.</summary>
    internal static void VerifyBombedRevealDrawMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xc8ee,0xc8f4,0xc8fa,0xc900,0xc91e,0xc924];
        foreach (ushort address in operands)
        {
            AssertTrue(RoomPlmBombedRevealProgramDefinitions.TryReadWord(address, out ushort value), "bombed reveal native draw operand owned");
            AssertEqual(ReadRomWord(rom,0x840000 | address), value, "bombed reveal named draw selection matches native operand");
        }
    }

    /// <summary>Checks contact-crumble headers for all supported BTS values and rejects unsupported indexes.</summary>
    internal static void VerifyContactCrumbleHeaderSelection(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 8; index++)
            AssertEqual(ReadRomWord(rom, 0x949139 + 2 * index),
                (ushort)RoomPlmHeaders.ContactCrumbleByReactionIndex(index), "contact crumble native header selection");
        for (int index = 8; index <= byte.MaxValue; index++)
            AssertThrows<ArgumentOutOfRangeException>(() => RoomPlmHeaders.ContactCrumbleByReactionIndex(index),
                "contact crumble header rejects unsupported BTS without masking");
        foreach (int index in new[] {int.MinValue, -1, 256, int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => RoomPlmHeaders.ContactCrumbleByReactionIndex(index),
                "contact crumble header int bounds");
    }

    /// <summary>Checks collision-triggered bomb PLM instruction selection through native headers.</summary>
    internal static void VerifyCollisionBombInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x94936b, 8,
            index => RoomPlmNativeListAddresses.BlockReactionEntry(BlockReactionPlmPrograms.CollisionBomb(index)), "collision bomb"));

    /// <summary>Checks projectile-triggered bomb reaction instruction selection through native headers.</summary>
    internal static void VerifyReactionBombInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x94a012, 8,
            index => RoomPlmNativeListAddresses.BlockReactionEntry(BlockReactionPlmPrograms.ReactionBomb(index)), "bomb reaction"));

    /// <summary>Checks crumble-reveal instruction selection for the four native sizes.</summary>
    internal static void VerifyCrumbleRevealInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949da4, 4,
            index => RoomPlmNativeListAddresses.BlockReactionEntry(BlockReactionPlmPrograms.CrumbleReveal(index)), "crumble reveal"));

    /// <summary>Checks contact-crumble instruction selection for the eight native reactions.</summary>
    internal static void VerifyContactCrumbleInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949139, 8,
            index => RoomPlmNativeListAddresses.BlockReactionEntry(BlockReactionPlmPrograms.ContactCrumble(index)), "contact crumble"));

    /// <summary>Checks the bounded bomb-special BTS domain, including adjacent area aliases.</summary>
    internal static void VerifyBombSpecialInstructionSelection(SuperMetroidAddressSpace rom)
    {
        AssertEqual(80, BombSpecialBlockReactions.Count, "bounded native normal-BTS compatibility extent");
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949da4, 80,
            index => PlmProgramStartOracle.NativeListOf(BombSpecialBlockReactions.ProgramAt(index)), "bomb special including adjacent area aliases"));
    }

    /// <summary>Compares a selector with the native header indirection and rejects indexes outside its domain.</summary>
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
            AssertThrows<ArgumentOutOfRangeException>(() => select(index), $"{label}: rejects unsupported BTS without masking");
        foreach (int index in new[] {int.MinValue, -1, 256, int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => select(index), $"{label}: int bounds");
    }
}
