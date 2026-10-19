using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Polyp.Artwork;

/// <summary>
/// The presentation boundary of Polyp's programs (#641): the bank-$A2 spritemap each
/// <see cref="PolypFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class PolypFrameDefinitions
{

    /// <summary>The installed frame each <see cref="PolypFrame"/> shows.</summary>
    internal static EnemyFrameTable<PolypFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.Polyp00 },
    };
}
