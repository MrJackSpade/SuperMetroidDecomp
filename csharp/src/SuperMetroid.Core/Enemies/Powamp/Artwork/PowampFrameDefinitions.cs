using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Powamp.Artwork;

/// <summary>
/// The presentation boundary of Powamp's programs (#641): the bank-$A8 spritemap each
/// <see cref="PowampFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class PowampFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PowampFrame frame) => frame.Pose switch
    {
        0 => 0xc675,
        1 => 0xc67c,
        2 => 0xc683,
        3 => 0xc68a,
        4 => 0xc691,
        5 => 0xc698,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Powamp pose."),
    };

    /// <summary>The installed frame each <see cref="PowampFrame"/> shows.</summary>
    internal static EnemyFrameTable<PowampFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.Powamp00 },
        { new(Pose: 1), EnemyFrameId.Powamp01 },
        { new(Pose: 2), EnemyFrameId.Powamp02 },
        { new(Pose: 3), EnemyFrameId.Powamp03 },
        { new(Pose: 4), EnemyFrameId.Powamp04 },
        { new(Pose: 5), EnemyFrameId.Powamp05 },
    };
}
