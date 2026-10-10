namespace SuperMetroid.Core.Game;

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
        int half = (mode & PhantoonWaveRomData.LongWaveModeBit) != 0
            ? PhantoonWaveRomData.LongHalfCycle : PhantoonWaveRomData.ShortHalfCycle;
        if (destination.Length != half * 2)
            throw new ArgumentException("Destination must hold exactly one native wave cycle.", nameof(destination));
        int step = (PhantoonWaveRomData.PhaseMask + 1) / (half * 2);
        for (int i = 0; i < half; i++)
        {
            int displacement = CalculateDisplacement(unchecked((ushort)(phase + i * step)), amplitude);
            destination[i] = unchecked((ushort)(bg2Scroll + displacement));
            destination[i + half] = unchecked((ushort)(bg2Scroll - displacement));
        }
    }

    /// <summary>Scales one signed sine-table sample using the cartridge's truncated unsigned-product behavior.</summary>
    /// <param name="phase">Byte phase used to select the native sine sample.</param>
    /// <param name="amplitude">Wave amplitude applied to the sample before the native product-byte truncation.</param>
    /// <returns>Signed horizontal scroll displacement for the selected wave sample.</returns>
    private static int CalculateDisplacement(ushort phase, ushort amplitude)
    {
        short sine = PhantoonWaveRomData.ReadSineAtBytePhase(phase);
        // Native unsigned byte products truncate BEFORE restoring sign. Its final
        // AND $FF00 / XBA retains only product bits 16..23, even for unaligned samples.
        int magnitude = (Math.Abs((int)sine) * amplitude >> 16) & byte.MaxValue;
        return sine < 0 ? -magnitude : magnitude;
    }
}
