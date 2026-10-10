using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static IEnumerable<(ushort Object, ushort First, ushort End)> OriginalSimpleAnimationLoops(SuperMetroidAddressSpace rom)
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
            yield return (header, first, cursor);
        }
    }

    private static void VerifySimpleAnimationFrameCursors(SuperMetroidAddressSpace rom)
    {
        int total = 0;
        foreach (var original in OriginalSimpleAnimationLoops(rom))
        {
            AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve((AnimatedTileObject)original.Object, out var definition),
                "Original animation object resolves");
            int count = (original.End - original.First) / 4;
            AssertEqual(count, definition.Frames.Count, "Native frame count");
            var enumerated = definition.Frames.ToArray();
            AssertEqual(count, enumerated.Length, "Calculated frame enumeration count");
            for (int index = 0; index < count; index++)
            {
                ushort cursor = (ushort)(original.First + 4 * index);
                AssertEqual(cursor, definition.Frames[index].InstructionPointer, "Native frame cursor");
                AssertEqual(cursor, enumerated[index].InstructionPointer, "Enumeration cursor view");
                AssertEqual((ushort)(cursor + 2), definition.Frames[index].SourceOperandPointer, "Native artwork operand identity");
                total++;
            }
            AssertEqual(original.End, definition.GotoInstructionPointer, "Native terminal goto address");
            foreach (ushort pointer in new[] { original.End, (ushort)(original.End + 2) })
            {
                AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort value), "Loop control is compiled");
                AssertEqual(ReadVerificationWord(rom, 0x870000 | pointer), value, "Original loop opcode/target");
            }
            foreach (int invalid in new[] { int.MinValue, -1, count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => { _ = definition.Frames[invalid]; }, "Original read-only frame bounds");
        }
        AssertEqual(30, total, "Complete original timed-frame domain");
    }

    private static void VerifySimpleAnimationFrameDurations(SuperMetroidAddressSpace rom)
    {
        foreach (var original in OriginalSimpleAnimationLoops(rom))
        {
            AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve((AnimatedTileObject)original.Object, out var definition), "Animation resolves");
            int index = 0;
            foreach (var frame in definition.Frames)
            {
                ushort expected = ReadVerificationWord(rom, 0x870000 | (original.First + 4 * index));
                AssertEqual(expected, frame.Duration, "Native enumerated frame duration");
                AssertEqual(expected, definition.Frames[index].Duration, "Native indexed frame duration");
                AssertTrue(definition.TryReadMechanicsWord(frame.InstructionPointer, out ushort value), "Frame duration is compiled");
                AssertEqual(expected, value, "Mechanics word duration view");
                index++;
            }
        }
    }
}