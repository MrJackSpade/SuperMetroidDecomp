using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.AnimatedTileInstructionCodesVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Compares the two Wrecked Ship treadmill instruction programs with bank $87 mechanics.</summary>
internal static class LookupTreadmillMechanicsVerification
{
    /// <summary>Walks each native header, wait, four timed frames, and terminating goto address.</summary>
    internal static IEnumerable<(ushort Header, ushort Wait, ushort[] Frames, ushort End)> OriginalTreadmillPrograms(ISnesAddressSpace rom)
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

    /// <summary>Resolves a native treadmill object header to its compiled direction definition.</summary>
    internal static WreckedShipTreadmillObjectDefinition TreadmillDefinition(ushort header)
    {
        AssertTrue(WreckedShipTreadmillMechanicsDefinitions.TryResolve((AnimatedTileObject)header, out var definition), "Native treadmill resolves");
        return (definition ?? throw new InvalidOperationException("Verification requires definition."));
    }

    /// <summary>Checks that a compiled mechanics word is owned and equals the cartridge word.</summary>
    private static void AssertTreadmillNativeWord(ISnesAddressSpace rom, WreckedShipTreadmillObjectDefinition definition, ushort pointer)
    {
        AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort actual), "Native treadmill mechanics word is owned");
        AssertEqual(ReadVerificationWord(rom, 0x870000 | pointer), actual, "Native treadmill field value");
    }

    /// <summary>Checks both header identities, direction mapping, and rejection of unsupported inputs.</summary>
    internal static void VerifyTreadmillHeaderSelection()
    {
        var all = WreckedShipTreadmillMechanicsDefinitions.All.ToArray();
        AssertEqual(2, all.Length, "Two native treadmill headers");
        for (int index = 0; index < 2; index++)
        {
            ushort header = index == 0 ? (ushort)0x8275 : (ushort)0x827b;
            var direction = index == 0 ? WreckedShipTreadmillDirection.Rightwards : WreckedShipTreadmillDirection.Leftwards;
            AssertEqual(header, (ushort)all[index].ObjectPointer, "Original enumeration identity/order");
            AssertEqual(direction, all[index].Direction, "Original direction association");
            AssertEqual(header, (ushort)WreckedShipTreadmillMechanicsDefinitions.ForDirection(direction).ObjectPointer, "Named direction selects header");
            AssertEqual(direction, TreadmillDefinition(header).Direction, "Header selects named direction");
        }
        foreach (AnimatedTileObject header in Enum.GetValues<AnimatedTileObject>())
        {
            ushort value = (ushort)header;
            bool found = WreckedShipTreadmillMechanicsDefinitions.TryResolve(header, out var definition);
            AssertEqual(value is 0x8275 or 0x827b, found, "Complete header domain");
            if (found) AssertEqual(value, (ushort)(definition ?? throw new InvalidOperationException("Verification requires definition.")).ObjectPointer, "Resolved identity");
            else AssertTrue(definition is null, "Unknown header returns null");
        }
        foreach (ushort undefined in UndefinedAnimatedTileObjects)
            AssertThrows<InvalidOperationException>(
                () => WreckedShipTreadmillMechanicsDefinitions.TryResolve((AnimatedTileObject)undefined, out _),
                "Undefined treadmill header is rejected at the typed boundary");
        foreach (int value in new[] { int.MinValue, -1, 2, 3, 255, 65535, int.MaxValue })
            AssertThrows<InvalidDataException>(() => WreckedShipTreadmillMechanicsDefinitions.ForDirection(
                (WreckedShipTreadmillDirection)value), "Unknown direction rejects");
    }

    /// <summary>Checks the wait instruction address selected by each native object header.</summary>
    internal static void VerifyTreadmillWaitPointers(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
        {
            var definition = TreadmillDefinition(p.Header);
            AssertEqual(p.Wait, definition.WaitInstructionPointer, "Native wait cursor");
            AssertTreadmillNativeWord(rom, definition, p.Header);
        }
    }

    /// <summary>Checks the four frame cursors, loop and goto addresses, and frame-index bounds.</summary>
    internal static void VerifyTreadmillFramePointers(ISnesAddressSpace rom)
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

    /// <summary>Checks each timed frame duration against its original program word.</summary>
    internal static void VerifyTreadmillDurations(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
            foreach (ushort frame in p.Frames) AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), frame);
    }

    /// <summary>Checks the native wait and goto opcodes for both treadmill programs.</summary>
    internal static void VerifyTreadmillControlOpcodes(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
        {
            AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), p.Wait);
            AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), p.End);
        }
    }

    /// <summary>Checks that each goto operand returns to its timed-frame loop.</summary>
    internal static void VerifyTreadmillLoopTargets(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
        {
            var definition = TreadmillDefinition(p.Header);
            AssertEqual(ReadVerificationWord(rom, 0x870002 + p.End), definition.LoopInstructionPointer, "Native goto target");
            AssertTreadmillNativeWord(rom, definition, (ushort)(p.End + 2));
        }
    }

    /// <summary>Checks the transfer-size word embedded in each treadmill header.</summary>
    internal static void VerifyTreadmillTransferSizes(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
            AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), (ushort)(p.Header + 2));
    }

    /// <summary>Checks the VRAM destination word embedded in each treadmill header.</summary>
    internal static void VerifyTreadmillVramDestinations(ISnesAddressSpace rom)
    {
        foreach (var p in OriginalTreadmillPrograms(rom))
            AssertTreadmillNativeWord(rom, TreadmillDefinition(p.Header), (ushort)(p.Header + 4));
    }

    /// <summary>Checks exact ownership of the ten mechanics words in each 16-bit pointer domain.</summary>
    internal static void VerifyTreadmillMechanicsDomain(ISnesAddressSpace rom)
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
