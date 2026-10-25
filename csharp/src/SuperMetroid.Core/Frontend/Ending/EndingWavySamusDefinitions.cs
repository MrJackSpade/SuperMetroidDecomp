namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Post-credits BG3 scroll program and artwork identities.</summary>
internal static class EndingWavySamusDefinitions
{
    /// <summary>$88:EC48 / Spawn_WavySamus_HDMAObject seeds unsigned 8.8 amplitude $4000.</summary>
    private const int Amplitude = 0x4000;
    /// <summary>$88:EC4E / Spawn_WavySamus_HDMAObject seeds phase delta eight, doubled to a byte index.</summary>
    private const int PhaseByteStep = 8 * 2;
    /// <summary>$88:ECA2 / Instruction_Setup_WavySamus initializes the phase to minus two.</summary>
    private const int InitialPhase = -2;
    /// <summary>$88:ECD1 / PreInstruction_WavySamus advances four sine-table bytes per first-half sample.</summary>
    private const int SampleByteStep = 4;
    /// <summary>$88:ECD6 / PreInstruction_WavySamus produces 64 words and a sign-inverted second half.</summary>
    private const int HalfCycle = 64;
    /// <summary>$8B:E193 / CinematicFunction_PostCredits_WavySamus advances BG3Y by two per call.</summary>
    private const int VerticalStep = 2;

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
