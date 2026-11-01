namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room flash (#641): fourteen color frames shown two updates each in a loop. Play runs the
/// typed flash by frame index; only extraction locates the frames in the original game.
/// </summary>
internal static class MotherBrainRoomPaletteProgramDefinitions
{
    /// <summary>The flash's fourteen two-tick frames, each showing one set of room colors.</summary>
    internal const int PresentationWordCount = 14;

    /// <summary>Reviewed two-tick visual cadence for the fourteen-image flash performance; only the private palette timer consumes it.</summary>
    internal const int FlashImageTicks = 2;

    /// <summary>Reviewed visual strength sequence in thirds for the fourteen events; each returns a paint image without gameplay callbacks.</summary>
    internal static readonly int[] FlashStrengths = [0, 1, 2, 3, 0, 1, 0, 3, 1, 0, 3, 2, 0, 2];
}
