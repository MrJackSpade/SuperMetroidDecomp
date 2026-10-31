namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room flash (#641): fourteen color rows shown two updates each in a loop. Play runs the
/// typed flash by frame index; only extraction locates the rows in the cartridge.
/// </summary>
internal static class MotherBrainRoomPaletteProgramDefinitions
{
    /// <summary>The flash's fourteen two-tick frames, each showing one color row.</summary>
    internal const int PresentationWordCount = 14;

    /// <summary>$A9:D046-D07C, reviewed two-tick visual cadence for the fourteen-image flash performance; only the private palette timer consumes it.</summary>
    internal const int FlashImageTicks = 2;

    /// <summary>Reviewed visual strength sequence in thirds for the fourteen events at $A9:D046-$D07C; each returns a paint image without gameplay callbacks.</summary>
    internal static readonly int[] FlashStrengths = [0, 1, 2, 3, 0, 1, 0, 3, 1, 0, 3, 2, 0, 2];
}
