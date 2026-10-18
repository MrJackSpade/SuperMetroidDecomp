namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of the ceiling-tube programs (#641): the installed artwork key of each tube's pose, which is
/// the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class MotherBrainTopTubeFrameDefinitions
{
    /// <summary>The installed artwork key of each tube's pose, in installed artwork order.</summary>
    internal static IReadOnlyDictionary<MotherBrainTopTubeProgram, ushort> PresentationKeys { get; } = new Dictionary<MotherBrainTopTubeProgram, ushort>
    {
        [MotherBrainTopTubeProgram.TopRight] = 0xcc45,
        [MotherBrainTopTubeProgram.TopLeft] = 0xcc4b,
        [MotherBrainTopTubeProgram.TopMiddleLeft] = 0xcc51,
        [MotherBrainTopTubeProgram.TopMiddleRight] = 0xcc57,
    };
}
