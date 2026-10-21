using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the escaping Etecoons' programs (#641): the bank-$B3 spritemap each
/// <see cref="EscapeEtecoonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EscapeEtecoonFrameDefinitions
{

    /// <summary>The installed frame each <see cref="EscapeEtecoonFrame"/> shows.</summary>
    internal static EnemyFrameTable<EscapeEtecoonFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.EscapeEtecoon00 },
        { new(Pose: 1), EnemyFrameId.EscapeEtecoon01 },
        { new(Pose: 2), EnemyFrameId.EscapeEtecoon02 },
        { new(Pose: 3), EnemyFrameId.EscapeEtecoon03 },
        { new(Pose: 4), EnemyFrameId.EscapeEtecoon04 },
        { new(Pose: 5), EnemyFrameId.EscapeEtecoon05 },
        { new(Pose: 6), EnemyFrameId.EscapeEtecoon06 },
        { new(Pose: 7), EnemyFrameId.EscapeEtecoon07 },
        { new(Pose: 8), EnemyFrameId.EscapeEtecoon09 },
        { new(Pose: 9), EnemyFrameId.EscapeEtecoon08 },
    };
}
