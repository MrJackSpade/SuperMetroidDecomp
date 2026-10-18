using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kzan.Artwork;

/// <summary>
/// The presentation boundary of Kzan's programs (#641): the bank-$A6 spritemap each
/// <see cref="KzanFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class KzanFrameDefinitions
{

    /// <summary>The installed frame each <see cref="KzanFrame"/> shows.</summary>
    internal static EnemyFrameTable<KzanFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.Kzan00 },
    };
}
