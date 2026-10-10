using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the four named treadmill frame sources against ROM and verifies that directional instruction views accept only their own frame operands.</summary>
    /// <param name="rom">Retail address space containing treadmill source operands and frame data.</param>
    private static void VerifyTreadmillArtworkSources(ISnesAddressSpace rom)
    {
        var right = WreckedShipTreadmillMechanicsDefinitions.ForDirection(WreckedShipTreadmillDirection.Rightwards);
        var left = WreckedShipTreadmillMechanicsDefinitions.ForDirection(WreckedShipTreadmillDirection.Leftwards);
        int[] aliases = [WreckedShipTreadmillRomData.Frame0Source, WreckedShipTreadmillRomData.Frame1Source,
            WreckedShipTreadmillRomData.Frame2Source, WreckedShipTreadmillRomData.Frame3Source];
        for (int index = 0; index < 4; index++)
        {
            int source = 0x870000 | ReadVerificationWord(rom, 0x8781e5 + 4 * index);
            AssertEqual(source, WreckedShipTreadmillRomData.FrameSource(index), "Original contiguous treadmill source");
            AssertEqual(source, aliases[index], "Named source alias");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, 5, 255, 65535, int.MaxValue })
            AssertEqual("frameIndex", AssertThrows<ArgumentOutOfRangeException>(
                () => WreckedShipTreadmillRomData.FrameSource(invalid), "Treadmill frame domain").ParamName!,
                "Preserved rejection parameter");
        foreach (var pair in new[] { (Definition: right, First: 0x81e3), (Definition: left, First: 0x81f9) })
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
