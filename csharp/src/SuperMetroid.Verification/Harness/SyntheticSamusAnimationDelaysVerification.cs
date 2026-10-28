using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Harness;

/// <summary>Supplies sparse cartridge animation-delay streams for targeted Samus timing checks.</summary>
internal static class SyntheticSamusAnimationDelaysVerification
{
    /// <summary>
    /// Seeds one synthetic bank-$91 animation-delay stream in the sparse cartridge fixture:
    /// <paramref name="pose"/>'s pointer-table entry selects <paramref name="stream"/>, whose
    /// first delay byte is <paramref name="firstDelay"/>.
    /// </summary>
    internal static void SeedSyntheticAnimationDelay(TestAddressSpace bus, SamusPoseId pose, ushort stream, byte firstDelay)
    {
        WriteTestWord(bus, SamusAnimationDelayDefinitions.PointerTableAddress + (int)pose * sizeof(ushort), stream);
        bus.WriteByte(0x910000 | stream, firstDelay);
    }
}
