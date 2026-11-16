namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room flash (#641): fourteen color frames shown two updates each in a loop. Play runs the
/// typed flash by frame index; only extraction locates the frames.
/// </summary>
internal static class MotherBrainRoomFlashDefinitions
{
    /// <summary>The flash's fourteen two-tick frames, each showing one set of room colors.</summary>
    internal const int FlashFrameCount = 14;

    /// <summary>The updates each of the flash's fourteen images shows for; only the room flash's own timer counts them.</summary>
    internal const int FlashImageTicks = 2;
}
