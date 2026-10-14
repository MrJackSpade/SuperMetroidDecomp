using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Compares compiled rain velocities with the four retail words and their full random-selection domain.</summary>
internal static class LookupRainVelocityVerification
{
    /// <summary>Checks all four entries, 65,536 native RNG selectors, and out-of-range rejection.</summary>
    internal static void VerifyRainHorizontalVelocity(SuperMetroidAddressSpace rom)
    {
        for (int selection = 0; selection < 4; selection++)
            AssertEqual(ReadVerificationWord(rom, 0x88d992 + 2 * selection),
                RoomFxRomData.Rain.HorizontalVelocity(selection), "Rain signed 8.8 velocity");
        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            // Native LSR / AND 0006 produces a byte offset, independently of the
            // caller's word-index expression. Read the original word, not a formula.
            int nativeOffset = (random >> 1) & 6;
            ushort expected = ReadVerificationWord(rom, 0x88d992 + nativeOffset);
            ushort actual = RoomFxRomData.Rain.HorizontalVelocity((random >> 2) & 3);
            AssertEqual(expected, actual, "Rain complete RNG selection domain");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RoomFxRomData.Rain.HorizontalVelocity(invalid),
                "Rain rejects selectors outside original four-word span");
    }
}
