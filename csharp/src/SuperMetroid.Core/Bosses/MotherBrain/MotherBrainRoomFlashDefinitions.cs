namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room flash (#641): fourteen images, each a <see cref="MotherBrainRoomFlashImage"/> (#2259),
/// shown two updates each in a loop; only extraction locates their colours.
/// </summary>
internal static class MotherBrainRoomFlashDefinitions
{
    /// <summary>The flash's fourteen two-tick frames, each showing one set of room colors.</summary>
    internal const int FlashFrameCount = 14;

    /// <summary>The updates each of the flash's fourteen images shows for; only the room flash's own timer counts them.</summary>
    internal const int FlashImageTicks = 2;
}
