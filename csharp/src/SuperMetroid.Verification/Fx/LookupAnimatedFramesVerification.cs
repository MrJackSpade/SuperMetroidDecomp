using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks seven room-FX animation loops against native frame cursors and durations.</summary>
internal static class LookupAnimatedFramesVerification
{
    /// <summary>Enumerates each retail timed-frame span up to its terminal control instruction.</summary>
    internal static IEnumerable<OriginalSimpleAnimationLoopsResult> OriginalSimpleAnimationLoops(CartridgeImportAddressSpace rom)
    {
        foreach (ushort header in new ushort[] { 0x8257, 0x8287, 0x828d, 0x82ab, 0x82c9, 0x82e7, 0x82fd })
        {
            ushort first = ReadVerificationWord(rom, 0x870000 | header);
            ushort cursor = first;
            int count = 0;
            while (ReadVerificationWord(rom, 0x870000 | cursor) < 0x8000)
            {
                AssertTrue(++count <= 256, "Original animation has bounded timed sequence");
                cursor = unchecked((ushort)(cursor + 4));
            }
            yield return new OriginalSimpleAnimationLoopsResult(header, first, cursor);
        }
    }

    /// <summary>Checks frame pointer enumeration, artwork operand offsets, Goto words, and index bounds.</summary>
    internal static void VerifySimpleAnimationFrameCursors(CartridgeImportAddressSpace rom)
    {
        int total = 0;
        foreach (var original in OriginalSimpleAnimationLoops(rom))
        {
            AssertTrue(RoomFxAnimatedTiles.TryResolve(FxNativeWords.AnimatedTileObjects.Decode(original.Object, "animated-tile object"), out var definition),
                "Original animation object resolves");
            int count = (original.End - original.First) / 4;
            AssertEqual(count, (definition ?? throw new InvalidOperationException("Verification requires definition.")).Frames.Count, "Native frame count");
            for (int index = 0; index < count; index++)
            {
                ushort cursor = (ushort)(original.First + 4 * index);
                AssertEqual(cursor, definition.FrameInstructionPointer(index), "Native frame cursor");
                AssertEqual((ushort)(cursor + 2), definition.FrameSourceOperandPointer(index), "Native artwork operand identity");
                total++;
            }
            AssertEqual(original.End, definition.GotoInstructionPointer, "Native terminal goto address");
            foreach (ushort pointer in new[] { original.End, (ushort)(original.End + 2) })
            {
                AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort value), "Loop control is compiled");
                AssertEqual(ReadVerificationWord(rom, 0x870000 | pointer), value, "Original loop opcode/target");
            }
            foreach (int invalid in new[] { int.MinValue, -1, count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => { _ = definition.FrameInstructionPointer(invalid); }, "Original frame bounds");
        }
        AssertEqual(30, total, "Complete original timed-frame domain");
    }

    /// <summary>Compares indexed and enumerated frame durations with native bank-$87 words.</summary>
    internal static void VerifySimpleAnimationFrameDurations(CartridgeImportAddressSpace rom)
    {
        foreach (var original in OriginalSimpleAnimationLoops(rom))
        {
            AssertTrue(RoomFxAnimatedTiles.TryResolve(FxNativeWords.AnimatedTileObjects.Decode(original.Object, "animated-tile object"), out var definition), "Animation resolves");
            ArgumentNullException.ThrowIfNull(definition);
            for (int index = 0; index < definition.Frames.Count; index++)
            {
                ushort expected = ReadVerificationWord(rom, 0x870000 | (original.First + 4 * index));
                AssertEqual(expected, definition.Frames[index].Duration, "Native frame duration");
                AssertTrue(definition.TryReadMechanicsWord(definition.FrameInstructionPointer(index), out ushort value), "Frame duration is compiled");
                AssertEqual(expected, value, "Mechanics word duration view");
            }
        }
    }
}
