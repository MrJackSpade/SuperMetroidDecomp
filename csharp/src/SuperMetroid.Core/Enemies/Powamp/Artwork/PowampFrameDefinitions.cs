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

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(PowampFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.Powamp00,
        { Pose: 1 } => EnemyFrameId.Powamp01,
        { Pose: 2 } => EnemyFrameId.Powamp02,
        { Pose: 3 } => EnemyFrameId.Powamp03,
        { Pose: 4 } => EnemyFrameId.Powamp04,
        { Pose: 5 } => EnemyFrameId.Powamp05,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the PowampFrame programs show."),
    };
}
