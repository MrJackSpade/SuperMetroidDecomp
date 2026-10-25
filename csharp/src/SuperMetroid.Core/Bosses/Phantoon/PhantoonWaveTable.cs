namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Scroll-data calculation performed by the wavy Phantoon HDMA pre-instruction.</summary>
public static class PhantoonWaveTable
{
    /// <summary>
    /// Builds one complete native cycle. Phase has already advanced for this call;
    /// the HDMA owner controls phase timing and repeats this cycle over the screen.
    /// </summary>
    public static void Build(ushort mode, ushort phase,
        ushort amplitude, ushort bg2Scroll, Span<ushort> destination)
    {
        if (mode == 0) throw new ArgumentOutOfRangeException(nameof(mode), "Inactive HDMA does not build a wave.");
        int half = (mode & PhantoonWaveDefinitions.LongWaveModeBit) != 0
            ? PhantoonWaveDefinitions.LongHalfCycle : PhantoonWaveDefinitions.ShortHalfCycle;
        Ensure.LengthEqual(destination, half * 2);
        int step = (PhantoonWaveDefinitions.PhaseMask + 1) / (half * 2);
        for (int i = 0; i < half; i++)
        {
            int displacement = CalculateDisplacement(unchecked((ushort)(phase + i * step)), amplitude);
            destination[i] = unchecked((ushort)(bg2Scroll + displacement));
            destination[i + half] = unchecked((ushort)(bg2Scroll - displacement));
        }
    }

    /// <summary>
    /// The signed sine value at a byte phase: an even phase reads a whole sample; an odd phase, which only a restored
    /// phase can hold, reads the high byte of one sample and the low byte of the next (the last odd phase reads
    /// <see cref="PhantoonWaveDefinitions.TrailingByte"/>). Independently reviewed for #1165: every phase matches the
    /// original, including the read past the last sample.
    /// </summary>
    public static short ReadSineAtBytePhase(ushort phase)
    {
        int offset = phase & PhantoonWaveDefinitions.PhaseMask;
        int sample = offset / PhantoonWaveDefinitions.SampleBytes;
        ushort value = unchecked((ushort)EnemyTrigonometryTables.SignedSine(new SnesByteAngle((byte)sample)));
        if (offset % PhantoonWaveDefinitions.SampleBytes == 0) return unchecked((short)value);
        byte next = offset == PhantoonWaveDefinitions.PhaseMask ? PhantoonWaveDefinitions.TrailingByte :
            SnesLittleEndian.LowByte(unchecked((ushort)EnemyTrigonometryTables.SignedSine(new SnesByteAngle((byte)(sample + 1)))));
        return unchecked((short)SnesLittleEndian.Word(SnesLittleEndian.HighByte(value), next));
    }

    /// <summary>
    /// Multiplies the signed sine sample by amplitude, retaining only native product
    /// bits 16-23 before restoring sign, including for unaligned phase samples.
    /// </summary>
    internal static int CalculateDisplacement(ushort phase, ushort amplitude)
    {
        short sine = ReadSineAtBytePhase(phase);
        int magnitude = unchecked((byte)SnesSixteenSixteen.Multiply(Math.Abs((int)sine), amplitude));
        return sine < 0 ? -magnitude : magnitude;
    }
}
