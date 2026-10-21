using SuperMetroid.Core.Enemies.LavaJumpers;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Distinct OAM frame identities selected by the parent and follower lava-
/// jumper programs. The hidden frame is the bank-local empty spritemap;
/// movement, callback timing, and looping remain compiled gameplay logic.
/// </summary>
internal static class NorfairLavaJumperVisualDefinitions
{
    /// <summary>Native Norfair lava-jumper spritemap bank $A2.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>Eleven distinct poses include the bank-local hidden spritemap.</summary>
    internal const int FrameCount = 11;

    /// <summary>Deduplicates the frames the parent and follower programs (#641) show, in first-shown order.</summary>
    internal static NativeEnemySpritemapDefinition[] Frames()
    {
        var frames = NorfairLavaJumperAnimationsConstants.Spritemaps(Enum.GetValues<NorfairLavaJumperProgram>()).Distinct()
            .Select(pointer => new NativeEnemySpritemapDefinition(Bank, pointer, $"norfair_lava_jumper_{pointer:x4}")).ToArray();
        if (frames.Length != FrameCount)
            throw new InvalidDataException(
                $"Norfair lava jumper selects {frames.Length} distinct frames, expected {FrameCount}.");
        return frames;
    }
}
