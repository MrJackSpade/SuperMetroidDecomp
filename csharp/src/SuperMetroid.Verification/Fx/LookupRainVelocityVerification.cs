using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Compares the rain winds with the four retail velocity words and their full random-selection domain.</summary>
internal static class LookupRainVelocityVerification
{
    /// <summary>Checks the wind every one of the 65,536 random numbers chooses against the word the original reads for it.</summary>
    internal static void VerifyRainHorizontalVelocity(CartridgeImportAddressSpace rom)
    {
        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            // Native LSR / AND 0006 produces a byte offset, independently of the
            // production choice. Read the original word, not a formula.
            int nativeOffset = (random >> 1) & 6;
            int expected = unchecked((short)ReadVerificationWord(rom, 0x88d992 + nativeOffset));
            int actual = AtmosphereDrift.RainHorizontalVelocity(AtmosphereDrift.RainWindChosenBy(random));
            AssertEqual(expected, actual, "Rain complete RNG selection domain");
        }
    }
}
