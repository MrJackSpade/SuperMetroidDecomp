using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The presentation boundary of the Ceres steam vents' programs (#641): the bank-$A6 spritemap each
/// <see cref="CeresSteamFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CeresSteamFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CeresSteamFrame frame) => frame.Pose switch
    {
        0 => 0xf142,
        1 => 0xf14c,
        2 => 0xf156,
        3 => 0xf160,
        4 => 0xf16a,
        5 => 0xf174,
        6 => 0xf17e,
        7 => 0xf188,
        8 => 0xf192,
        9 => 0xf19c,
        10 => 0xf1a6,
        11 => 0xf1b0,
        12 => 0xf1ba,
        13 => 0xf1c4,
        14 => 0xf1ce,
        15 => 0xf1d8,
        16 => 0xf1e2,
        17 => 0xf1ec,
        18 => 0xf1f6,
        19 => 0xf200,
        20 => 0xf20a,
        21 => 0xf214,
        22 => 0xf21e,
        23 => 0xf228,
        24 => 0xf232,
        25 => 0xf23c,
        26 => 0xf246,
        27 => 0xf250,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ceres steam pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(CeresSteamFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.CeresSteamOamF142,
        { Pose: 1 } => EnemyFrameId.CeresSteamOamF14C,
        { Pose: 10 } => EnemyFrameId.CeresSteamOamF1A6,
        { Pose: 11 } => EnemyFrameId.CeresSteamOamF1B0,
        { Pose: 12 } => EnemyFrameId.CeresSteamOamF1BA,
        { Pose: 13 } => EnemyFrameId.CeresSteamOamF1C4,
        { Pose: 14 } => EnemyFrameId.CeresSteamOamF1CE,
        { Pose: 15 } => EnemyFrameId.CeresSteamOamF1D8,
        { Pose: 16 } => EnemyFrameId.CeresSteamOamF1E2,
        { Pose: 17 } => EnemyFrameId.CeresSteamOamF1EC,
        { Pose: 18 } => EnemyFrameId.CeresSteamOamF1F6,
        { Pose: 19 } => EnemyFrameId.CeresSteamOamF200,
        { Pose: 2 } => EnemyFrameId.CeresSteamOamF156,
        { Pose: 20 } => EnemyFrameId.CeresSteamOamF20A,
        { Pose: 21 } => EnemyFrameId.CeresSteamOamF214,
        { Pose: 22 } => EnemyFrameId.CeresSteamOamF21E,
        { Pose: 23 } => EnemyFrameId.CeresSteamOamF228,
        { Pose: 24 } => EnemyFrameId.CeresSteamOamF232,
        { Pose: 25 } => EnemyFrameId.CeresSteamOamF23C,
        { Pose: 26 } => EnemyFrameId.CeresSteamOamF246,
        { Pose: 27 } => EnemyFrameId.CeresSteamOamF250,
        { Pose: 3 } => EnemyFrameId.CeresSteamOamF160,
        { Pose: 4 } => EnemyFrameId.CeresSteamOamF16A,
        { Pose: 5 } => EnemyFrameId.CeresSteamOamF174,
        { Pose: 6 } => EnemyFrameId.CeresSteamOamF17E,
        { Pose: 7 } => EnemyFrameId.CeresSteamOamF188,
        { Pose: 8 } => EnemyFrameId.CeresSteamOamF192,
        { Pose: 9 } => EnemyFrameId.CeresSteamOamF19C,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the CeresSteamFrame programs show."),
    };
}
