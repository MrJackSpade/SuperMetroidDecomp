using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.Verification.Audio;

/// <summary>Checks the SPC pan interpolation table's boundary against the adjacent FIR preset.</summary>
internal static class SpcPanSamplesVerification
{
    /// <summary>Compares native bytes, verifies the 21-sample bound, and rejects out-of-range pan indexes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for SPC pan samples")]
    internal static void VerifySpcPanSamples(ISnesAddressSpace rom)
    {
        AssertEqual(21, SpcMusicTables.PanSampleCount, "native curve ends before the FIR preset");
        for (int index = 0; index < 22; index++)
            AssertEqual(rom.ReadCartridgeByte(0xcf8a25 + index), SpcMusicTables.PanVolume(index),
                "original local interpolation view including adjacent FIR byte");
        AssertEqual(0x1e32, (int)SpcDriverDefinitions.Echo.FirCoefficientTableAddress, "native adjacent FIR base");
        // The neighboring sharp-echo preset is an impulse: maximal signed positive
        // first tap followed by seven zeros, not an additional pan sampling interval.
        AssertEqual((byte)127, rom.ReadCartridgeByte(0xcf8a3a), "sharp-echo first tap");
        for (int tap = 1; tap < 8; tap++)
            AssertEqual((byte)0, rom.ReadCartridgeByte(0xcf8a3a + tap), "sharp-echo remaining taps");
        foreach (int invalid in new[] { int.MinValue, -1, 22, 255, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SpcMusicTables.PanVolume(invalid), "bounded pan interpolation view");
    }
}
