using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Dragon.Artwork;

/// <summary>
/// The presentation boundary of Dragon's programs (#641): the bank-$A2 spritemap each
/// <see cref="DragonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class DragonFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(DragonFrame frame) => frame.Pose switch
    {
        0 => 0xe80c,
        1 => 0xe836,
        2 => 0xe860,
        3 => 0xe88a,
        4 => 0xe8b4,
        5 => 0xe8bb,
        6 => 0xe8c2,
        7 => 0xe8ec,
        8 => 0xe916,
        9 => 0xe940,
        10 => 0xe96a,
        11 => 0xe971,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Dragon pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(DragonFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.DragonBodyIdleLeft,
        { Pose: 1 } => EnemyFrameId.DragonBodyAttackLeft0,
        { Pose: 10 } => EnemyFrameId.DragonWingRight0,
        { Pose: 11 } => EnemyFrameId.DragonWingRight1,
        { Pose: 2 } => EnemyFrameId.DragonBodyAttackLeft1,
        { Pose: 3 } => EnemyFrameId.DragonBodyAttackLeft2,
        { Pose: 4 } => EnemyFrameId.DragonWingLeft0,
        { Pose: 5 } => EnemyFrameId.DragonWingLeft1,
        { Pose: 6 } => EnemyFrameId.DragonBodyIdleRight,
        { Pose: 7 } => EnemyFrameId.DragonBodyAttackRight0,
        { Pose: 8 } => EnemyFrameId.DragonBodyAttackRight1,
        { Pose: 9 } => EnemyFrameId.DragonBodyAttackRight2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the DragonFrame programs show."),
    };
}
