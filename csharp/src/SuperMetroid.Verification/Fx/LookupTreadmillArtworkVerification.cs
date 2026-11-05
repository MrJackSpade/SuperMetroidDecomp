using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Fx;

/// <summary>Compares Wrecked Ship treadmill artwork source selection with retail direction programs.</summary>
internal static class LookupTreadmillArtworkVerification
{
    /// <summary>Checks four source aliases, invalid frame indices, and every 16-bit instruction pointer for both directions.</summary>
    internal static void VerifyTreadmillArtworkSources(ISnesAddressSpace rom)
    {
        var right = WreckedShipTreadmills.ForDirection(WreckedShipTreadmillDirection.Rightwards);
        var left = WreckedShipTreadmills.ForDirection(WreckedShipTreadmillDirection.Leftwards);
        int[] aliases = [AnimatedTileImageSources.Treadmill(0), AnimatedTileImageSources.Treadmill(1),
            AnimatedTileImageSources.Treadmill(2), AnimatedTileImageSources.Treadmill(3)];
        for (int index = 0; index < 4; index++)
        {
            int source = 0x870000 | ReadVerificationWord(rom, 0x8781e5 + 4 * index);
            AssertEqual(source, AnimatedTileImageSources.Treadmill(index), "Original contiguous treadmill source");
            AssertEqual(source, aliases[index], "Named source alias");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, 5, 255, 65535, int.MaxValue })
            AssertEqual("frameIndex", AssertThrows<ArgumentOutOfRangeException>(
                () => AnimatedTileImageSources.Treadmill(invalid), "Treadmill frame domain").ParamName,
                "Preserved rejection parameter");
        foreach (var pair in new[] { new VerifyTreadmillArtworkSourcesCase(Definition:right, First:0x81e3), new VerifyTreadmillArtworkSourcesCase(Definition:left, First:0x81f9) })
        {
            // The four frames' original operands are read first; the pointer domain is then swept across the cores, as
            // a frame's source is a pure function of the immutable direction program. A source lookup rejects exactly the
            // pointers its frame classification places outside the loop, so each pointer is classified without an
            // exception, and the rejection itself is confirmed once per direction on the first pointer past the loop.
            var operands = new Dictionary<int, int>();
            for (int delta = 0; delta < 16; delta += 4)
                operands.Add(pair.First + delta, 0x870000 | ReadVerificationWord(rom, 0x870002 + pair.First + delta));
            ParallelSweep.For(0, ushort.MaxValue + 1, pointer =>
            {
                if (operands.TryGetValue(pointer, out int operand))
                    AssertEqual(operand, pair.Definition.FrameSourceAddress((ushort)pointer), "Original directional operand view");
                else
                    AssertTrue(pair.Definition.FrameIndex((ushort)pointer) < 0, "All non-frame instruction pointers reject");
            });
            AssertThrows<InvalidDataException>(() => pair.Definition.FrameSourceAddress((ushort)(pair.First + 16)),
                "All non-frame instruction pointers reject");
        }
    }
}
