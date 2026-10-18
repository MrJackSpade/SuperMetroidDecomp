using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>
/// The presentation boundary of Dead Torizo's programs (#641): the bank-$A9 spritemap each
/// <see cref="DeadTorizoFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class DeadTorizoFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(DeadTorizoFrame frame) => frame.Pose switch
    {
        0 => 0xd6e2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Dead Torizo pose."),
    };

    /// <summary>The installed frame each <see cref="DeadTorizoFrame"/> shows.</summary>
    internal static EnemyFrameTable<DeadTorizoFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.DeadTorizoStationaryA9D6e2 },
    };
}
