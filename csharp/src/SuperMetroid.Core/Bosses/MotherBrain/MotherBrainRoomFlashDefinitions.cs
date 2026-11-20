namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room flash (#641): its images, each a <see cref="MotherBrainRoomFlashImage"/> (#2259),
/// shown in a loop (<see cref="MotherBrainRoomFlashAnimations"/>); only extraction locates their colours.
/// </summary>
internal static class MotherBrainRoomFlashDefinitions
{
    /// <summary>The flash's frames, each showing one set of room colors.</summary>
    internal const int FlashFrameCount = 14;

    /// <summary>The updates each of the flash's images shows for.</summary>
    internal const int FlashImageTicks = 2;
}
