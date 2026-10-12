using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AssertionsVerification;
using static VerificationHelpersVerification;

internal static class SyntheticSamusAnimationDelaysVerification
{
    /// <summary>
    /// Seeds one synthetic bank-$91 animation-delay stream in the sparse cartridge fixture:
    /// <paramref name="pose"/>'s pointer-table entry selects <paramref name="stream"/>, whose
    /// first delay byte is <paramref name="firstDelay"/>.
    /// </summary>
    [CpuBusBoundary("exact bus test: seeds a synthetic Samus animation-delay pointer and first delay byte in the sparse cartridge fixture")]
    internal static void SeedSyntheticAnimationDelay(TestAddressSpace bus, SamusPoseId pose, ushort stream, byte firstDelay)
    {
        WriteTestWord(bus, SamusAnimationDelayDefinitions.PointerTableAddress + (int)pose * sizeof(ushort), stream);
        bus.WriteByte(0x910000 | stream, firstDelay);
    }
}
