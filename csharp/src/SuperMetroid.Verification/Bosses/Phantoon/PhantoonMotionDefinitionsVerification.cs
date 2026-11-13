using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Checks Phantoon figure-eight speed constants and phase transitions against the cartridge.</summary>
internal static class PhantoonMotionDefinitionsVerification
{
    /// <summary>Compares fourteen native words and exercises both real speed adjusters across fractional, reachable whole, and phase boundary values.</summary>
    internal static void VerifyCompiledPhantoonMotion(CartridgeImportAddressSpace rom)
    {
        // Each definition is the signed value the cartridge's word holds; every unsigned one is below $8000.
        int[] definitions = [PhantoonMotionDefinitions.SlowFraction, PhantoonMotionDefinitions.SlowWhole,
            PhantoonMotionDefinitions.FastFraction, PhantoonMotionDefinitions.FastWhole,
            PhantoonMotionDefinitions.ForwardSlowCap, PhantoonMotionDefinitions.ForwardFastCap,
            PhantoonMotionDefinitions.ForwardMinimum, PhantoonMotionDefinitions.SlowFraction,
            PhantoonMotionDefinitions.SlowWhole, PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole, PhantoonMotionDefinitions.ReverseSlowCap,
            PhantoonMotionDefinitions.ReverseFastCap, PhantoonMotionDefinitions.ReverseMaximum];
        var native = new ushort[14];
        for (int i = 0; i < native.Length; i++)
        {
            int address = 0xa7cd73 + i * 2;
            native[i] = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(unchecked((short)native[i]), definitions[i], "Phantoon compiled motion value");
        }
        Action<PhantoonBodyState> forward = PhantoonPatterns.AdjustPhantoonForwardFigureEightSpeed;
        Action<PhantoonBodyState> reverse = PhantoonPatterns.AdjustPhantoonReverseFigureEightSpeed;
        // The phase words the cartridge writes: the slow start, the surge and settling (PhantoonFigureEightStageWords).
        ushort[] phases = [0, 1, 2];
        ushort[] fractions = [0, 1, 0x5ff, 0x600, 0xefff, 0xf000, 0xf9ff, 0xfa00, 0xffff];
        PhantoonMotionCase[] cases = [.. new[] { false, true }.SelectMany(backwards => phases.SelectMany(phase =>
            fractions.Select(fraction => new PhantoonMotionCase(backwards, phase, fraction))))];
        // Each direction, phase and fraction sweeps every whole speed on one worker, adjusting a state of its own; the
        // adjustments change only the state they are given.
        ParallelSweep.For(0, cases.Length, index =>
        {
            (bool backwards, ushort phase, ushort fraction) = cases[index];
            var words = new PhantoonBodyState();
            // Every whole speed the figure-eight reaches, with a margin: its caps keep it within seven entries of rest.
            for (int whole = -16; whole <= 16; whole++)
            {
                words.FigureEightSpeed = SignedSixteenSixteen.FromParts(whole, fraction);
                words.FigureEightStage = SuperMetroid.TestSupport.MovieReplay.PhantoonFigureEightStageWords.Decode(phase);
                (ushort low, ushort high, ushort next) = Expected(fraction, unchecked((ushort)whole), phase, backwards);
                (backwards ? reverse : forward)(words);
                AssertEqual(low, words.FigureEightSpeed.Fraction, "Phantoon real fractional speed after adjustment");
                AssertEqual(high, words.FigureEightSpeed.UnsignedWholePixels, "Phantoon real signed whole speed after adjustment");
                AssertEqual(SuperMetroid.TestSupport.MovieReplay.PhantoonFigureEightStageWords.Decode(next), words.FigureEightStage, "Phantoon real acceleration phase transition");
            }
        });
        Console.WriteLine("Phantoon motion: 14 native words and 1,782 real speed/stage adjustments match without a bus.");
        foreach (bool backwards in new[] { false, true })
        {
            // A pass from rest runs the stages in their order and returns to its start, as the cartridge's phase word does.
            var body = new PhantoonBodyState { FigureEightSpeed = SignedSixteenSixteen.FromParts(backwards ? 0 : 1, 0) };
            var stages = new List<PhantoonFigureEightStage> { body.FigureEightStage };
            while (stages.Count < 4)
            {
                (backwards ? reverse : forward)(body);
                if (body.FigureEightStage != stages[^1])
                    stages.Add(body.FigureEightStage);
            }
            AssertTrue(stages.SequenceEqual([PhantoonFigureEightStage.EasingIn, PhantoonFigureEightStage.Surging,
                PhantoonFigureEightStage.Settling, PhantoonFigureEightStage.EasingIn]),
                $"a figure-eight's {(backwards ? "returning" : "forward")} pass eases in, surges, settles and eases in again");
        }

        // Use the ROM records, wide arithmetic and explicit word extraction independently
        // of the production add/sub helpers. Signed comparisons intentionally wrap at 16 bits.
        ExpectedResult Expected(ushort low, ushort high, ushort phase, bool backwards)
        {
            int basis = backwards ? 7 : 0;
            bool slow = phase == 0;
            bool accelerating = slow || (phase & 1) != 0;
            int offset = basis + (slow ? 0 : 2);
            long delta = native[offset] + ((long)native[offset + 1] << 16);
            long value = low + ((long)high << 16);
            value += backwards == accelerating ? -delta : delta;
            low = unchecked((ushort)value);
            high = unchecked((ushort)(value >> 16));
            ushort cap = native[basis + (slow ? 4 : accelerating ? 5 : 6)];
            short difference = unchecked((short)(high - cap));
            bool reached = backwards == accelerating ? difference <= 0 : difference >= 0;
            if (reached)
            {
                low = 0;
                high = unchecked((ushort)(cap + (backwards ? slow ? 2 : accelerating ? 1 : 0 : slow ? -1 : accelerating ? 0 : 1)));
                phase = slow ? (ushort)1 : accelerating ? unchecked((ushort)(phase + 1)) : (ushort)0;
            }
            return new ExpectedResult(low, high, phase);
        }
    }
}
