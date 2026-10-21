using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>Builds sparse cartridge inputs for exact signed-sine table checks.</summary>
internal static class SignedSineFixtureVerification
{
    /// <summary>
    /// Seeds entries of the shared signed 8.8 sine table (`$A0:B443`, 256 words indexed by
    /// eight-bit angle) into a synthetic cartridge. Each sample is written as one native
    /// little-endian word at its angle's slot; unseeded angles stay zero.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for the $A0:B443 signed sine table")]
    internal static void SeedSignedSineTable(TestAddressSpace bus, ReadOnlySpan<SineSample> samples)
    {
        foreach ((byte angle, short value) in samples)
            WriteTestWord(bus, 0xa0b443 + angle * 2, unchecked((ushort)value));
    }
}
