namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room flash (#641): fourteen color rows shown two updates each in a loop. The bank-$A9
/// list at $A9:D046 only locates the rows for extraction; play runs the typed flash by frame index.
/// </summary>
internal static class MotherBrainRoomPaletteProgramDefinitions
{
    /// <summary><c>MotherBrain_FakeDeath_RoomPalette_InstructionList</c> at $A9:D046.</summary>
    public const ushort FlashStart = 0xd046;

    /// <summary>The final grey room palette at $A9:D082.</summary>
    public const ushort FinalPalette = 0xd082;

    /// <summary>$A9:D046-$D07C: fourteen two-tick frames, each with a palette operand.</summary>
    internal const int PresentationWordCount = 14;

    /// <summary>$A9:D046-D07C, reviewed two-tick visual cadence for the fourteen-image flash performance; only the private palette timer consumes it.</summary>
    internal const ushort FlashImageTicks = 2;

    /// <summary>Returns the palette operand two bytes after one of the fourteen timed fake-death flash entries, for extraction.</summary>
    internal static ushort PresentationWordAddress(int index) =>
        (uint)index < PresentationWordCount ? (ushort)(FlashStart + index * 4 + 2) :
            throw new IndexOutOfRangeException();

    /// <summary>Reviewed visual strength sequence in thirds for the fourteen events at $A9:D046-$D07C; each returns a paint image without gameplay callbacks.</summary>
    internal static int FlashStrength(int frame) => frame switch
    {
        1 or 5 or 8 => 1,
        2 or 11 or 13 => 2,
        3 or 7 or 10 => 3,
        0 or 4 or 6 or 9 or 12 => 0,
        _ => throw new IndexOutOfRangeException(),
    };
}
