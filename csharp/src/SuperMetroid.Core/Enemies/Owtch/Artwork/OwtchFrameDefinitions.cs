using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Owtch.Artwork;

/// <summary>
/// The presentation boundary of Owtch's programs (#641): the bank-$A2 spritemap each
/// <see cref="OwtchFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class OwtchFrameDefinitions
{

    /// <summary>The installed frame each <see cref="OwtchFrame"/> shows.</summary>
    internal static EnemyFrameTable<OwtchFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.OwtchLeft0 },
        { new(Pose: 1), EnemyFrameId.OwtchLeft1 },
        { new(Pose: 2), EnemyFrameId.OwtchLeft2 },
    };
}
