using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>Checks native block-reaction selectors for bomb shapes, reveals, and their live dispatch paths.</summary>
internal static class BlockReactionSelectorsVerification
{
    /// <summary>Matches bomb-block break and reverse-reveal draw operands to the four native shape programs.</summary>
    internal static void VerifySharedBreakAnimationSelection(CartridgeImportAddressSpace rom)
    {
        // Independent operands in the four original respawning bomb programs,
        // including their reverse reveal. These are not draw-layout base pointers.
        ushort[] firstOperands = [0xcc41,0xcc6b,0xcc97,0xccc3];
        for (int shape = 0; shape < 4; shape++)
        {
            for (int frame = 0; frame < 7; frame++)
                AssertEqual(ReadRomWord(rom,0x840000 | (firstOperands[shape] + 4 * frame)),
                    RoomObjectBreakAnimationDefinitions.DrawForShape(shape,frame),"shared breakup native shape/frame selection");
            foreach (int frame in new[] {int.MinValue,-1,7,8,255,256,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => RoomObjectBreakAnimationDefinitions.DrawForShape(shape,frame),
                    "shared breakup rejects frames outside forward/reverse sequence");
        }
        foreach (int shape in new[] {int.MinValue,-1,4,5,255,256,int.MaxValue})
        for (int frame = 0; frame < 7; frame++)
            AssertThrows<ArgumentOutOfRangeException>(() => RoomObjectBreakAnimationDefinitions.DrawForShape(shape,frame),
                "shared breakup rejects unknown shapes");
    }

    /// <summary>Checks the complete bombed-reveal physical draw domain and native run geometry.</summary>
    internal static void VerifyBombedRevealPhysicalDrawMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] nativePointers = [0xa49b,0xa4e7,0xa4ed];
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool found = BombedRevealObjectDraws.TryGet(NativeObjectDraws.At<RoomObjectBombedRevealDraw>(pointer), out var draw);
            AssertEqual(nativePointers.Contains((ushort)pointer), found, "bombed reveal complete native draw pointer domain");
            if (!found)
            {
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), draw, "bombed reveal missing draw cleared");
                continue;
            }
            AssertEqual((ushort)pointer, RoomObjectDrawAddressCodec.Of(draw.Draw), "bombed reveal draw identity");
            AssertEqual(1, draw.Runs.Length, "bombed reveal single native run");
            var run = draw.Runs.Span[0];
            ushort count = ReadRomWord(rom,0x840000 | pointer);
            AssertRunHeader(count,run,"bombed reveal native direction/count");
            AssertEqual(count & 0x7fff,run.Count,"bombed reveal native word count");
            // What the reveal shows is its installed picture, which RevealAndWallVisualsVerification compares with the cartridge.
            AssertEqual(RoomLevelWordCodec.Decode(ReadRomWord(rom,0x840000 | (pointer + 2))).Collision,run.Collisions.Span[0],
                "bombed reveal native physical parent");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (pointer + 4))),RoomObjectRunContinuationCodecConstants.X(draw, 0),"bombed reveal native terminal X");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (pointer + 5))),RoomObjectRunContinuationCodecConstants.Y(draw, 0),"bombed reveal native terminal Y");
        }
    }

    /// <summary>Checks every possible address against the bombed-reveal control and operand domain.</summary>
    internal static void VerifyBombedRevealControlMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] controls = [0xc8ec,0xc8f0,0xc8f2,0xc8f6,0xc8f8,0xc8fc,0xc8fe,0xc902,0xc91c,0xc920,0xc922,0xc926];
        ushort[] operands = [0xc8ee,0xc8f4,0xc8fa,0xc900,0xc91e,0xc924];
        var known = controls.Concat(operands).ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomObjectBombedRevealProgramDefinitions.TryReadWord((ushort)address, out ushort value);
            AssertEqual(known.Contains((ushort)address), found, "bombed reveal complete word domain including unused-list gaps and odd bytes");
            if (!found) AssertEqual((ushort)0, value, "bombed reveal missing value cleared");
        }
        foreach (ushort address in controls)
        {
            AssertTrue(RoomObjectBombedRevealProgramDefinitions.TryReadWord(address, out ushort value), "bombed reveal native control owned");
            AssertEqual(ReadRomWord(rom,0x840000 | address), value, "bombed reveal native one-frame duration/delete");
        }
    }

    /// <summary>Matches all bombed-reveal draw operands to cartridge words and the shared PLM reader.</summary>
    internal static void VerifyBombedRevealDrawMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] operands = [0xc8ee,0xc8f4,0xc8fa,0xc900,0xc91e,0xc924];
        foreach (ushort address in operands)
        {
            AssertTrue(RoomObjectBombedRevealProgramDefinitions.TryReadWord(address, out ushort value), "bombed reveal native draw operand owned");
            AssertEqual(ReadRomWord(rom,0x840000 | address), value, "bombed reveal named draw selection matches native operand");
        }
    }

    /// <summary>
    /// Checks that exactly the first eight common special-block reactions select crumble blocks, that each selects the
    /// cartridge's header for its BTS, and that an undefined crumble block is rejected.
    /// </summary>
    internal static void VerifyContactCrumbleHeaderSelection(CartridgeImportAddressSpace rom)
    {
        // Header identities are arbitrary values since #1452, so the cartridge's header pointer is decoded
        // through the native header table and compared as an identity.
        for (int index = 0; index <= BlockReaction.LastEntry; index++)
        {
            bool selected = BlockReactionDefinitions.ContactCrumbles.TryGetMeaning(BlockReaction.Common(index), out ContactCrumbleBlock crumble);
            AssertEqual(index < 8, selected, $"common special-block BTS {index:X2} selects a crumble block exactly when it is one of the first eight");
            if (selected)
                AssertEqual(RoomObjectNativeWords.Header(ReadRomWord(rom, 0x949139 + 2 * index)),
                    RoomObjectHeaders.ContactCrumbleOf(crumble), $"contact crumble native header selection at BTS {index:X2}");
            AssertTrue(!BlockReactionDefinitions.ContactCrumbles.TryGetMeaning(BlockReaction.AreaSpecific(index), out _),
                $"area special-block reaction {index} selects no crumble block");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => RoomObjectHeaders.ContactCrumbleOf((ContactCrumbleBlock)(-1)),
            "contact crumble header rejects an undefined crumble block");
        AssertThrows<ArgumentOutOfRangeException>(() => BlockReactionChanges.ContactCrumble((ContactCrumbleBlock)(-1)),
            "contact crumble program rejects an undefined crumble block");
    }

    /// <summary>The crumble block common special-block reaction <paramref name="index"/> selects, rejecting any other index.</summary>
    private static ContactCrumbleBlock ContactCrumbleAt(int index) =>
        index is >= 0 and <= BlockReaction.LastEntry &&
            BlockReactionDefinitions.ContactCrumbles.TryGetMeaning(BlockReaction.Common(index), out ContactCrumbleBlock crumble)
            ? crumble
            : throw new ArgumentOutOfRangeException(nameof(index), index, "The reaction selects no crumble block.");

    /// <summary>Checks collision-triggered bomb PLM instruction selection through native headers.</summary>
    internal static void VerifyCollisionBombInstructionSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x94936b, 8,
            index => RoomObjectNativeListAddresses.BlockReactionEntry(BlockReactionChanges.CollisionBomb(index)), "collision bomb"));

    /// <summary>Checks projectile-triggered bomb reaction instruction selection through native headers.</summary>
    internal static void VerifyReactionBombInstructionSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x94a012, 8,
            index => RoomObjectNativeListAddresses.BlockReactionEntry(BlockReactionChanges.ReactionBomb(index)), "bomb reaction"));

    /// <summary>Checks crumble-reveal instruction selection for the four native sizes.</summary>
    internal static void VerifyCrumbleRevealInstructionSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949da4, 4,
            index => RoomObjectNativeListAddresses.BlockReactionEntry(BlockReactionChanges.CrumbleReveal(index)), "crumble reveal"));

    /// <summary>Checks contact-crumble instruction selection for the eight native reactions.</summary>
    internal static void VerifyContactCrumbleInstructionSelection(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949139, 8,
            index => RoomObjectNativeListAddresses.BlockReactionEntry(BlockReactionChanges.ContactCrumble(ContactCrumbleAt(index))), "contact crumble"));

    /// <summary>Checks the bounded bomb-special BTS domain, including adjacent area aliases.</summary>
    internal static void VerifyBombSpecialInstructionSelection(CartridgeImportAddressSpace rom)
    {
        AssertEqual(80, BombSpecialBlockReactions.Count, "bounded native normal-BTS compatibility extent");
        Suite(nameof(VerifyNativeBlockInstructionSelection), () => VerifyNativeBlockInstructionSelection(rom, 0x949da4, 80,
            index => BombSpecialNativeList(BombSpecialBlockReactions.ChangeAt(index)), "bomb special including adjacent area aliases"));
    }

    /// <summary>The native list a bomb-special reaction stands for: the shared delete, the speed reveal or a crumble reveal.</summary>
    private static ushort BombSpecialNativeList(BlockReactionChange change) =>
        change == BlockReactionChange.MadeAtSpawn ? RoomObjectNativeListAddresses.DeleteList
            : change == SpeedBoosterBlockObjectDefinitions.BombReveal
                ? RoomObjectNativeListAddresses.SpeedBoosterBlockEntries.Single(entry => entry.Program == change).Entry
                : RoomObjectNativeListAddresses.BlockReactionEntry(change);

    /// <summary>Compares a selector with the native header indirection and rejects indexes outside its domain.</summary>
    private static void VerifyNativeBlockInstructionSelection(CartridgeImportAddressSpace rom,
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
