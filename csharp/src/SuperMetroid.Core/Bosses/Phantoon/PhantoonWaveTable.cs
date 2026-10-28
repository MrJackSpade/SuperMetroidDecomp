using System.Buffers.Binary;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Scroll-data calculation performed by the wavy Phantoon HDMA pre-instruction.</summary>
public static class PhantoonWaveTable
{
    /// <summary>
    /// Builds one complete native cycle. Phase has already advanced for this call;
    /// the HDMA owner controls phase timing and repeats this cycle over the screen.
    /// </summary>
    /// <param name="mode">The wave channel's nonzero mode; bit zero selects the long cycle.</param>
    /// <param name="phase">The sine phase the cycle starts at, 0 through 65535; only its byte phase within one cycle is read.</param>
    /// <param name="amplitude">The wave amplitude, 0 through 65535.</param>
    /// <param name="bg2Scroll">The base horizontal scroll the wave displaces, 0 through 65535.</param>
    /// <param name="destination">Receives one horizontal scroll per line of the cycle.</param>
    public static void Build(int mode, int phase,
        int amplitude, int bg2Scroll, Span<ushort> destination)
    {
        if (mode == 0) throw new ArgumentOutOfRangeException(nameof(mode), "Inactive HDMA does not build a wave.");
        int half = PhantoonWaveModeWord.HalfCycle(mode);
        Ensure.LengthEqual(destination, half * 2);
        int step = PhantoonWaveDefinitions.PhaseCycle / (half * 2);
        for (int i = 0; i < half; i++)
        {
            int displacement = CalculateDisplacement(phase + i * step, amplitude);
            destination[i] = unchecked((ushort)(bg2Scroll + displacement));
            destination[i + half] = unchecked((ushort)(bg2Scroll - displacement));
        }
    }

    /// <summary>
    /// The position of <paramref name="phase"/> within one whole cycle, 0 through
    /// <see cref="PhantoonWaveDefinitions.PhaseCycle"/> minus one. A phase below zero, as the setup's
    /// <see cref="PhantoonWaveDefinitions.InitialPhase"/> is, counts back from the cycle's end.
    /// </summary>
    /// <param name="phase">Any phase.</param>
    public static int PhaseInCycle(int phase)
    {
        int remainder = phase % PhantoonWaveDefinitions.PhaseCycle;
        return remainder < 0 ? remainder + PhantoonWaveDefinitions.PhaseCycle : remainder;
    }

    /// <summary>
    /// The signed sine value at a byte phase: an even phase reads a whole sample; an odd phase, which only a restored
    /// phase can hold, reads the high byte of one sample and the low byte of the next (the last odd phase reads
    /// <see cref="PhantoonWaveDefinitions.TrailingByte"/>). Independently reviewed for #1165: every phase matches the
    /// original, including the read past the last sample.
    /// </summary>
    /// <param name="phase">The byte phase, nonnegative; only its position within one cycle is read.</param>
    public static short ReadSineAtBytePhase(int phase)
    {
        int offset = PhaseInCycle(phase);
        int sample = offset / PhantoonWaveDefinitions.SampleBytes;
        int value = unchecked((ushort)Trigonometry.SignedSine(new ByteAngle((byte)sample)));
        if (offset % PhantoonWaveDefinitions.SampleBytes == 0) return unchecked((short)value);
        // The odd phase reads the little-endian word straddling this sample and the next.
        Span<byte> samples = stackalloc byte[2 * PhantoonWaveDefinitions.SampleBytes];
        BinaryPrimitives.WriteUInt16LittleEndian(samples, (ushort)value);
        if (offset == PhantoonWaveDefinitions.PhaseCycle - 1)
            samples[PhantoonWaveDefinitions.SampleBytes] = PhantoonWaveDefinitions.TrailingByte;
        else
            BinaryPrimitives.WriteUInt16LittleEndian(samples[PhantoonWaveDefinitions.SampleBytes..],
                unchecked((ushort)Trigonometry.SignedSine(new ByteAngle((byte)(sample + 1)))));
        return unchecked((short)BinaryPrimitives.ReadUInt16LittleEndian(samples[1..]));
    }

    /// <summary>
    /// Multiplies the signed sine sample by amplitude, retaining only native product
    /// bits 16-23 before restoring sign, including for unaligned phase samples.
    /// </summary>
    /// <param name="phase">The byte phase, nonnegative.</param>
    /// <param name="amplitude">The wave amplitude, 0 through 65535.</param>
    internal static int CalculateDisplacement(int phase, int amplitude)
    {
        int sine = ReadSineAtBytePhase(phase);
        int magnitude = unchecked((byte)SixteenSixteen.Multiply(Math.Abs(sine), amplitude));
        return sine < 0 ? -magnitude : magnitude;
    }
}
