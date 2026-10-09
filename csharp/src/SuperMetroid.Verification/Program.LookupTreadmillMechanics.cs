using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Parses the two cartridge treadmill instruction programs into their wait, timed-frame, and terminator pointers for comparison with the migrated definitions.</summary>
    /// <param name="rom">Address space containing the original Wrecked Ship instruction lists.</param>
    /// <returns>One tuple per native treadmill direction, preserving the header and instruction order read from the cartridge.</returns>
    private static IEnumerable<(ushort Header, ushort Wait, ushort[] Frames, ushort End)> OriginalTreadmillPrograms(ISnesAddressSpace rom)
    {
        foreach (ushort header in new ushort[] { 0x8275, 0x827b })
        {
            ushort wait = ReadVerificationWord(rom, 0x870000 | header);
            int cursor = wait + 2;
            var frames = new List<ushort>();
            while (ReadVerificationWord(rom, 0x870000 | cursor) < 0x8000)
            {
                AssertTrue(frames.Count < 4, "Native treadmill program has at most four timed entries");
                frames.Add((ushort)cursor);
                cursor += 4;
            }
            AssertEqual(4, frames.Count, "Native treadmill timed-entry count");
            yield return (header, wait, frames.ToArray(), (ushort)cursor);
        }
    }

    /// <summary>Resolves a native object header through the migrated treadmill catalog and fails the verification if it is not recognized.</summary>
    /// <param name="header">Cartridge object pointer identifying one treadmill direction.</param>
    /// <returns>The definition whose instruction pointers and mechanics words correspond to that object.</returns>
    private static WreckedShipTreadmillObjectDefinition TreadmillDefinition(ushort header)
    {
        AssertTrue(WreckedShipTreadmillMechanicsDefinitions.TryResolve(header, out var definition), "Native treadmill resolves");
        return definition;
    }

    /// <summary>Confirms that a pointer is owned as treadmill mechanics data and that the catalog returns the cartridge's original word.</summary>
    /// <param name="rom">Address space containing the native word used as the expected value.</param>
    /// <param name="definition">Treadmill definition whose ownership and value are being checked.</param>
    /// <param name="pointer">Instruction or object-data pointer to read from the definition.</param>
    private static void AssertTreadmillNativeWord(ISnesAddressSpace rom, WreckedShipTreadmillObjectDefinition definition, ushort pointer)
    {
        AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort actual), "Native treadmill mechanics word is owned");
        AssertEqual(ReadVerificationWord(rom, 0x870000 | pointer), actual, "Native treadmill field value");
    }

    /// <summary>Checks catalog order, direction-to-header mapping, complete header recognition, and rejection of invalid direction values.</summary>
    private static void VerifyTreadmillHeaderSelection()
    {
        var all = WreckedShipTreadmillMechanicsDefinitions.All.ToArray();
        AssertEqual(2, all.Length, "Two native treadmill headers");
        for (int index = 0; index < 2; index++)
        {
            ushort header = index == 0 ? (ushort)0x8275 : (ushort)0x827b;
            var direction = index == 0 ? WreckedShipTreadmillDirection.Rightwards : WreckedShipTreadmillDirection.Leftwards;
            AssertEqual(header, all[index].ObjectPointer, "Original enumeration identity/order");
            AssertEqual(direction, all[index].Direction, "Original direction association");
            AssertEqual(header, WreckedShipTreadmillMechanicsDefinitions.ForDirection(direction).ObjectPointer, "Named direction selects header");
            AssertEqual(direction, TreadmillDefinition(header).Direction, "Header selects named direction");
        }
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            bool found = WreckedShipTreadmillMechanicsDefinitions.TryResolve((ushort)value, out var definition);
            AssertEqual(value is 0x8275 or 0x827b, found, "Complete header domain");
            if (found) AssertEqual((ushort)value, definition.ObjectPointer, "Resolved identity");
            else AssertTrue(definition is null, "Unknown header returns null");
        }
        foreach (int value in new[] { int.MinValue, -1, 2, 3, 255, 65535, int.MaxValue })
            AssertThrows<InvalidDataException>(() => WreckedShipTreadmillMechanicsDefinitions.ForDirection(
                (WreckedShipTreadmillDirection)value), "Unknown direction rejects");
    }

    /// <summary>Checks that each definition's wait pointer matches the native instruction immediately following its object header.</summary>
    /// <param name="rom">Address space used to derive the original treadmill instruction layout.</param>
    private static void VerifyTreadmillWaitPointers(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
        {
            var definition = TreadmillDefinition(p.Header);
            AssertEqual(p.Wait, definition.WaitInstructionPointer, "Native wait cursor");
            AssertTreadmillNativeWord(rom, definition, p.Header);
        }
    }

    /// <summary>Checks the loop and goto pointers plus the ordered, bounds-checked view of four timed frame instructions.</summary>
    /// <param name="rom">Address space used to derive the original treadmill instruction layout.</param>
    private static void VerifyTreadmillFramePointers(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
        {
            var definition = TreadmillDefinition(p.Header);
            AssertEqual(p.Frames[0], definition.LoopInstructionPointer, "Loop follows two-byte wait");
            AssertEqual(p.End, definition.GotoInstructionPointer, "Goto follows four timed entries");
            AssertEqual(p.Frames.Length, definition.FrameInstructionPointers.Count, "Frame view count");
            AssertTrue(p.Frames.SequenceEqual(definition.FrameInstructionPointers), "Calculated frame enumeration");
            for (int index = 0; index < p.Frames.Length; index++)
                AssertEqual(p.Frames[index], definition.FrameInstructionPointers[index], "Calculated indexed frame cursor");
            foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => { _ = definition.FrameInstructionPointers[invalid]; }, "Frame view bounds");
        }
    }

    /// <summary>Checks that every timed frame instruction word is exposed as owned treadmill mechanics data with its native value.</summary>
    /// <param name="rom">Address space containing the original frame-duration words.</param>
    private static void VerifyTreadmillDurations(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
            foreach (ushort frame in p.Frames) AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), frame);
    }

    /// <summary>Checks ownership and values of each native wait opcode and the terminating goto instruction.</summary>
    /// <param name="rom">Address space containing the original control instructions.</param>
    private static void VerifyTreadmillControlOpcodes(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
        {
            AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), p.Wait);
            AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), p.End);
        }
    }

    /// <summary>Checks that the terminator jumps back to the first timed frame and that its target word is included in the owned domain.</summary>
    /// <param name="rom">Address space containing each native goto target.</param>
    private static void VerifyTreadmillLoopTargets(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
        {
            var definition = TreadmillDefinition(p.Header);
            AssertEqual(ReadVerificationWord(rom, 0x870002 + p.End), definition.LoopInstructionPointer, "Native goto target");
            AssertTreadmillNativeWord(rom, definition, (ushort)(p.End + 2));
        }
    }

    /// <summary>Checks ownership of each object header's transfer-size operand against the original cartridge word.</summary>
    /// <param name="rom">Address space containing the native object operands.</param>
    private static void VerifyTreadmillTransferSizes(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
            AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), (ushort)(p.Header + 2));
    }

    /// <summary>Checks ownership of each object header's VRAM-destination operand against the original cartridge word.</summary>
    /// <param name="rom">Address space containing the native object operands.</param>
    private static void VerifyTreadmillVramDestinations(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
            AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), (ushort)(p.Header + 4));
    }

    /// <summary>Enumerates the word-address domain to verify that exactly the two headers' mechanics operands and instruction words are owned.</summary>
    /// <param name="rom">Address space used to derive expected owned addresses and native values.</param>
    private static void VerifyTreadmillMechanicsDomain(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
        {
            var definition = TreadmillDefinition(p.Header);
            var expected = p.Frames.ToHashSet();
            expected.UnionWith([p.Header, (ushort)(p.Header + 2), (ushort)(p.Header + 4), p.Wait, p.End, (ushort)(p.End + 2)]);
            AssertEqual(10, expected.Count, "Ten original owned mechanics words");
            for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            {
                bool found = definition.TryReadMechanicsWord((ushort)pointer, out ushort value);
                AssertEqual(expected.Contains((ushort)pointer), found, "Full word-ownership domain including artwork operands");
                if (!found) AssertEqual((ushort)0, value, "Unowned word output clears");
            }
        }
    }
}
