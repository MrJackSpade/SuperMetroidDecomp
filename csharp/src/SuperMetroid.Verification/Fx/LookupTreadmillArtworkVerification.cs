using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;

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
            for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            {
                int delta = pointer - pair.First;
                if (delta >= 0 && delta < 16 && delta % 4 == 0)
                    AssertEqual(0x870000 | ReadVerificationWord(rom, 0x870002 + pointer),
                        pair.Definition.FrameSourceAddress((ushort)pointer), "Original directional operand view");
                else
                    AssertThrows<InvalidDataException>(() => pair.Definition.FrameSourceAddress((ushort)pointer),
                        "All non-frame instruction pointers reject");
            }
        }
    }
}
