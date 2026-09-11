using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Scroll-data calculation performed by the wavy Phantoon HDMA pre-instruction.</summary>
public static class PhantoonWaveTable
{
    /// <summary>
    /// Builds one complete native cycle. Phase has already advanced for this call;
    /// the HDMA owner controls phase timing and repeats this cycle over the screen.
    /// </summary>
    public static void Build(ISnesAddressSpace bus, ushort mode, ushort phase,
        ushort amplitude, ushort bg2Scroll, Span<ushort> destination)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (mode == 0) throw new ArgumentOutOfRangeException(nameof(mode), "Inactive HDMA does not build a wave.");
        int half = (mode & PhantoonWaveRomData.LongWaveModeBit) != 0
            ? PhantoonWaveRomData.LongHalfCycle : PhantoonWaveRomData.ShortHalfCycle;
        if (destination.Length != half * 2)
            throw new ArgumentException("Destination must hold exactly one native wave cycle.", nameof(destination));
        int step = (PhantoonWaveRomData.PhaseMask + 1) / (half * 2);
        for (int i = 0; i < half; i++)
        {
            int address = PhantoonWaveRomData.SineWords + ((phase + i * step) & PhantoonWaveRomData.PhaseMask);
            short sine = unchecked((short)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));
            // Native unsigned partial products truncate the magnitude before applying
            // its sign. Arithmetic right-shifting a negative product rounds differently.
            int magnitude = Math.Abs((int)sine) * amplitude >> 16;
            int displacement = sine < 0 ? -magnitude : magnitude;
            destination[i] = unchecked((ushort)(bg2Scroll + displacement));
            destination[i + half] = unchecked((ushort)(bg2Scroll - displacement));
        }
    }
}
