using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="RioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class RioFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(RioFrame frame) => frame.Pose switch
    {
        0 => 0xbd6c,
        1 => 0xbd82,
        2 => 0xbd98,
        3 => 0xbdae,
        4 => 0xbdc4,
        5 => 0xbdda,
        6 => 0xbdf0,
        7 => 0xbe06,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Rio pose."),
    };

    /// <summary>The installed frame each <see cref="RioFrame"/> shows.</summary>
    internal static EnemyFrameTable<RioFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.RioBd6c },
        { new(Pose: 1), EnemyFrameId.RioBd82 },
        { new(Pose: 2), EnemyFrameId.RioBd98 },
        { new(Pose: 3), EnemyFrameId.RioBdae },
        { new(Pose: 4), EnemyFrameId.RioBdc4 },
        { new(Pose: 5), EnemyFrameId.RioBdda },
        { new(Pose: 6), EnemyFrameId.RioBdf0 },
        { new(Pose: 7), EnemyFrameId.RioBe06 },
    };
}
