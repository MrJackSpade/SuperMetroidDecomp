using static SuperMetroid.Core.Frontend.Ending.EndingWavySamusDefinitions;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Ripples Samus in the ending (<see cref="EndingWavySamusDefinitions"/>): the per-line scrolls the wave shows at each age.
/// </summary>
internal static class EndingWavySamus
{
    /// <summary>Reconstructs all 224 BG3 scanline offsets from cinematic age, retaining the setup-only first pass and native sine phase.</summary>
    internal static Position16<BgPlanePixels>[] ScrollsAtAge(int age)
    {
        var result = new Position16<BgPlanePixels>[224];
        // Global HDMA precedes the cinematic dispatcher. Age one executes setup only;
        // age two is the first pre-instruction. Derive this deterministic owner from
        // the existing cinematic countdown so debugger restores retain exact phase.
        int phase = InitialPhase + PhaseByteStep * (age - 1);
        for (int line = 0; line < result.Length; line++)
        {
            int displacement = 0;
            if (age >= 2)
            {
                int sample = line % HalfCycle;
                int sine = Trigonometry.SignedSine(unchecked(new ByteAngle((byte)((phase + sample * SampleByteStep) >> 1))));
                int magnitude = SixteenSixteen.Multiply(Math.Abs(sine), Amplitude);
                displacement = sine < 0 ? -magnitude : magnitude;
                if ((line / HalfCycle & 1) != 0) displacement = -displacement;
            }
            result[line] = new(unchecked((ushort)displacement), unchecked((ushort)(age * VerticalStep)));
        }
        return result;
    }
}
