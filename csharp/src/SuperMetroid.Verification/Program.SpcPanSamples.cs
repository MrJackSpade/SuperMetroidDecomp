using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies the 21 native pan samples, their bounded interpolation endpoint, and the
    /// adjacent sharp-echo FIR impulse preset against the SPC ROM data.
    /// </summary>
    /// <remarks>Also checks that pan-table indices outside the local interpolation view are rejected.</remarks>
    /// <param name="rom">ROM address space supplying the native SPC pan and FIR bytes.</param>
    private static void VerifySpcPanSamples(ISnesAddressSpace rom)
    {
        AssertEqual(21, SpcMusicTables.PanSampleCount, "native curve ends before the FIR preset");
        for (int index = 0; index < 22; index++)
            AssertEqual(rom.ReadByte(0xcf8a25 + index), SpcMusicTables.PanVolume(index),
                "original local interpolation view including adjacent FIR byte");
        AssertEqual(0x1e32, (int)SpcDriverData.Echo.FirCoefficientTableAddress, "native adjacent FIR base");
        // The neighboring sharp-echo preset is an impulse: maximal signed positive
        // first tap followed by seven zeros, not an additional pan sampling interval.
        AssertEqual((byte)127, rom.ReadByte(0xcf8a3a), "sharp-echo first tap");
        for (int tap = 1; tap < 8; tap++)
            AssertEqual((byte)0, rom.ReadByte(0xcf8a3a + tap), "sharp-echo remaining taps");
        foreach (int invalid in new[] { int.MinValue, -1, 22, 255, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.PanVolume(invalid), "bounded pan interpolation view");
    }
}
